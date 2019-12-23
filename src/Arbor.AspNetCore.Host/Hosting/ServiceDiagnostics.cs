using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Arbor.App.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.AspNetCore.Host.Hosting
{
    public class ServiceDiagnostics
    {
        private ServiceDiagnostics(IEnumerable<ServiceRegistrationInfo> registrations) =>
            Registrations = registrations.SafeToImmutableArray();

        public ImmutableArray<ServiceRegistrationInfo> Registrations { get; }

        public static ServiceDiagnostics Create(IServiceCollection services)
        {
            IEnumerable<ServiceRegistrationInfo> registrations = services.Select(ServiceRegistrationInfo.Create);

            return new ServiceDiagnostics(registrations);
        }
    }
}