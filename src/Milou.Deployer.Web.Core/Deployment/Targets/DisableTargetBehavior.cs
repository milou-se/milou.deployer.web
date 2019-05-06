using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class DisableTargetBehavior : IRequestPostProcessor<DisableTarget, Unit>
    {
        private readonly IMediator _mediator;

        public DisableTargetBehavior(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Process(DisableTarget request, Unit response, CancellationToken cancellationToken)
        {
            await _mediator.Publish(new TargetDisabled(request.TargetId), cancellationToken);
        }
    }
}