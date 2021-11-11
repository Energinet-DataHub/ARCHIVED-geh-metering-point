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
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint
{
    public sealed class ConnectMeteringPointResultHandler : IBusinessProcessResultHandler<Application.Connect.ConnectMeteringPoint>
    {
        private readonly ICommandScheduler _commandScheduler;
        private readonly ErrorMessageFactory _errorMessageFactory;
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ICorrelationContext _correlationContext;

        public ConnectMeteringPointResultHandler(
            ErrorMessageFactory errorMessageFactory,
            IOutbox outbox,
            IOutboxMessageFactory outboxMessageFactory,
            IJsonSerializer jsonSerializer,
            ICorrelationContext correlationContext,
            ICommandScheduler commandScheduler)
        {
            _errorMessageFactory = errorMessageFactory;
            _outbox = outbox;
            _outboxMessageFactory = outboxMessageFactory;
            _jsonSerializer = jsonSerializer;
            _correlationContext = correlationContext;
            _commandScheduler = commandScheduler;
        }

        public Task HandleAsync(Application.Connect.ConnectMeteringPoint request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return result.Success
                ? SuccessAsync(request, result)
                : RejectAsync(request, result);
        }

        private async Task SuccessAsync(Application.Connect.ConnectMeteringPoint request, BusinessProcessResult result)
        {
            var confirmMessage = CreateConfirmMessage(request, result);
            AddToOutbox(confirmMessage);

            var command = new SendAccountingPointCharacteristicsMessage(
                request.GsrnNumber,
                request.TransactionId,
                BusinessReasonCodes.ConnectMeteringPoint);
            await _commandScheduler.EnqueueAsync(command).ConfigureAwait(false);
        }

        private MessageHubEnvelope? CreateConfirmMessage(Application.Connect.ConnectMeteringPoint request, BusinessProcessResult result)
        {
            var confirmMessage = new ConnectMeteringPointAccepted(
                TransactionId: result.TransactionId,
                GsrnNumber: request.GsrnNumber,
                Status: "Accepted");

            var serializedMessage = _jsonSerializer.Serialize(confirmMessage);

            var envelope = new MessageHubEnvelope(
                string.Empty,
                serializedMessage,
                DocumentType.ConnectMeteringPointAccepted,
                _correlationContext.AsTraceContext(),
                request.GsrnNumber);

            return envelope;
        }

        private Task RejectAsync(Application.Connect.ConnectMeteringPoint request, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .ToArray();

            var ediMessage = new ConnectMeteringPointRejected(
                TransactionId: result.TransactionId,
                GsrnNumber: request.GsrnNumber,
                Status: "Rejected", // TODO: Is this necessary? Also, Reason?
                Reason: "TODO",
                Errors: errors);

            var envelope = new MessageHubEnvelope(string.Empty, _jsonSerializer.Serialize(ediMessage), DocumentType.ConnectMeteringPointRejected, _correlationContext.AsTraceContext(), request.GsrnNumber);
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
