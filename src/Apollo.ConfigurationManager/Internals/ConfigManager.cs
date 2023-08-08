namespace Com.Ctrip.Framework.Apollo.Internals;

internal sealed class ConfigManager : IDisposable
{
    private readonly Dictionary<string, IConfig> _configs = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IConfigRepositoryFactory _repositoryFactory;

    public ConfigManager(IConfigRepositoryFactory repositoryFactory) => _repositoryFactory = repositoryFactory;

    public async Task<IConfig> GetConfig(string namespaceName)
    {
        if (_configs.TryGetValue(namespaceName, out var config)) return config;

        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (!_configs.TryGetValue(namespaceName, out config))
                _configs[namespaceName] = config = await Create(namespaceName).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }

        return config;
    }

    private async Task<IConfig> Create(string namespaceName)
    {
        var configRepository = _repositoryFactory.GetConfigRepository(namespaceName);

        var config = new DefaultConfig(namespaceName, configRepository);

        await config.Initialize().ConfigureAwait(false);

        return config;
    }

    public void Dispose() => _semaphore.Dispose();
}
