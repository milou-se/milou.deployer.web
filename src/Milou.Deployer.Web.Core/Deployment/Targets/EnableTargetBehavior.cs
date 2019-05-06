using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class EnableTargetBehavior : IRequestPostProcessor<EnableTarget, Unit>
    {
        private readonly IMediator _mediator;

        public EnableTargetBehavior(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Process(EnableTarget request, Unit response, CancellationToken cancellationToken)
        {
            await _mediator.Publish(new TargetEnabled(request.TargetId), cancellationToken);
        }
    }
}
