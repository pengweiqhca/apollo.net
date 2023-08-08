using Com.Ctrip.Framework.Apollo.Core.Utils;

namespace Com.Ctrip.Framework.Apollo;

public interface ICacheFileProvider
{
    Task<Properties> Get(string configFile);

    Task Save(string configFile, Properties properties);
}

public class LocalPlaintextCacheFileProvider : ICacheFileProvider
{
    public async Task<Properties> Get(string configFile)
    {
        if (!File.Exists(configFile)) return new();

        using var reader = new FileStream(configFile, FileMode.Open);

        return await Properties.Read(reader).ConfigureAwait(false);
    }

    public async Task Save(string configFile, Properties properties)
    {
        using var file = new FileStream(configFile, FileMode.Create);

        await properties.ThrowIfNull().Store(file).ConfigureAwait(false);
    }
}
