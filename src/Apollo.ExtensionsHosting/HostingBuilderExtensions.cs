using Com.Ctrip.Framework.Apollo;
using Microsoft.Extensions.Configuration;

#if Web
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;

namespace Microsoft.AspNetCore.Hosting;

#pragma warning disable SA1649
public static class WebHostingBuilderExtensions
#else
namespace Microsoft.Extensions.Hosting;

public static class HostingBuilderExtensions
#endif
{
    /// <param name="hostBuilder"></param>
    /// <param name="fromAppConfiguration">apollo config source. false: environment variables, commandline and so on, true：appsettings.json and so on</param>
    /// <param name="key">apollo config prefix</param>
    public static IHostBuilder AddApollo(this IHostBuilder hostBuilder, bool fromAppConfiguration = true,
        string key = "apollo")
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

        return fromAppConfiguration
            ? hostBuilder.ConfigureAppConfiguration((_, builder) => builder.AddApollo(builder.Build().GetSection(key)))
            : hostBuilder.ConfigureAppConfiguration((context, builder) =>
                builder.AddApollo(context.Configuration.GetSection(key)));
    }

    public static IHostBuilder AddApollo(this IHostBuilder hostBuilder, string appId, string metaServer)
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));
        if (appId == null) throw new ArgumentNullException(nameof(appId));
        if (metaServer == null) throw new ArgumentNullException(nameof(metaServer));

        return hostBuilder.ConfigureAppConfiguration((_, builder) => builder.AddApollo(appId, metaServer));
    }

    public static IHostBuilder AddApollo(this IHostBuilder hostBuilder, Action<ApolloOptions> configure)
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var options = new ApolloOptions();

        configure(options);

        return hostBuilder.ConfigureAppConfiguration((_, builder) => builder.AddApollo(options));
    }
}
