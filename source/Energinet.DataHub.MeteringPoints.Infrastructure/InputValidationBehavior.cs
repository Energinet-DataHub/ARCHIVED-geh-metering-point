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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.InputValidation;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure
{
    public class InputValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : new()
    {
        private readonly IValidator<TRequest, TResponse> _validator;

        public InputValidationBehavior(IValidator<TRequest, TResponse> validator)
        {
            _validator = validator;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var validationResult = _validator.Validate(request);

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
