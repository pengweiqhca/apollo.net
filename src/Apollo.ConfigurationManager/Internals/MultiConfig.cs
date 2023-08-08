using Com.Ctrip.Framework.Apollo.Core.Utils;

namespace Com.Ctrip.Framework.Apollo.Internals;

internal sealed class MultiConfig : AbstractConfig, IDisposable
{
    private readonly IReadOnlyCollection<IConfig> _configs;
    private readonly SemaphoreSlim _waitHandle = new(1, 1);
    private Properties _configProperties;

    /// <param name="configs">order desc</param>
    public MultiConfig(IEnumerable<IConfig> configs)
    {
        if (configs == null) throw new ArgumentNullException(nameof(configs));

        _configs = configs.ToArray();

        foreach (var config in _configs)
            config.ConfigChanged += Config_ConfigChanged;

        _configProperties = CombineProperties();
    }

    private Properties CombineProperties()
    {
        var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var config in _configs)
        foreach (var name in config.GetPropertyNames())
            if (!dic.ContainsKey(name) && config.TryGetProperty(name, out var value))
                dic[name] = value;

        return new(dic);
    }

    public override bool TryGetProperty(string key, [NotNullWhen(true)] out string? value) =>
        _configProperties.TryGetProperty(key, out value);

    public override IEnumerable<string> GetPropertyNames() => _configProperties.GetPropertyNames();

    private void Config_ConfigChanged(IConfig config)
    {
        _waitHandle.Wait();

        try
        {
            _configProperties = CombineProperties();
        }
        finally
        {
            _waitHandle.Release();
        }

        FireConfigChange();
    }

    public void Dispose() => _waitHandle.Dispose();
}
