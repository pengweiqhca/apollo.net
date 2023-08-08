namespace Com.Ctrip.Framework.Apollo.Core.Dto;

internal sealed class ApolloConfig
{
    public string? AppId { get; set; }

    public string? Cluster { get; set; }

    public string? NamespaceName { get; set; }

    public string? ReleaseKey { get; set; }

    public IDictionary<string, string>? Configurations { get; set; }
}
