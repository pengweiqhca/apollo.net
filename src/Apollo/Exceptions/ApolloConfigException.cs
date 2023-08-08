using System.Globalization;

namespace Com.Ctrip.Framework.Apollo.Exceptions;

public class ApolloConfigException : Exception
{
    public ApolloConfigException(FormattableString message) : base(message.ThrowIfNull()
        .ToString(CultureInfo.InvariantCulture)) => FormattableMessage = message;

    public ApolloConfigException(FormattableString message, Exception ex) : base(
        message.ThrowIfNull().ToString(CultureInfo.InvariantCulture), ex) =>
        FormattableMessage = message;

    public FormattableString FormattableMessage { get; }
}
