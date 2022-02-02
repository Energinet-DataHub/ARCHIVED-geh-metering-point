// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
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
using Energinet.DataHub.MeteringPoints.Application.Disconnect;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.DisconnectMeteringPoint
{
    public class DisconnectReconnectMeteringPointResultHandler : IBusinessProcessResultHandler<DisconnectReconnectMeteringPointRequest>
    {
        private readonly IActorMessageService _actorMessageService;
        private readonly ErrorMessageFactory _errorMessageFactory;
        private readonly MeteringPointPipelineContext _pipelineContext;
        private readonly ICommandScheduler _commandScheduler;

        public DisconnectReconnectMeteringPointResultHandler(
            ErrorMessageFactory errorMessageFactory,
            MeteringPointPipelineContext pipelineContext,
            ICommandScheduler commandScheduler,
            IActorMessageService actorMessageService)
        {
            _errorMessageFactory = errorMessageFactory ?? throw new ArgumentNullException(nameof(errorMessageFactory));
            _pipelineContext = pipelineContext ?? throw new ArgumentNullException(nameof(pipelineContext));
            _commandScheduler = commandScheduler ?? throw new ArgumentNullException(nameof(commandScheduler));
            _actorMessageService = actorMessageService;
        }

        public Task HandleAsync(DisconnectReconnectMeteringPointRequest request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            var connectionState = EnumerationType.FromName<PhysicalState>(request.ConnectionState);

            return result.Success
                ? CreateAcceptMessageAsync(request, connectionState)
                : CreateRejectResponseAsync(request, result, connectionState);
        }

        private async Task CreateAcceptMessageAsync(DisconnectReconnectMeteringPointRequest request, PhysicalState connectionState)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (connectionState == PhysicalState.Disconnected)
            {
                await _actorMessageService
                    .SendDisconnectMeteringPointConfirmAsync(request.TransactionId, request.GsrnNumber)
                    .ConfigureAwait(false);
            }
            else if (connectionState == PhysicalState.Connected)
            {
                await _actorMessageService
                    .SendReconnectMeteringPointConfirmAsync(request.TransactionId, request.GsrnNumber)
                    .ConfigureAwait(false);
            }

            var command = new SendAccountingPointCharacteristicsMessage(
                _pipelineContext.MeteringPointId,
                request.TransactionId,
                BusinessReasonCodes.DisconnectReconnectMeteringPoint);
            await _commandScheduler.EnqueueAsync(command).ConfigureAwait(false);
        }

        private async Task CreateRejectResponseAsync(DisconnectReconnectMeteringPointRequest request, BusinessProcessResult result, PhysicalState connectionState)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .AsEnumerable();

            if (connectionState == PhysicalState.Disconnected)
            {
                await _actorMessageService
                    .SendDisconnectMeteringPointRejectAsync(request.TransactionId, request.GsrnNumber, errors)
                    .ConfigureAwait(false);
            }
            else if (connectionState == PhysicalState.Connected)
            {
                await _actorMessageService
                    .SendDisconnectMeteringPointRejectAsync(request.TransactionId, request.GsrnNumber, errors)
                    .ConfigureAwait(false);
            }
        }
    }
}
