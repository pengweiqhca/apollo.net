using Com.Ctrip.Framework.Apollo.Logging;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Com.Ctrip.Framework.Apollo.Foundation;

public class NetworkInterfaceManager
{
    private static readonly string[] HostIps = default!;

    static NetworkInterfaceManager()
    {
        try
        {
            HostIps = NetworkInterface.GetAllNetworkInterfaces()
                .Where(network => network.OperationalStatus == OperationalStatus.Up)
                .Select(network => network.GetIPProperties())
                .OrderByDescending(properties => properties.GatewayAddresses.Count)
                .SelectMany(properties => properties.UnicastAddresses)
                .Where(address => !IPAddress.IsLoopback(address.Address) && address.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(address => address.Address.ToString())
                .ToArray();

            if (HostIps.Length > 0)
            {
                HostIp = HostIps.First();
            }
        }
        catch
        {
            HostIps = Array.Empty<string>();
        }
    }

    public static string HostIp { get; } = "127.0.0.1";

    public static string GetHostIp(IReadOnlyCollection<string>? preferSubnet)
    {
        if (preferSubnet == null || preferSubnet.Count < 1) return HostIp;

        try
        {
            if (IsInSubnet(HostIps, preferSubnet, out var ip)) return ip;
        }
        catch (Exception ex)
        {
            LogManager.CreateLogger(typeof(NetworkInterfaceManager)).Error($"Can not get local ip address with prefer option '{preferSubnet}'.", ex);
        }

        return HostIp;
    }

    internal static bool IsInSubnet(string[] ips, IEnumerable<string> cidrs, [NotNullWhen(true)] out string? matchedIp)
    {
        foreach (var cidr in cidrs)
        {
            if (string.IsNullOrEmpty(cidr)) continue;

            foreach (var ip in ips)
            {
                if (string.IsNullOrEmpty(ip)) continue;

                if (IsInSubnet(ip, cidr))
                {
                    matchedIp = ip;
                    return true;
                }
            }
        }

        matchedIp = default;

        return false;
    }

    private static bool IsInSubnet(string ipAddress, string cidr)
    {
        var parts = cidr.Split('/');

        var baseAddress = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);

        var address = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);

        var mask = IPAddress.HostToNetworkOrder(-1 << 32 - int.Parse(parts[1]));

        return (baseAddress & mask) == (address & mask);
    }
}
