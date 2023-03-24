using Com.Ctrip.Framework.Apollo.Internals;

namespace Com.Ctrip.Framework.Apollo;

public interface IApolloConfigurationBuilder : IConfigurationBuilder
{
    ConfigRepositoryFactory ConfigRepositoryFactory { get; }
}
