namespace Com.Ctrip.Framework.Apollo.Internals;

public interface IConfigRepositoryFactory
{
#pragma warning disable CA1716
    IConfigRepository GetConfigRepository(string @namespace);
}
