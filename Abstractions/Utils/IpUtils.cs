using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Abstractions.Utils;

public static class IpUtils
{
    public static string? GetLocalIPv4()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni =>
                ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .ToList();

        foreach (var ni in networkInterfaces)
        {
            var ipProps = ni.GetIPProperties();
            foreach (var addr in ipProps.UnicastAddresses)
            {
                if (addr.Address.AddressFamily != AddressFamily.InterNetwork ||
                    IPAddress.IsLoopback(addr.Address)) continue;
                var s = addr.Address.ToString();
                return s;
            }
        }

        return null;
    }

    private static readonly HttpClient HttpClient = new();
    private static string? _cachedIpv4;

    public static string GetIpv4()
    {
        return _cachedIpv4 ??= HttpClient.GetStringAsync("https://ifconfig.me/ip").GetAwaiter().GetResult().Trim();
    }
}
