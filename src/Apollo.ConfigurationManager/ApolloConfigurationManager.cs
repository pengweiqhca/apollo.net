using Com.Ctrip.Framework.Apollo.Core;
using Com.Ctrip.Framework.Apollo.Internals;
using Com.Ctrip.Framework.Apollo.Util;
using System.Runtime.ExceptionServices;

namespace Com.Ctrip.Framework.Apollo;

internal static class ApolloConfigurationManager
{
    private static readonly bool ConfigEnablePlaceholder;
    private static readonly ExceptionDispatchInfo? Exception;
    private static readonly ConfigManager? Manager;

    static ApolloConfigurationManager()
    {
        try
        {
            var config = new ConfigUtil();

            ConfigEnablePlaceholder = config.EnablePlaceholder;

            Manager = new(new ConfigRepositoryFactory(config));
        }
        catch (Exception ex)
        {
            Exception = ExceptionDispatchInfo.Capture(ex);
        }
    }

    public static async Task<IConfig> GetConfig(IReadOnlyList<string>? namespaces)
    {
        Exception?.Throw();

        var config = namespaces == null || namespaces.Count < 1
            ? await Manager!.GetConfig(ConfigConsts.NamespaceApplication).ConfigureAwait(false)
            : namespaces.Count == 1
                ? await Manager!.GetConfig(namespaces[0]).ConfigureAwait(false)
                : new MultiConfig(await Task.WhenAll(namespaces.Reverse().Distinct().Select(Manager!.GetConfig))
                    .ConfigureAwait(false));

        return ConfigEnablePlaceholder ? new PlaceholderConfig(config) : config;
    }
}
