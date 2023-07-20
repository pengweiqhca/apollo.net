using System.Configuration;

namespace Com.Ctrip.Framework.Apollo.Internals;

internal static class ConfigExtensions
{
    public static IEnumerable<ConfigKey> GetChildren(this IConfig config, string keyPrefix)
    {
        var hash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(keyPrefix))
            foreach (var propertyName in config.GetPropertyNames())
            {
                var index = propertyName.IndexOf(':');

                hash.Add(index > 0 ? propertyName[..index] : propertyName);
            }
        else
        {
            keyPrefix += ":";

            foreach (var propertyName in config.GetPropertyNames())
            {
                if (!propertyName.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase)) continue;

                var index = propertyName.IndexOf(':', keyPrefix.Length);

                hash.Add(index > 0 ? propertyName[..index] : propertyName);
            }
        }

        return hash.Select(key => new ConfigKey(key[(key.LastIndexOf(':') + 1)..], key));
    }

    public static IEnumerable<ConnectionStringSettings> GetConnectionStrings(this IConfig config,
        string keyPrefix, string? defaultProviderName)
    {
        var keyPrefixAndColon = string.IsNullOrWhiteSpace(keyPrefix) ? keyPrefix = string.Empty : keyPrefix + ":";

        foreach (var name in config.GetChildren(keyPrefix))
        {
            var connectionName = keyPrefixAndColon + name.Name;

            if (!config.TryGetProperty($"{connectionName}:ConnectionString", out var connectionString) &&
                !config.TryGetProperty(connectionName, out connectionString) ||
                string.IsNullOrWhiteSpace(connectionString)) continue;

            config.TryGetProperty($"{connectionName}:ProviderName", out var providerName);

            yield return new(name.Name, connectionString, providerName ?? defaultProviderName);
        }
    }

    public static IConfig WithPrefix(this IConfig config, string? keyPrefix) =>
        string.IsNullOrWhiteSpace(keyPrefix) ? config : new KeyPrefixConfig(config, keyPrefix!);

    public record struct ConfigKey(string Name, string FullName);
}
