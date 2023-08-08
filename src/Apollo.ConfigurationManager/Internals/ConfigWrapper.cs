namespace Com.Ctrip.Framework.Apollo.Internals;

internal abstract class ConfigWrapper : IConfig
{
    protected IConfig Config { get; }

    protected ConfigWrapper(IConfig config) => Config = config;

    public virtual bool TryGetProperty(string key, [NotNullWhen(true)] out string? value) =>
        Config.TryGetProperty(key, out value);

    public virtual IEnumerable<string> GetPropertyNames() => Config.GetPropertyNames();

    public event Action<IConfig>? ConfigChanged
    {
        add => Config.ConfigChanged += value;
        remove => Config.ConfigChanged -= value;
    }
}
