using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Milou.Deployer.Web.Core.Startup;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Startup
{
    [UsedImplicitly]
    public class StartupTasksMiddleware
    {
        private readonly StartupTaskContext _context;
        private readonly RequestDelegate _next;
        private readonly PathString _startupSegment = new PathString("/startup");

        public StartupTasksMiddleware(StartupTaskContext context, RequestDelegate next)
        {
            _context = context;
            _next = next;
        }

        [PublicAPI]
        public async Task Invoke(HttpContext httpContext)
        {
            if (_context.IsCompleted || httpContext.Request.Path.StartsWithSegments(_startupSegment, StringComparison.OrdinalIgnoreCase))
            {
                await _next(httpContext);
            }
            else
            {
                var response = httpContext.Response;
                response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;

                response.Headers.TryAdd("location", _startupSegment.Value);
            }
        }
    }
}
