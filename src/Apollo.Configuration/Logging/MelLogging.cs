﻿using Microsoft.Extensions.Logging;

namespace Com.Ctrip.Framework.Apollo.Logging;

public static class MelLogging
{
    public static void UseMel(ILoggerFactory loggerFactory)
    {
        loggerFactory.ThrowIfNull();
#pragma warning disable CA1848, CA2254
        LogManager.LogFactory = logger =>
            (level, msg, ex) => loggerFactory.CreateLogger(logger).Log(Convert(level), ex, msg.Format, msg.GetArguments());
    }

    private static Microsoft.Extensions.Logging.LogLevel Convert(LogLevel level) => level switch
    {
        LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
        LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
        LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
        LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
        LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
        LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
        _ => Microsoft.Extensions.Logging.LogLevel.None
    };
}
