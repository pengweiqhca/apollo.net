namespace Com.Ctrip.Framework.Apollo.Internals;

internal abstract class AbstractConfig : IConfig
{
    public event Action<IConfig>? ConfigChanged;

    public abstract bool TryGetProperty(string key, [NotNullWhen(true)] out string? value);

    public abstract IEnumerable<string> GetPropertyNames();

    protected void FireConfigChange()
    {
        if (ConfigChanged is { } configChanged) configChanged(this);
    }
}
