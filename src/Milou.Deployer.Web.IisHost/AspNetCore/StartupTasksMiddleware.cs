using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public class StartupTasksMiddleware
    {
        private readonly StartupTaskContext _context;
        private readonly RequestDelegate _next;

        public StartupTasksMiddleware(StartupTaskContext context, RequestDelegate next)
        {
            _context = context;
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_context.IsCompleted)
            {
                await _next(httpContext);
            }
            else
            {
                var response = httpContext.Response;
                response.StatusCode = 503;
                response.Headers["Retry-After"] = "30";
                await response.WriteAsync("Service Unavailable");
            }
        }
    }

}
