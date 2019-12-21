using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Time;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Messages;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [UsedImplicitly]
    public class WorkerLifetimeManager : INotificationHandler<TargetCreated>
    {
        private readonly ConfigurationInstanceHolder _configurationInstanceHolder;
        private readonly DeploymentService _deploymentService;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly WorkerConfiguration _workerConfiguration;
        private readonly TimeoutHelper _timeoutHelper;

        private readonly ICustomClock _clock;

        public WorkerLifetimeManager(
            ConfigurationInstanceHolder configurationInstanceHolder,
            DeploymentService deploymentService,
            WorkerConfiguration workerConfiguration,
            IMediator mediator,
            ILogger logger,
            TimeoutHelper timeoutHelper,
            ICustomClock clock)
        {
            _configurationInstanceHolder = configurationInstanceHolder;
            _deploymentService = deploymentService;
            _workerConfiguration = workerConfiguration;
            _mediator = mediator;
            _logger = logger;
            _timeoutHelper = timeoutHelper;
            _clock = clock;
        }

        public async Task Handle(TargetCreated notification, CancellationToken cancellationToken)
        {
            var worker = new DeploymentTargetWorker(notification.TargetId,
                _deploymentService,
                _logger,
                _mediator,
                _workerConfiguration,
                _timeoutHelper,
                _clock);

            // TODO remove old worker

            _configurationInstanceHolder.Add(new NamedInstance<DeploymentTargetWorker>(worker, notification.TargetId));

            await _mediator.Publish(new WorkerCreated(worker), cancellationToken);
        }
    }
}
