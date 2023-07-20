namespace Com.Ctrip.Framework.Apollo.Core.Dto;

internal sealed class ApolloConfigNotification
{
    public string? NamespaceName { get; set; }

    public long NotificationId { get; set; }

    public ApolloNotificationMessages? Messages { get; set; }
}
