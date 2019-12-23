using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Arbor.App.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.AspNetCore.Host.Hosting
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