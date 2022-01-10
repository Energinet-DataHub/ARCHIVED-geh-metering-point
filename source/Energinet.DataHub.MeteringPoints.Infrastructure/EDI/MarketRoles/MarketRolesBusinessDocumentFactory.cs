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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Actors;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using NodaTime;
using Actor = Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Actors.Actor;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.MarketRoles
{
    // TODO: This is a hack used during the test period and should be removed as soon as the business processes from Market roles are included
    public class MarketRolesBusinessDocumentFactory : IMarketRolesBusinessDocumentFactory
    {
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;
        private readonly ISystemDateTimeProvider _dateTimeProvider;
        private readonly ActorProvider _actorProvider;
        private readonly IMessageHubDispatcher _messageHubDispatcher;

        public MarketRolesBusinessDocumentFactory(
            IOutbox outbox,
            IOutboxMessageFactory outboxMessageFactory,
            ISystemDateTimeProvider dateTimeProvider,
            ActorProvider actorProvider,
            IMessageHubDispatcher messageHubDispatcher)
        {
            _outbox = outbox;
            _outboxMessageFactory = outboxMessageFactory;
            _dateTimeProvider = dateTimeProvider;
            _actorProvider = actorProvider;
            _messageHubDispatcher = messageHubDispatcher;
        }

        public async Task CreateMoveInMessageAsync(string gsrn, Instant startDate)
        {
            var message = MapMoveInMessage(gsrn, startDate);

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.GenericNotification, gsrn).ConfigureAwait(false);
        }

        protected GenericNotificationMessage MapMoveInMessage(string gsrn, Instant startDate)
        {
            return new GenericNotificationMessage(
                DocumentName: "ConfirmRequestChangeOfSupplier_MarketDocument",
                Id: Guid.NewGuid().ToString(),
                Type: "414",
                ProcessType: "E65",
                BusinessSectorType: "E21",
                Sender: Map(_actorProvider.DataHub),
                Receiver: Map(_actorProvider.CurrentActor),
                CreatedDateTime: _dateTimeProvider.Now(),
                MarketActivityRecord: new MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    StartDateAndOrTime: startDate,
                    // TODO: This should be replaced with the actual transactionID when this is moved to MarketRoles. Currently the message has not actually been sent by the market actor and thus they don't have an actual transaction ID.
                    OriginalTransaction: Guid.NewGuid().ToString()));
        }

        private static MarketRoleParticipant Map(Actor actor)
        {
            var codingScheme = actor.IdentificationType switch
            {
                nameof(IdentificationType.GLN) => "A10",
                nameof(IdentificationType.EIC) => "A01",
                _ => throw new InvalidOperationException("Unknown party identifier type"),
            };

            var role = actor.Role switch
            {
                nameof(Role.MeteringPointAdministrator) => "DDZ",
                nameof(Role.GridAccessProvider) => "DDM",
                _ => throw new InvalidOperationException("Unknown party role"),
            };

            return new MarketRoleParticipant(actor.IdentificationNumber, codingScheme, role);
        }

        private void AddToOutbox<TEdiMessage>(TEdiMessage ediMessage)
        {
            var outboxMessage = _outboxMessageFactory.CreateFrom(ediMessage, OutboxMessageCategory.ActorMessage);
            _outbox.Add(outboxMessage);
        }
    }
}
