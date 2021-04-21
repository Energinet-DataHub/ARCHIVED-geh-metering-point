using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            throw new System.NotImplementedException();
        }
    }
}
