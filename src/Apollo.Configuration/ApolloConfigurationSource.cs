using Com.Ctrip.Framework.Apollo.Internals;

namespace Com.Ctrip.Framework.Apollo;

[DebuggerDisplay("apollo {" + nameof(ConfigRepository) + ("}[{" + nameof(SectionKey) + "}]"))]
internal class ApolloConfigurationSource : IConfigurationSource
{
    public string? SectionKey { get; }

    public IConfigRepository ConfigRepository { get; }

    private Task? _initializeTask;

    public ApolloConfigurationSource(string? sectionKey, IConfigRepository configRepository)
    {
        SectionKey = sectionKey;
        ConfigRepository = configRepository;

        _initializeTask = configRepository.Initialize();
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        Interlocked.Exchange(ref _initializeTask, null)?.ConfigureAwait(false).GetAwaiter().GetResult();

        return new ApolloConfigurationProvider(SectionKey, ConfigRepository);
    }

    public override string ToString() => string.IsNullOrEmpty(SectionKey)
        ? $"apollo {ConfigRepository}"
        : $"apollo {ConfigRepository}[{SectionKey}]";
}
