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
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Users;
using Energinet.DataHub.MeteringPoints.Application.Create.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint
{
    public class CreateMeteringPointResultHandler : IBusinessProcessResultHandler<CreateConsumptionMeteringPoint>
    {
        private readonly IActorMessageFactory _actorMessageFactory;
        private readonly ErrorMessageFactory _errorMessageFactory;
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ICorrelationContext _correlationContext;
        private readonly IUserContext _userContext;

        private readonly string _glnNumber = "8200000008842";

        public CreateMeteringPointResultHandler(
            IActorMessageFactory actorMessageFactory,
            ErrorMessageFactory errorMessageFactory,
            IOutbox outbox,
            IOutboxMessageFactory outboxMessageFactory,
            IJsonSerializer jsonSerializer,
            ICorrelationContext correlationContext,
            IUserContext userContext)
        {
            _actorMessageFactory = actorMessageFactory;
            _errorMessageFactory = errorMessageFactory;
            _outbox = outbox;
            _outboxMessageFactory = outboxMessageFactory;
            _jsonSerializer = jsonSerializer;
            _correlationContext = correlationContext;
            _userContext = userContext;
        }

        public Task HandleAsync(CreateConsumptionMeteringPoint request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return result.Success
                ? CreateAcceptMessageAsync(request)
                : CreateRejectResponseAsync(request, result);
        }

        private Task CreateAcceptMessageAsync(CreateConsumptionMeteringPoint request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var message = _actorMessageFactory.CreateNewMeteringPointConfirmation(request.GsrnNumber, request.EffectiveDate, request.TransactionId);
            var envelope = CreateMessageHubEnvelope(
                recipient: _userContext.CurrentUser?.GlnNumber ?? _glnNumber, // TODO: Hardcoded
                cimContent: _jsonSerializer.Serialize(message),
                messageType: DocumentType.CreateMeteringPointAccepted);

            AddToOutbox(envelope);

            return Task.CompletedTask;
        }

        private Task CreateRejectResponseAsync(CreateConsumptionMeteringPoint request, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .AsEnumerable();

            var message = _actorMessageFactory.CreateNewMeteringPointReject(request.GsrnNumber, request.EffectiveDate, request.TransactionId, errors);
            var envelope = CreateMessageHubEnvelope(
                recipient: _userContext.CurrentUser?.GlnNumber ?? _glnNumber, // TODO: Hardcoded
                cimContent: _jsonSerializer.Serialize(message),
                messageType: DocumentType.CreateMeteringPointRejected);

            AddToOutbox(envelope);

            return Task.CompletedTask;
        }

        private MessageHubEnvelope CreateMessageHubEnvelope(string recipient, string cimContent, DocumentType messageType)
        {
            return new(
                Recipient: recipient,
                Content: cimContent,
                MessageType: messageType,
                Correlation: _correlationContext.AsTraceContext()); // TODO: add correlation when Telemetry is added
        }

        private void AddToOutbox<TEdiMessage>(TEdiMessage ediMessage)
        {
            var outboxMessage = _outboxMessageFactory.CreateFrom(ediMessage, OutboxMessageCategory.ActorMessage);
            _outbox.Add(outboxMessage);
        }
    }
}
