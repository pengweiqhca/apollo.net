using Com.Ctrip.Framework.Apollo.Core.Utils;
using Com.Ctrip.Framework.Apollo.Logging;

namespace Com.Ctrip.Framework.Apollo.Internals;

internal sealed class DefaultConfig : AbstractConfig, IRepositoryChangeListener
{
    private static readonly Func<Action<LogLevel, FormattableString, Exception?>> Logger = () =>
        LogManager.CreateLogger(typeof(DefaultConfig));

    private readonly string _namespace;
    private readonly IConfigRepository _configRepository;
    private volatile Properties? _configProperties;

    public DefaultConfig(string namespaceName, IConfigRepository configRepository)
    {
        _namespace = namespaceName;
        _configRepository = configRepository;
    }

    public async Task Initialize()
    {
        await _configRepository.Initialize().ConfigureAwait(false);

        try
        {
            _configProperties = _configRepository.GetConfig();
        }
        catch (Exception ex)
        {
            Logger().Warn($"Init Apollo Local Config failed - namespace: {_namespace}", ex);
        }
        finally
        {
            // register the change listener no matter config repository is working or not
            // so that whenever config repository is recovered, config could get changed
            _configRepository.AddChangeListener(this);
        }
    }

    public override bool TryGetProperty(string key, [NotNullWhen(true)] out string? value)
    {
        value = _configProperties?.GetProperty(key);

        if (value == null)
            Logger().Debug(
                $"Could not load config {key} for namespace {_namespace} from Apollo, please check whether the configs are released in Apollo! Return default value now!");

        return value != null;
    }

    public Task OnRepositoryChange(string namespaceName, Properties newProperties)
    {
        FireConfigChange();

        return Task.CompletedTask;
    }

    public override IEnumerable<string> GetPropertyNames() =>
        _configProperties?.GetPropertyNames() ?? Enumerable.Empty<string>();
}
