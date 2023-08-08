using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Core;
using Com.Ctrip.Framework.Apollo.Core.Utils;
using Com.Ctrip.Framework.Apollo.Internals;
using Xunit;

namespace Apollo.ConfigurationManager.Tests;

public class ConfigTest
{
    [Fact]
    public async Task SingleNamespaceTest()
    {
        var repositoryFactory = new FakeConfigRepository(ConfigConsts.NamespaceApplication,
            new(new Dictionary<string, string> { ["A"] = "3" }));

        var config = await CreateConfig(repositoryFactory).ConfigureAwait(false);

        Assert.Equal("3", GetProperty(config, "A", ""));

        IConfig? args = null;
        config.ConfigChanged += sender => args = sender;

        await repositoryFactory.Change(new(new Dictionary<string, string>()));

        await Task.Delay(100).ConfigureAwait(false);

        Assert.NotNull(args);
    }

    [Fact]
    public async Task MultipleNamespaceTest()
    {
        var repositoryFactories = new[]
        {
            new FakeConfigRepository(ConfigConsts.NamespaceApplication,
                new(new Dictionary<string, string> { ["A"] = "3", ["B"] = "3" })),
            new FakeConfigRepository(ConfigConsts.NamespaceApplication,
                new(new Dictionary<string, string> { ["B"] = "4" })),
        };

        var config = new MultiConfig(await Task.WhenAll(repositoryFactories.Select(CreateConfig).Reverse())
            .ConfigureAwait(false));

        Assert.Equal("3", GetProperty(config, "A", ""));
        Assert.Equal("4", GetProperty(config, "B", ""));

        IConfig? args = null;
        config.ConfigChanged += sender => args = sender;

        await repositoryFactories[1].Change(new(new Dictionary<string, string>()));

        await Task.Delay(100).ConfigureAwait(false);

        Assert.NotNull(args);

        args = null;

        await repositoryFactories[1].Change(new(new Dictionary<string, string> { ["B"] = "3" }));

        await Task.Delay(100).ConfigureAwait(false);

        Assert.Null(args);
    }

    public static string GetProperty(IConfig config, string key, string defaultValue) =>
        config.TryGetProperty(key, out var value) ? value : defaultValue;

    private static Task<IConfig> CreateConfig(FakeConfigRepository repositoryFactory) =>
        new ConfigManager(repositoryFactory).GetConfig(repositoryFactory.Namespace);

    private class FakeConfigRepository : AbstractConfigRepository, IConfigRepositoryFactory
    {
        private Properties _properties;

        public FakeConfigRepository(string @namespace, Properties properties) : base(@namespace) =>
            _properties = properties;

        public override Properties GetConfig() => _properties;

        public IConfigRepository GetConfigRepository(string @namespace) => this;

        public override Task Initialize() => Task.CompletedTask;

        protected override void Dispose(bool disposing) { }

        public Task Change(Properties properties)
        {
            _properties = properties;

            return FireRepositoryChange(Namespace, properties);
        }
    }
}
