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
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline
{
    public class InternalCommandHandlingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : InternalCommand, IRequest<TResponse>
    {
        private readonly MeteringPointContext _context;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;

        public InternalCommandHandlingBehaviour(MeteringPointContext context, ISystemDateTimeProvider systemDateTimeProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _systemDateTimeProvider = systemDateTimeProvider ?? throw new ArgumentNullException(nameof(systemDateTimeProvider));
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (next == null) throw new ArgumentNullException(nameof(next));

            await MarkAsProcessedAsync(request).ConfigureAwait(false);
            return await next().ConfigureAwait(false);
        }

        private async Task MarkAsProcessedAsync(TRequest request)
        {
            var queuedCommand = await _context.QueuedInternalCommands.FindAsync(request.Id).ConfigureAwait(false);
            queuedCommand?.SetProcessed(_systemDateTimeProvider.Now());
        }
    }
}
