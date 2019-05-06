using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using MediatR.Pipeline;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    [UsedImplicitly]
    public class EnableTargetBehavior : IPipelineBehavior<EnableTarget, Unit>
    {
        private readonly IMediator _mediator;

        public EnableTargetBehavior(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Unit> Handle(EnableTarget request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            await next();
            await _mediator.Publish(new TargetEnabled(request.TargetId), cancellationToken);
            return Unit.Value;
        }
    }
}
