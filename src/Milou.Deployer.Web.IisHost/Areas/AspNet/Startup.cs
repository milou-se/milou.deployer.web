using System.Collections.Generic;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Configuration.AutofacConfiguration;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.NuGet;
using Milou.Deployer.Web.IisHost.Areas.Security;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.AspNet
{
    public class Startup
    {
        [UsedImplicitly]
        public void ConfigureContainer([NotNull] AutofacOptions autofacOptions)
        {
            IServiceCollection services = autofacOptions.Services;

            IComponentContext rootScope = autofacOptions.ComponentContext;

            var deploymentTargetReadService = rootScope.Resolve<IDeploymentTargetReadService>();

            string[] enumerables = deploymentTargetReadService.GetOrganizationsAsync().Result.SelectMany(o => o.Projects.SelectMany(p => p.DeploymentTargets.Select(t => t.Id))).ToArray();


            var deploymentService = rootScope.Resolve<DeploymentService>();

            var logger = rootScope.Resolve<ILogger>();

            var workers = new List<DeploymentTargetWorker>();
            foreach (string targetId in enumerables)
            {
               var worker = new DeploymentTargetWorker(targetId, deploymentService,logger);

                workers.Add(worker);

                services.Add(new ServiceDescriptor(typeof(IHostedService), worker));
            }

            var deploymentWorker = new DeploymentWorker(workers);

            services.AddSingleton(deploymentWorker);

            services.AddMvc();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.IPOrToken,
                    policy =>
                        policy.Requirements.Add(new DefaulAuthorizationRequrement()));
            });

            services
                .AddAuthentication(option =>
                    option.DefaultAuthenticateScheme = MilouAuthenticationConstants.MilouAuthenticationScheme)
                .AddMilouAuthentication(MilouAuthenticationConstants.MilouAuthenticationScheme,
                    "Milou",
                    options => { });

            services.AddSingleton<IAuthorizationHandler, DefaultAuthorizationHandler>();
            services.AddSingleton<IHostedService, RefreshCacheBackgroundService>();


            autofacOptions.UpdateServices();
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            app.UseWebSockets();

            app.UseMiddleware<JobLogMiddleware>();

            app.UseMvc();

            app.UseStaticFiles();
        }
    }
}