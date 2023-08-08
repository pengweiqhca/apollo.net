namespace Com.Ctrip.Framework.Apollo.Util;

internal static class QueryUtils
{
    public static string Build(IReadOnlyCollection<KeyValuePair<string, string?>>? source)
    {
        if (source == null || source.Count == 0) return string.Empty;

        var sb = new StringBuilder(source.Count * 32);

        foreach (var kv in source)
            sb.Append('&').Append(WebUtility.UrlEncode(kv.Key)).Append('=').Append(WebUtility.UrlEncode(kv.Value));

        return sb.ToString(1, sb.Length - 1);
    }
}
