﻿using MediatR;

namespace Milou.Deployer.Web.Core.Agents
{
    public class AgentLogNotification : INotification
    {
        public AgentLogNotification(string deploymentTaskId, string deploymentTargetId, string message)
        {
            DeploymentTaskId = deploymentTaskId;
            DeploymentTargetId = deploymentTargetId;
            Message = message;
        }

        public string DeploymentTargetId { get; }

        public string Message { get;  }

        public string DeploymentTaskId { get; }
    }
}