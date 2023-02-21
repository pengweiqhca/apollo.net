using Com.Ctrip.Framework.Apollo.Core;
using Com.Ctrip.Framework.Apollo.Internals;
using Com.Ctrip.Framework.Apollo.Spi;
using Com.Ctrip.Framework.Apollo.Util;

namespace Com.Ctrip.Framework.Apollo;

/// <summary>
/// Entry point for client config use
/// </summary>
internal static class ApolloConfigurationManager
{
    private static readonly bool ConfigEnablePlaceholder;
    private static readonly Exception? Exception;
    public static IConfigManager? Manager { get; }

    static ApolloConfigurationManager()
    {
        try
        {
            var config = new ConfigUtil();

            ConfigEnablePlaceholder = config.EnablePlaceholder;

            Manager = new DefaultConfigManager(new DefaultConfigRegistry(), new ConfigRepositoryFactory(config));
        }
        catch (Exception ex)
        {
            Exception = ex;
        }
    }

    /// <summary>
    /// Get Application's config instance. </summary>
    /// <returns> config instance </returns>
    public static Task<IConfig> GetAppConfig() => GetConfig(ConfigConsts.NamespaceApplication);

    /// <summary>
    /// Get the config instance for the namespace. </summary>
    /// <param name="namespaceName"> the namespace of the config </param>
    /// <returns> config instance </returns>
    public static Task<IConfig> GetConfig(string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName)) throw new ArgumentNullException(nameof(namespaceName));

        if (Exception != null) throw new InvalidOperationException("Apollo初始化异常", Exception);

        return Manager!.GetConfig(namespaceName);
    }

    /// <summary>
    /// Get the config instance for the namespace. </summary>
    /// <param name="namespaces"> the namespaces of the config, order desc. </param>
    /// <returns> config instance </returns>
    public static Task<IConfig> GetConfig(params string[] namespaces) => GetConfig((IEnumerable<string>)namespaces);

    /// <summary>
    /// Get the config instance for the namespace. </summary>
    /// <param name="namespaces"> the namespaces of the config, order desc. </param>
    /// <returns> config instance </returns>
    public static async Task<IConfig> GetConfig(IEnumerable<string> namespaces)
    {
        if (namespaces == null) throw new ArgumentNullException(nameof(namespaces));

        var configs = await Task.WhenAll(namespaces.Reverse().Distinct().Select(GetConfig)).ConfigureAwait(false);

        if (configs.Length < 1) throw new ArgumentException("namespaces not allow empty");

        var config = configs.Length == 1 ? configs[0] : new MultiConfig(configs);

        if (ConfigEnablePlaceholder) config = new PlaceholderConfig(config);

        return config;
    }
}
