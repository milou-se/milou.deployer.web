using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;

namespace Milou.Deployer.Web.Tests.Integration
{
    internal static class TcpHelper
    {
        private static readonly ConcurrentDictionary<int, PortPoolRental> Rentals =
            new ConcurrentDictionary<int, PortPoolRental>();

        private static void Return([NotNull] PortPoolRental rental)
        {
            if (rental == null)
            {
                throw new ArgumentNullException(nameof(rental));
            }

            Rentals.TryRemove(rental.Port, out _);
        }

        public static PortPoolRental GetAvailablePort(in PortPoolRange range, IEnumerable<int> excludes = null)
        {
            var excluded = (excludes ?? new List<int>()).ToList();

            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var activeTcpConnections = ipGlobalProperties.GetActiveTcpConnections();

            var random = new Random();
            for (var attempt = 0; attempt < 50; attempt++)
            {
                var port = random.Next(range.StartPort, range.EndPort);

                var portIsInUse = activeTcpConnections.Any(tcpPort => tcpPort.LocalEndPoint.Port == port);

                if (!Rentals.ContainsKey(port) && !portIsInUse && !excluded.Any(excludedPort => excludedPort == port))
                {
                    var portPoolRental = new PortPoolRental(port, Return);

                    if (Rentals.TryAdd(port, portPoolRental))
                    {
                        return portPoolRental;
                    }
                }
            }

            throw new DeployerAppException($"Could not find any TCP port in range {range.Format()}");
        }
    }
}
