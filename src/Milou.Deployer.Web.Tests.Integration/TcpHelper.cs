using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Tests.Integration
{
    internal static class TcpHelper
    {
        private static ConcurrentDictionary<int, PortPoolRental> _rentals =
            new ConcurrentDictionary<int, PortPoolRental>();

        public static PortPoolRental GetAvailablePort(in PortPoolRange range, IEnumerable<int> exludes = null)
        {
            List<int> excluded = (exludes ?? new List<int>()).ToList();

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            for (int port = range.StartPort; port <= range.EndPort; port++)
            {
                bool portIsInUse = tcpConnInfoArray.Any(tcpPort => tcpPort.LocalEndPoint.Port == port);

                if (!_rentals.ContainsKey(port) && !portIsInUse && !excluded.Any(excludedPort => excludedPort == port))
                {
                    var portPoolRental = new PortPoolRental(port, Return);

                    _rentals.TryAdd(port, portPoolRental);

                    return portPoolRental;
                }
            }

            throw new Core.DeployerAppException($"Could not find any TCP port in range {range.Format()}");
        }

        public static void Return([NotNull] PortPoolRental rental)
        {
            if (rental == null)
            {
                throw new ArgumentNullException(nameof(rental));
            }

            _rentals.TryRemove(rental.Port, out _);
        }
    }
}