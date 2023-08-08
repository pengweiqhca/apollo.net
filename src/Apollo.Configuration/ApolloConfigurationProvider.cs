using Com.Ctrip.Framework.Apollo.Core.Utils;
using Com.Ctrip.Framework.Apollo.Internals;

namespace Com.Ctrip.Framework.Apollo;

[DebuggerDisplay("apollo {" + nameof(ConfigRepository) + ("}[{" + nameof(SectionKey) + "}]"))]
internal sealed class ApolloConfigurationProvider : ConfigurationProvider, IRepositoryChangeListener, IDisposable
{
    internal string? SectionKey { get; }

    internal IConfigRepository ConfigRepository { get; }

    public ApolloConfigurationProvider(string? sectionKey, IConfigRepository configRepository)
    {
        SectionKey = sectionKey;
        ConfigRepository = configRepository;

        ConfigRepository.AddChangeListener(this);
    }

    public override void Load() => SetData(ConfigRepository.GetConfig());

    private void SetData(Properties properties)
    {
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in properties.ThrowIfNull().GetPropertyNames())
        {
            if (string.IsNullOrEmpty(SectionKey))
                data[key] = properties.GetProperty(key) ?? string.Empty;
            else
                data[SectionKey + ConfigurationPath.KeyDelimiter + key] = properties.GetProperty(key) ?? string.Empty;
        }

        Data = data;
    }

    Task IRepositoryChangeListener.OnRepositoryChange(string namespaceName, Properties newProperties)
    {
        SetData(newProperties);

        OnReload();

        return Task.CompletedTask;
    }

    public void Dispose() => ConfigRepository.RemoveChangeListener(this);
}
