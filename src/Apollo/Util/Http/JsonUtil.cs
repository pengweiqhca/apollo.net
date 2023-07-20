using System.Text.Json.Serialization;

namespace Com.Ctrip.Framework.Apollo.Util.Http;

internal static class JsonUtil
{
    internal static readonly JsonSerializerOptions JsonSettings = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serialize<T>(T? obj) => JsonSerializer.Serialize(obj, JsonSettings);
}
