﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Milou.Deployer.Web.Core.Application;
using Serilog;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [UsedImplicitly]
    public class DevelopmentPermissionHandler : IAuthorizationHandler
    {
        private readonly ILogger _logger;
        private readonly EnvironmentConfiguration _environmentConfiguration;

        public DevelopmentPermissionHandler(EnvironmentConfiguration environmentConfiguration, ILogger logger)
        {
            _logger = logger;
            _environmentConfiguration = environmentConfiguration;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (!_environmentConfiguration.IsDevelopmentMode)
            {
                return Task.CompletedTask;
            }

            List<IAuthorizationRequirement> pendingRequirements = context.PendingRequirements.ToList();

            if (pendingRequirements.Count > 0)
            {
                _logger.Warning("Development mode is enabled: request has [{Count}] pending requirements", pendingRequirements.Count);
                foreach (IAuthorizationRequirement requirement in pendingRequirements)
                {
                    _logger.Warning("Development mode is enabled: fulfilling authorization requirement {Requirement}", requirement.GetType().FullName);
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}