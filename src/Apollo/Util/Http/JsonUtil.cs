
using System.Text.Json.Serialization;

namespace Com.Ctrip.Framework.Apollo.Util.Http;

internal static class JsonUtil
{
    private static readonly JsonSerializerOptions JsonSettings = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serialize<T>(T? obj) => JsonSerializer.Serialize(obj, JsonSettings);
}
