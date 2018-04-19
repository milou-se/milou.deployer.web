using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace Milou.Deployer.Web.Core.Email
{
    public interface ISmtpService
    {
        Task SendAsync(MimeMessage mimeMessage, CancellationToken cancellationToken);
    }
}