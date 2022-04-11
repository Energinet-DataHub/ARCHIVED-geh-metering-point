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
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.ProcessOverview;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.Processes;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline
{
    public class ProcessOverviewBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ProcessExtractor<TRequest> _processExtractor;
        private readonly MeteringPointContext _meteringPointContext;

        public ProcessOverviewBehavior(
            ProcessExtractor<TRequest> processExtractor,
            MeteringPointContext meteringPointContext)
        {
            _processExtractor = processExtractor;
            _meteringPointContext = meteringPointContext;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));

            if (!_processExtractor.IsProcessOverviewEnabled)
            {
                return await next().ConfigureAwait(false);
            }

            var requestDetails = _processExtractor.GetProcessDetails(request);

            // Call next handler in the pipeline and wait for the result
            var result = await next().ConfigureAwait(false);

            var businessProcessResult = result as BusinessProcessResult
                                        ?? throw new InvalidOperationException($"Results should be {nameof(BusinessProcessResult)}");
            var resultDetails = _processExtractor.GetProcessDetails(businessProcessResult);

            var process = _processExtractor.GetProcess(request, requestDetails, resultDetails);

            _meteringPointContext.Processes.Add(process);

            return result;
        }
    }
}
