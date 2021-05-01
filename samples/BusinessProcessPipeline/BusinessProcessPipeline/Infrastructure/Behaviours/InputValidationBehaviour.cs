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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessProcessPipeline.Application.Commands;
using MediatR;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace BusinessProcessPipeline.Infrastructure.Behaviours
{
    public class InputValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBusinessRequest
        where TResponse : BusinessProcessResult
    {
        private readonly IBusinessProcessResponder<TRequest> _businessProcessResponder;

        public InputValidationBehaviour(IBusinessProcessResponder<TRequest> businessProcessResponder)
        {
            _businessProcessResponder = businessProcessResponder;
            // Inject all instances of AbstractValidator<TRequest> here
        }
        
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            // Run validations
            bool success = true;
            if (success == true)
            {
                // No validation errors => continue to next handler in pipeline
                return next();
            }
            else
            {
                // Some validations failed => return BusinesProcessResult
                var result = BusinessProcessResult.Failed(new List<string>() {"Validation errors."}) as TResponse;
                _businessProcessResponder.RespondAsync(request, result);
                return Task.FromResult(result);
            }
        }
    }
}