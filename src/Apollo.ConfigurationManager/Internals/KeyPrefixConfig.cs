namespace Com.Ctrip.Framework.Apollo.Internals;

internal sealed class KeyPrefixConfig : ConfigWrapper
{
    private readonly string _keyPrefix;

    public KeyPrefixConfig(IConfig config, string keyPrefix) : base(config) => _keyPrefix = keyPrefix + ":";

    public override bool TryGetProperty(string key, [NotNullWhen(true)] out string? value) =>
        Config.TryGetProperty(_keyPrefix + key, out value);

    public override IEnumerable<string> GetPropertyNames() => Config.GetPropertyNames()
        .Where(propertyName => propertyName.Length > _keyPrefix.Length &&
            propertyName.StartsWith(_keyPrefix, StringComparison.OrdinalIgnoreCase))
        .Select(propertyName => propertyName[_keyPrefix.Length..]);
}
