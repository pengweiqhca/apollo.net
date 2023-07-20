namespace Com.Ctrip.Framework.Apollo.Internals;

internal class PlaceholderConfig : ConfigWrapper
{
    public PlaceholderConfig(IConfig config) : base(config) { }

    public override bool TryGetProperty(string key, [NotNullWhen(true)] out string? value)
    {
        if (!Config.TryGetProperty(key, out value)) return false;

        value = this.ResolvePlaceholders(value);

        return true;
    }
}
