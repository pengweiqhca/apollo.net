﻿using Com.Ctrip.Framework.Apollo.Core.Dto;
using Com.Ctrip.Framework.Apollo.Core.Utils;
using Com.Ctrip.Framework.Apollo.Exceptions;
using Com.Ctrip.Framework.Apollo.Logging;
using Com.Ctrip.Framework.Apollo.Util;
using Com.Ctrip.Framework.Apollo.Util.Http;
using System.Runtime.ExceptionServices;
using System.Web;

namespace Com.Ctrip.Framework.Apollo.Internals;

internal class RemoteConfigRepository : AbstractConfigRepository
{
    private static readonly Func<Action<LogLevel, string, Exception?>> Logger = () => LogManager.CreateLogger(typeof(RemoteConfigRepository));
    private static readonly TaskFactory ExecutorService = new(new LimitedConcurrencyLevelTaskScheduler(5));

    private readonly ConfigServiceLocator _serviceLocator;
    private readonly HttpUtil _httpUtil;
    private readonly IApolloOptions _options;
    private readonly RemoteConfigLongPollService _remoteConfigLongPollService;
    private readonly Timer _timer;

    private volatile ApolloConfig? _configCache;
    private volatile ServiceDto? _longPollServiceDto;
    private volatile ApolloNotificationMessages? _remoteMessages;
    private ExceptionDispatchInfo? _syncException;

    public RemoteConfigRepository(string @namespace,
        IApolloOptions configUtil,
        HttpUtil httpUtil,
        ConfigServiceLocator serviceLocator,
        RemoteConfigLongPollService remoteConfigLongPollService) : base(@namespace)
    {
        _options = configUtil;
        _httpUtil = httpUtil;
        _serviceLocator = serviceLocator;
        _remoteConfigLongPollService = remoteConfigLongPollService;

        _timer = new(SchedulePeriodicRefresh);
    }

    public override async Task Initialize()
    {
        await SchedulePeriodicRefresh(true).ConfigureAwait(false);

        _timer.Change(_options.RefreshInterval, _options.RefreshInterval);

        _remoteConfigLongPollService.Submit(Namespace, this);
    }

    public override Properties GetConfig()
    {
        _syncException?.Throw();

        return TransformApolloConfigToProperties(_configCache);
    }

    private async void SchedulePeriodicRefresh(object? state) => await SchedulePeriodicRefresh(false).ConfigureAwait(false);

    private async Task SchedulePeriodicRefresh(bool isFirst)
    {
        try
        {
            Logger().Debug($"refresh config for namespace: {Namespace}");

            await Sync(isFirst).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _syncException = ExceptionDispatchInfo.Capture(ex);

            Logger().Warn($"refresh config error for namespace: {Namespace}", ex);
        }
    }

    private async Task Sync(bool isFirst)
    {
        var previous = _configCache;
        var current = await LoadApolloConfig(isFirst).ConfigureAwait(false);

        // reference equals means HTTP 304
        if (ReferenceEquals(previous, current)) return;

        Logger().Debug("Remote Config refreshed!");
        _configCache = current;
        _syncException = null;
        FireRepositoryChange(Namespace, GetConfig());
    }

