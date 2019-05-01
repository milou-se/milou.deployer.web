﻿using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class DeploymentHistoryRequest : IRequest<DeploymentHistoryResponse>
    {
        public DeploymentHistoryRequest(string deploymentTargetId)
        {
            DeploymentTargetId = deploymentTargetId;
        }

        public string DeploymentTargetId { get; }
    }
}
