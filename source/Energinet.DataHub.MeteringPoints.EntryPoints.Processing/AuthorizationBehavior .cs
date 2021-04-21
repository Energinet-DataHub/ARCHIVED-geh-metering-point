using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            throw new System.NotImplementedException();
        }
    }
}
