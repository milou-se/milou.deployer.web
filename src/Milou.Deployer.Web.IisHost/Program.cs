using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Structure;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.IisHost
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var customCancellationTokenSource = new CancellationTokenSource();

            App app = await App.CreateAsync(customCancellationTokenSource, default, args);

            int exitCode = await app.RunAsync(args);

            if (Environment.UserInteractive)
            {
                await Task.Run(() => RunInteractive(app, customCancellationTokenSource),
                    customCancellationTokenSource.Token);
            }

            await app.WebHost.WaitForShutdownAsync(customCancellationTokenSource.Token);

            app.Dispose();

            return exitCode;
        }

        private static async Task RunInteractive(App app, CancellationTokenSource customCancellationTokenSource)
        {
            Console.WriteLine("Running interactive");

            var commands = new Dictionary<string, Func<App, Task>>(StringComparer.OrdinalIgnoreCase)
            {
                ["proj"] = AddProject,
                ["org"] = AddOrg,
                ["orgs"] = ListOrgs
            };

            while (!customCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine(
                        $"Available commands: {Environment.NewLine}{string.Join(Environment.NewLine, commands.Keys)}");
                    Console.WriteLine("Enter command");
                    string line = Console.ReadLine()?.Trim();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }

                    if (commands.TryGetValue(line, out Func<App, Task> taskFactory))
                    {
                        await taskFactory.Invoke(app);
                    }
                    else
                    {
                        Console.WriteLine("Invalid command");
                    }
                }

                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "Exception throw in interactive mode");
                }
            }
        }

        private static async Task AddProject(App app)
        {
            Console.WriteLine("Enter project id");

            string id = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine("Invalid id");
            }

            Console.WriteLine("Enter organization id");
            string organizationId = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(organizationId))
            {
                Console.WriteLine("Invalid organization id");
            }

            var deploymentTargetWriteService = app.ComponentContext.Resolve<IDeploymentTargetWriteService>();

            if (deploymentTargetWriteService != null)
            {
                await deploymentTargetWriteService.CreateProjectAsync(new CreateProject(id, organizationId),
                    CancellationToken.None);
            }
        }

        private static async Task ListOrgs(App app)
        {
            var service = app.ComponentContext.Resolve<IDeploymentTargetReadService>();

            ImmutableArray<OrganizationInfo>
                organizations = await service.GetOrganizationsAsync(CancellationToken.None);

            foreach (OrganizationInfo organizationInfo in organizations)
            {
                Console.WriteLine($"* {organizationInfo.Organization}");
                foreach (ProjectInfo organizationInfoProject in organizationInfo.Projects)
                {
                    Console.WriteLine($"\t{organizationInfoProject.ProjectInvariantName}");
                }
            }
        }

        private static async Task AddOrg(App app)
        {
            Console.WriteLine("Enter organization id");

            string id = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(id))
            {
                var deploymentTargetWriteService = app.ComponentContext.Resolve<IDeploymentTargetWriteService>();

                if (deploymentTargetWriteService != null)
                {
                    await deploymentTargetWriteService.CreateOrganizationAsync(new CreateOrganization(id),
                        CancellationToken.None);
                }
            }
            else
            {
                Console.WriteLine("Invalid id");
            }
        }
    }
}