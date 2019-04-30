using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Email;

namespace Milou.Deployer.Web.IisHost.Areas.Email
{
    [UsedImplicitly]
    public class EmailModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder.AddSingleton<ISmtpService, SmtpService>();
        }
    }
}
