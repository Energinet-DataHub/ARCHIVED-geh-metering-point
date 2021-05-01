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
using BusinessProcessPipeline.Application.Commands;
using MediatR;

namespace BusinessProcessPipeline.Infrastructure.Behaviours
{
    public class ValidationReportsBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBusinessRequest
        where TResponse : BusinessProcessResult
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            // Call next handler and await result
            var result = await next();
            
            // Not Success we must forward result to validation reports service
            if (result.Success == false)
            {
                
            }

            return result;
        }
    }
}