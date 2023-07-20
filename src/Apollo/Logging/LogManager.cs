using Com.Ctrip.Framework.Apollo.Exceptions;
using Com.Ctrip.Framework.Apollo.Util;

namespace Com.Ctrip.Framework.Apollo.Logging;

public static class LogManager
{
    private static readonly Action<LogLevel, FormattableString, Exception?> Noop = (_, _, _) => { };

    public static Func<string, Action<LogLevel, FormattableString, Exception?>> LogFactory { get; set; } = _ => Noop;

    public static void UseConsoleLogging(LogLevel minimumLevel) =>
        LogFactory = name => (level, message, exception) =>
        {
            if (level < minimumLevel) return;

            Console.WriteLine(exception == null
                ? $"{DateTime.Now:HH:mm:ss} [{level}] {name} {message}"
                : $"{DateTime.Now:HH:mm:ss} [{level}] {name} {message} - {exception.GetDetailMessage()}");
        };

    internal static Action<LogLevel, FormattableString, Exception?> CreateLogger(Type type)
    {
        try
        {
            return LogFactory(type.FullName ?? type.ToString());
        }
        catch (Exception e)
        {
            Trace.TraceError(e.ToString());

            return Noop;
        }
    }

    internal static void Error(this Action<LogLevel, FormattableString, Exception?> logger,
        FormattableString message) => logger(LogLevel.Error, message, null);

    internal static void Error(this Action<LogLevel, FormattableString, Exception?> logger, Exception exception) =>
        logger(LogLevel.Error,
            exception is ApolloConfigException ace ? ace.FormattableMessage : $"{exception.Message}", exception);

    internal static void Error(this Action<LogLevel, FormattableString, Exception?> logger, FormattableString message,
        Exception exception) => logger(LogLevel.Error, message, exception);

    internal static void Warn(this Action<LogLevel, FormattableString, Exception?> logger, Exception exception) =>
        logger(LogLevel.Warning,
            exception is ApolloConfigException ace ? ace.FormattableMessage : $"{exception.Message}", exception);

    internal static void Warn(this Action<LogLevel, FormattableString, Exception?> logger, FormattableString message) =>
        logger(LogLevel.Warning, message, null);

    internal static void Warn(this Action<LogLevel, FormattableString, Exception?> logger, FormattableString message,
        Exception exception) => logger(LogLevel.Warning, message, exception);

    internal static void Debug(this Action<LogLevel, FormattableString, Exception?> logger,
        FormattableString message) => logger(LogLevel.Debug, message, null);
}
