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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Actors;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint
{
    public sealed class ConnectMeteringPointResultHandler : IBusinessProcessResultHandler<ConnectMeteringPointRequest>
    {
        private readonly IActorMessageFactory _actorMessageFactory;
        private readonly IMessageHubDispatcher _messageHubDispatcher;
        private readonly ErrorMessageFactory _errorMessageFactory;
        private readonly ActorProvider _actorProvider;
        private readonly MeteringPointPipelineContext _pipelineContext;
        private readonly ICommandScheduler _commandScheduler;

        public ConnectMeteringPointResultHandler(
            IActorMessageFactory actorMessageFactory,
            IMessageHubDispatcher messageHubDispatcher,
            ErrorMessageFactory errorMessageFactory,
            ActorProvider actorProvider,
            MeteringPointPipelineContext pipelineContext,
            ICommandScheduler commandScheduler)
        {
            _actorMessageFactory = actorMessageFactory ?? throw new ArgumentNullException(nameof(actorMessageFactory));
            _messageHubDispatcher =
                messageHubDispatcher ?? throw new ArgumentNullException(nameof(messageHubDispatcher));
            _errorMessageFactory = errorMessageFactory ?? throw new ArgumentNullException(nameof(errorMessageFactory));
            _actorProvider = actorProvider ?? throw new ArgumentNullException(nameof(actorProvider));
            _pipelineContext = pipelineContext ?? throw new ArgumentNullException(nameof(pipelineContext));
            _commandScheduler = commandScheduler ?? throw new ArgumentNullException(nameof(commandScheduler));
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

            var message = _actorMessageFactory.ConnectMeteringPointConfirmation(request.GsrnNumber, request.EffectiveDate, request.TransactionId, _actorProvider.DataHub, _actorProvider.CurrentActor);
            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmConnectMeteringPoint, request.GsrnNumber).ConfigureAwait(false);

            var command = new SendAccountingPointCharacteristicsMessage(
                _pipelineContext.MeteringPointId,
                request.TransactionId,
                BusinessReasonCodes.ConnectMeteringPoint);
            await _commandScheduler.EnqueueAsync(command).ConfigureAwait(false);
        }

        private Task CreateRejectResponseAsync(ConnectMeteringPointRequest request, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .AsEnumerable();

            var message = _actorMessageFactory.ConnectMeteringPointReject(request.GsrnNumber, request.EffectiveDate, request.TransactionId, errors, _actorProvider.DataHub, _actorProvider.CurrentActor);
            return _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectConnectMeteringPoint, request.GsrnNumber);
        }
    }
}
