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
using System.Xml.Linq;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Users;
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
    public class CreateMeteringPointResultHandler : IBusinessProcessResultHandler<Application.Create.CreateMeteringPoint>
    {
        private readonly ErrorMessageFactory _errorMessageFactory;
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ICorrelationContext _correlationContext;
        private readonly ISystemDateTimeProvider _dateTimeProvider;
        private readonly IUserContext _userContext;

        public CreateMeteringPointResultHandler(
            ErrorMessageFactory errorMessageFactory,
            IOutbox outbox,
            IOutboxMessageFactory outboxMessageFactory,
            IJsonSerializer jsonSerializer,
            ICorrelationContext correlationContext,
            ISystemDateTimeProvider dateTimeProvider,
            IUserContext userContext)
        {
            _errorMessageFactory = errorMessageFactory;
            _outbox = outbox;
            _outboxMessageFactory = outboxMessageFactory;
            _jsonSerializer = jsonSerializer;
            _correlationContext = correlationContext;
            _dateTimeProvider = dateTimeProvider;
            _userContext = userContext;
        }

        public Task HandleAsync(Application.Create.CreateMeteringPoint request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return result.Success
                ? CreateAcceptMessageAsync(request, result)
                : CreateRejectResponseAsync(request, result);
        }

        protected Task CreateAcceptMessageAsync(Application.Create.CreateMeteringPoint request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            var message = new ConfirmMessage(
                DocumentName: "ConfirmRequestChangeOfSupplier_MarketDocument",
                Id: Guid.NewGuid().ToString(),
                Type: "414",
                ProcessType: "E65",
                BusinessSectorType: "E21",
                Sender: new MarketRoleParticipant(
                    Id: "DataHub GLN", // TODO: Use correct GLN
                    CodingScheme: "9",
                    Role: "EZ"),
                Receiver: new MarketRoleParticipant(
                    Id: _userContext.CurrentUser?.GlnNumber ?? "8200000000006", // TODO: Hardcoded
                    CodingScheme: "9",
                    Role: "DDQ"),
                CreatedDateTime: _dateTimeProvider.Now(),
                ReasonCode: "39",
                MarketActivityRecord: new MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    BusinessProcessReference: _correlationContext.Id,
                    MarketEvaluationPoint: request.GsrnNumber,
                    StartDateAndOrTime: request.EffectiveDate,
                    OriginalTransaction: request.TransactionId));

            XNamespace xmlNamespace = "urn:ebix:org:ChangeAccountingPointCharacteristics:0:1";
            var document = AcknowledgementXmlSerializer.Serialize(message, xmlNamespace);

            var envelope = CreatePostOfficeEnvelope(
                recipient: _userContext.CurrentUser?.GlnNumber ?? "8200000000006", // TODO: Hardcoded
                cimContent: document,
                messageType: typeof(ConfirmMessage).FullName!);

            AddToOutbox(envelope);

            return Task.CompletedTask;
        }

        private PostOfficeMessageEnvelope CreatePostOfficeEnvelope(string recipient, string cimContent, string messageType)
        {
            return new(
                Recipient: recipient,
                Content: cimContent,
                MessageType: messageType,
                Correlation: _correlationContext.AsTraceContext()); // TODO: add correlation when Telemetry is added
        }

        private Task CreateAcceptResponseAsync(Application.Create.CreateMeteringPoint request, BusinessProcessResult result)
        {
            var ediMessage = new CreateMeteringPointAccepted(
                TransactionId: result.TransactionId,
                GsrnNumber: request.GsrnNumber,
                Status: "Accepted");
            var envelope = new PostOfficeMessageEnvelope(string.Empty, _jsonSerializer.Serialize(ediMessage), nameof(CreateMeteringPointAccepted), _correlationContext.AsTraceContext());
            AddToOutbox(envelope);

            return Task.CompletedTask;
        }

        private Task CreateRejectResponseAsync(Application.Create.CreateMeteringPoint request, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .ToArray();

            var ediMessage = new CreateMeteringPointRejected(
                TransactionId: result.TransactionId,
                GsrnNumber: request.GsrnNumber,
                Status: "Rejected", // TODO: Is this necessary? Also, Reason?
                Reason: "TODO",
                Errors: errors);
            var envelope = new PostOfficeMessageEnvelope(string.Empty, _jsonSerializer.Serialize(ediMessage), nameof(CreateMeteringPointRejected), _correlationContext.AsTraceContext());

            AddToOutbox(envelope);

            return Task.CompletedTask;
        }

        private void AddToOutbox<TEdiMessage>(TEdiMessage ediMessage)
        {
            var outboxMessage = _outboxMessageFactory.CreateFrom(ediMessage, OutboxMessageCategory.ActorMessage);
            _outbox.Add(outboxMessage);
        }
    }
}
