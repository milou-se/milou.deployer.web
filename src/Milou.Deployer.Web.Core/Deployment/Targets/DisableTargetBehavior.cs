using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using MediatR.Pipeline;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    [UsedImplicitly]
    public class DisableTargetBehavior : IPipelineBehavior<DisableTarget, Unit>
    {
        private readonly IMediator _mediator;

        public DisableTargetBehavior(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Unit> Handle(DisableTarget request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            await next();
            await _mediator.Publish(new TargetDisabled(request.TargetId), cancellationToken);
            return Unit.Value;
        }
    }
}
