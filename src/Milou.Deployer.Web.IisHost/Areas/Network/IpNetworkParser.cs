﻿using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.HttpOverrides;

namespace Milou.Deployer.Web.IisHost.Areas.Network
{
    public static class IpNetworkParser
    {
        public static bool TryParse(string value, out IPNetwork network)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                network = default;
                return false;
            }

            var parts = value.Split("/");

            if (parts.Length != 2)
            {
                network = default;
                return false;
            }

            if (!IPAddress.TryParse(parts[0], out var address))
            {
                network = default;
                return false;
            }

            if (!int.TryParse(parts[1], out var length))
            {
                network = default;
                return false;
            }

            if (length < 0)
            {
                network = default;
                return false;
            }

            const int maxIpv4Length = 32;
            if (address.AddressFamily == AddressFamily.InterNetwork && length > maxIpv4Length)
            {
                network = default;
                return false;
            }

            network = new IPNetwork(address, length);

            return true;
        }
    }
}
