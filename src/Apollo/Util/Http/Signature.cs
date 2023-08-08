using System.Globalization;
using System.Security.Cryptography;

namespace Com.Ctrip.Framework.Apollo.Util.Http;

internal static class Signature
{
    public static long GetTimeStamp()
    {
#if NETFRAMEWORK
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
#else
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();
#endif
    }

    public static IDictionary<string, string> BuildHttpHeaders(Uri url, string appId, string secret)
    {
        var timestamp = GetTimeStamp().ToString(CultureInfo.InvariantCulture);

        return new Dictionary<string, string>
        {
            {
                "Authorization", $"Apollo {appId}:{SignString(timestamp + "\n" + url.PathAndQuery, secret)}"
            },
            {
                "Timestamp", timestamp
            }
        };
    }

    public static string SignString(string data, string secret)
    {
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret));

        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }
}
