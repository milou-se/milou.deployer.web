using System;
using System.Globalization;
using System.Net;

namespace Milou.Deployer.Web.Tests.Integration
{
    internal readonly struct PortPoolRange
    {
        private readonly int _portCount;

        public PortPoolRange(int port, int portCount = 0)
        {
            _portCount = portCount;
            StartPort = port;
            if (portCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(portCount), "Port count must be a non-negative number");
            }

            if (port < IPEndPoint.MinPort)
            {
                throw new ArgumentOutOfRangeException(nameof(port),
                    $"Port must be a number between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}");
            }

            if (port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(nameof(port),
                    $"Port must be a number between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}");
            }

            if (port + portCount > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(nameof(portCount),
                    $"The last port number cannot be greater than {IPEndPoint.MaxPort}");
            }
        }

        public int StartPort { get; }

        public int EndPort => StartPort + _portCount;

        public string Format()
        {
            if (_portCount == 1)
            {
                return StartPort.ToString(CultureInfo.InvariantCulture);
            }

            return $"{StartPort}-{EndPort}";
        }
    }
}