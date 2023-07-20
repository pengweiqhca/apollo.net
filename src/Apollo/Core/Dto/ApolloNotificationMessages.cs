namespace Com.Ctrip.Framework.Apollo.Core.Dto;

internal class ApolloNotificationMessages : ICloneable
{
    public IDictionary<string, long> Details { get; set; } = new Dictionary<string, long>();

    public void MergeFrom(ApolloNotificationMessages? source)
    {
        if (source == null) return;

        // to make sure the notification id always grows bigger
        foreach (var entry in source.Details)
            if (!Details.TryGetValue(entry.Key, out var value) || value < entry.Value) Details[entry.Key] = entry.Value;
    }

    public ApolloNotificationMessages Clone() => new() { Details = new Dictionary<string, long>(Details) };

    object ICloneable.Clone() => Clone();
}
