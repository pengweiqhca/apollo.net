namespace Com.Ctrip.Framework.Apollo.Exceptions;

public class ApolloConfigStatusCodeException : Exception
{
    public ApolloConfigStatusCodeException(HttpStatusCode statusCode, string message)
        : base($"[status code: {statusCode:D}] {message}")
    {
        StatusCode = statusCode;

        FormattableMessage = $"[status code: {statusCode:D}] {message}";
    }

    public virtual HttpStatusCode StatusCode { get; }

    public FormattableString FormattableMessage { get; }
}
