namespace Com.Ctrip.Framework.Apollo.Core.Dto;

internal class ApolloConfigNotification
{
    public string NamespaceName { get; set; } = default!;

    public long NotificationId { get; set; }

    public ApolloNotificationMessages? Messages { get; set; }

    public override string ToString() => $"ApolloConfigNotification{{namespaceName='{NamespaceName}{'\''}, notificationId={NotificationId}{'}'}";
}
