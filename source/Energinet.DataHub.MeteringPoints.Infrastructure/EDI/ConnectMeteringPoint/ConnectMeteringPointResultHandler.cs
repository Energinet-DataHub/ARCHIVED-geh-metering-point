﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint
{
    public sealed class ConnectMeteringPointResultHandler : IBusinessProcessResultHandler<ConnectMeteringPointRequest>
    {
        private readonly IActorMessageService _actorMessageService;
        private readonly MeteringPointPipelineContext _pipelineContext;
        private readonly ICommandScheduler _commandScheduler;

        public ConnectMeteringPointResultHandler(
            MeteringPointPipelineContext pipelineContext,
            ICommandScheduler commandScheduler,
            IActorMessageService actorMessageService)
        {
            _pipelineContext = pipelineContext ?? throw new ArgumentNullException(nameof(pipelineContext));
            _commandScheduler = commandScheduler ?? throw new ArgumentNullException(nameof(commandScheduler));
            _actorMessageService = actorMessageService;
        }

        public Task HandleAsync(ConnectMeteringPointRequest request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return result.Success
                ? CreateAcceptMessageAsync(request)
                : CreateRejectResponseAsync(request, result);
        }

        private async Task CreateAcceptMessageAsync(ConnectMeteringPointRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await _actorMessageService
                .SendConnectMeteringPointConfirmAsync(request.TransactionId, request.GsrnNumber)
                .ConfigureAwait(false);

            var command = new SendAccountingPointCharacteristicsMessage(
                _pipelineContext.MeteringPointId,
                request.TransactionId,
                BusinessReasonCodes.ConnectMeteringPoint);
            await _commandScheduler.EnqueueAsync(command).ConfigureAwait(false);
        }

        private async Task CreateRejectResponseAsync(ConnectMeteringPointRequest request, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => ErrorMessageFactory.GetErrorMessage(error))
                .AsEnumerable();

            await _actorMessageService
                .SendConnectMeteringPointRejectAsync(request.TransactionId, request.GsrnNumber, errors)
                .ConfigureAwait(false);
        }
    }
}
