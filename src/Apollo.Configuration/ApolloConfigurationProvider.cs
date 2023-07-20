using Com.Ctrip.Framework.Apollo.Core.Utils;
using Com.Ctrip.Framework.Apollo.Internals;

namespace Com.Ctrip.Framework.Apollo;

[DebuggerDisplay("apollo {" + nameof(ConfigRepository) + "}")]
internal sealed class ApolloConfigurationProvider
    : ConfigurationProvider, IRepositoryChangeListener, IConfigurationSource, IDisposable
{
    internal IConfigRepository ConfigRepository { get; }

    private Task? _initializeTask;
    private int _buildCount;

    public ApolloConfigurationProvider(IConfigRepository configRepository)
    {
        ConfigRepository = configRepository;

        ConfigRepository.AddChangeListener(this);

        _initializeTask = ConfigRepository.Initialize();
    }

    public override void Load()
    {
        Interlocked.Exchange(ref _initializeTask, null)?.GetAwaiter().GetResult();

        SetData(ConfigRepository.GetConfig());
    }

    private void SetData(Properties properties)
    {
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in properties.ThrowIfNull().GetPropertyNames())
            data[key] = properties.GetProperty(key) ?? string.Empty;

        Data = data;
    }

    Task IRepositoryChangeListener.OnRepositoryChange(string namespaceName, Properties newProperties)
    {
        SetData(newProperties);

        OnReload();

        return Task.CompletedTask;
    }

    IConfigurationProvider IConfigurationSource.Build(IConfigurationBuilder builder)
    {
        Interlocked.Increment(ref _buildCount);

        return this;
    }

    public void Dispose()
    {
        if (Interlocked.Decrement(ref _buildCount) == 0) ConfigRepository.RemoveChangeListener(this);
    }
}
