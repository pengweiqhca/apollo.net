﻿using Com.Ctrip.Framework.Apollo.ConfigAdapter;
using Com.Ctrip.Framework.Apollo.Core;
using Com.Ctrip.Framework.Apollo.Core.Utils;
using Com.Ctrip.Framework.Apollo.Enums;
using Com.Ctrip.Framework.Apollo.Logging;

namespace Com.Ctrip.Framework.Apollo;

public interface IConfigAdapter
{
    Properties GetProperties(Properties properties);
}

public abstract class ContentConfigAdapter : IConfigAdapter
{
    private static readonly Func<Action<LogLevel, FormattableString, Exception?>> Logger = () => LogManager.CreateLogger(typeof(ContentConfigAdapter));

    public Properties GetProperties(Properties properties)
    {
        if (properties.ThrowIfNull().GetPropertyNames().Count < 1) return properties;

        var content = properties.GetProperty(ConfigConsts.ConfigFileContentKey);

        if (!string.IsNullOrWhiteSpace(content)) return GetProperties(content!);

        Logger().Warn($"Can not find {ConfigConsts.ConfigFileContentKey}");

        return properties;
    }

    public abstract Properties GetProperties(string content);
}

public static class ConfigAdapterRegister
{
    private static readonly IDictionary<ConfigFileFormat, IConfigAdapter> Dic;

    static ConfigAdapterRegister()
    {
        Dic = new Dictionary<ConfigFileFormat, IConfigAdapter>();

        AddAdapter(ConfigFileFormat.Json, new JsonConfigAdapter());
        AddAdapter(ConfigFileFormat.Xml, new XmlConfigAdapter());
    }

    public static void AddAdapter(ConfigFileFormat format, IConfigAdapter adapter) =>
        Dic[format] = adapter ?? throw new ArgumentNullException(nameof(adapter));

    internal static bool TryGetAdapter(ConfigFileFormat format, [NotNullWhen(true)] out IConfigAdapter? adapter) =>
        Dic.TryGetValue(format, out adapter);
}