    private async Task<ApolloConfig?> LoadApolloConfig(bool isFirst)
    {
        var appId = _options.AppId;
        var cluster = _options.Cluster;
        var dataCenter = _options.DataCenter;

        var configServices = await _serviceLocator.GetConfigServices().ConfigureAwait(false);

        Exception? exception = null;
        Uri? url = null;

        var notFound = false;
        for (var i = 0; i < (isFirst ? 1 : 2); i++)
        {
            IList<ServiceDto> randomConfigServices = configServices.OrderBy(_ => Guid.NewGuid()).ToList();

            // Access the server which notifies the client first
            var longPollServiceDto = Interlocked.Exchange(ref _longPollServiceDto, null);
            if (longPollServiceDto != null)
                randomConfigServices.Insert(0, longPollServiceDto);

            foreach (var configService in randomConfigServices)
            {
                url = AssembleQueryConfigUrl(configService.HomepageUrl, appId, cluster, Namespace, dataCenter, _remoteMessages, _configCache);

                Logger().Debug($"Loading config from {url}");

                try
                {
                    var response = await _httpUtil.DoGetAsync<ApolloConfig?>(url).ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        Logger().Debug("Config server responds with 304 HTTP status code.");
                        return _configCache!;
                    }

                    var result = response.Body;

                    Logger().Debug($"Loaded config for {Namespace}: {result?.Configurations?.Count ?? 0}");

                    return result;
                }
                catch (ApolloConfigStatusCodeException ex)
                {
                    var statusCodeException = ex;

                    // config not found
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        notFound = true;

                        var message = $"Could not find config for namespace - appId: {appId}, cluster: {cluster}, namespace: {Namespace}, please check whether the configs are released in Apollo!";
                        statusCodeException = new(ex.StatusCode, message);
                    }

                    Logger().Warn(statusCodeException);
                    exception = statusCodeException;
                }
                catch (Exception ex)
                {
                    Logger().Warn("Load apollo config fail from " + configService, ex);

                    exception = ex;
                }
            }

            await Task.Delay(1000).ConfigureAwait(false);
        }

        if (notFound)
            return null;

        var fallbackMessage = $"Load Apollo Config failed - appId: {appId}, cluster: {cluster}, namespace: {Namespace}, url: {url}";

        throw new ApolloConfigException(fallbackMessage, exception!);
    }

    private Uri AssembleQueryConfigUrl(string uri,
        string appId,
        string cluster,
        string? namespaceName,
        string? dataCenter,
        ApolloNotificationMessages? remoteMessages,
        ApolloConfig? previousConfig)
    {
        if (!uri.EndsWith("/", StringComparison.Ordinal))
            uri += "/";

        // Looks like .Net will handle all the url encoding for me...
        var path = $"configs/{appId}/{cluster}/{namespaceName}";
        var uriBuilder = new UriBuilder(uri + path);
#if NETFRAMEWORK
        // 不要使用HttpUtility.ParseQueryString()，.NET Framework里会死锁
        var query = new Dictionary<string, string>();
#else
        var query = HttpUtility.ParseQueryString(string.Empty);
#endif
        if (previousConfig != null)
            query["releaseKey"] = previousConfig.ReleaseKey;

        if (!string.IsNullOrEmpty(dataCenter))
            query["dataCenter"] = dataCenter!;

        var localIp = _options.LocalIp;
        if (!string.IsNullOrEmpty(localIp))
            query["ip"] = localIp;

        if (remoteMessages != null)
            query["messages"] = JsonUtil.Serialize(remoteMessages);
#if NETFRAMEWORK
        uriBuilder.Query = QueryUtils.Build(query);
#else
        uriBuilder.Query = query.ToString();
#endif
        return uriBuilder.Uri;
    }

    private static Properties TransformApolloConfigToProperties(ApolloConfig? apolloConfig) =>
        apolloConfig?.Configurations == null ? new() : new Properties(apolloConfig.Configurations);

    public void OnLongPollNotified(ServiceDto longPollNotifiedServiceDto, ApolloNotificationMessages remoteMessages)
    {
        _longPollServiceDto = longPollNotifiedServiceDto;
        _remoteMessages = remoteMessages;

        ExecutorService.StartNew(async () =>
        {
            try
            {
                await Sync(false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger().Warn($"Sync config failed, will retry. Repository {GetType()}, reason: {ex.GetDetailMessage()}");
            }
        });
    }

    private bool _disposed;

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _timer.Dispose();

        // 释放非托管资源
        _disposed = true;
    }

    public override string ToString() => $"remote {_options.AppId} {Namespace}";
}
