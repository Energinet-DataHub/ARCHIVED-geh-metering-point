// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Authorization;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline
{
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : new()
    {
        private readonly IAuthorizationHandler<TRequest, TResponse> _authorizationHandler;

        public AuthorizationBehavior(IAuthorizationHandler<TRequest, TResponse> authorizationHandler)
        {
            _authorizationHandler = authorizationHandler;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));

            var validationResult = _authorizationHandler.Authorize(request);

            if (validationResult.Success)
            {
                return next();
            }

            // TODO: Must be finalized when BusinessProcessResponder is implemented
            // var result = BusinessProcessResult.Failed(new List<string>() {"Validation errors."}) as TResponse;
            // _businessProcessResponder.RespondAsync(request, result);
            return Task.FromResult(new TResponse());
        }
    }
}
