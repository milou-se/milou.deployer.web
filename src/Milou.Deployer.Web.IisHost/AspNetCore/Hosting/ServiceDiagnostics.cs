using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Hosting
{
    public class ServiceDiagnostics
    {
        public ImmutableArray<ServiceRegistrationInfo> Registrations { get; }

        private ServiceDiagnostics(IEnumerable<ServiceRegistrationInfo> registrations)
        {
            Registrations = registrations.SafeToImmutableArray();
        }

        public static ServiceDiagnostics Create(IServiceCollection services)
        {
            IEnumerable<ServiceRegistrationInfo> registrations = services.Select(ServiceRegistrationInfo.Create);

            return new ServiceDiagnostics(registrations);
        }
    }
}