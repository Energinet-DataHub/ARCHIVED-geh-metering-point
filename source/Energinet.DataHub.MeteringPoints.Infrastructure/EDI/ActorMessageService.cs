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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GenericNotification;
using NodaTime;
using Actor = Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor.Actor;
using IdentificationType = Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor.IdentificationType;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI
{
    public class ActorMessageService : IActorMessageService
    {
        private readonly ISystemDateTimeProvider _dateTimeProvider;
        private readonly IMessageHubDispatcher _messageHubDispatcher;
        private readonly IActorContext _actorContext;

        public ActorMessageService(
            ISystemDateTimeProvider dateTimeProvider,
            IMessageHubDispatcher messageHubDispatcher,
            IActorContext actorContext)
        {
            _dateTimeProvider = dateTimeProvider;
            _messageHubDispatcher = messageHubDispatcher;
            _actorContext = actorContext;
        }

        public async Task SendGenericNotificationMessageAsync(string transactionId, string gsrn, Instant startDateAndOrTime, string receiverGln)
        {
            var message = GenericNotificationMessageFactory.GenericNotification(
                sender: Map(_actorContext.DataHub),
                receiver: new MarketRoleParticipant(receiverGln, "A10", "DDQ"), // TODO: Re-visit when actor context has been implemented properly
                createdDateTime: _dateTimeProvider.Now(),
                gsrn,
                startDateAndOrTime,
                transactionId);

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.GenericNotification, gsrn).ConfigureAwait(false);
        }

        public async Task SendCreateMeteringPointConfirmAsync(string transactionId, string gsrn)
        {
            var message = ConfirmMessageFactory.CreateMeteringPoint(
                sender: Map(_actorContext.DataHub),
                receiver: Map(_actorContext.CurrentActor),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmCreateMeteringPoint, gsrn).ConfigureAwait(false);
        }

        public async Task SendCreateMeteringPointRejectAsync(
            string transactionId,
            string gsrn,
            IEnumerable<ErrorMessage> errors)
        {
            var message = RejectMessageFactory.ConnectMeteringPoint(
                sender: Map(_actorContext.DataHub),
                receiver: Map(_actorContext.CurrentActor),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectCreateMeteringPoint, gsrn).ConfigureAwait(false);
        }

        public async Task SendUpdateMeteringPointConfirmAsync(string transactionId, string gsrn)
        {
            var message = ConfirmMessageFactory.CreateMeteringPoint(
                sender: Map(_actorContext.DataHub),
                receiver: Map(_actorContext.CurrentActor),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmChangeMasterData, gsrn).ConfigureAwait(false);
        }

        public async Task SendUpdateMeteringPointRejectAsync(string transactionId, string gsrn, IEnumerable<ErrorMessage> errors)
        {
            var message = RejectMessageFactory.ConnectMeteringPoint(
                sender: Map(_actorContext.DataHub),
                receiver: Map(_actorContext.CurrentActor),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectChangeMasterData, gsrn).ConfigureAwait(false);
        }

        public async Task SendConnectMeteringPointConfirmAsync(string transactionId, string gsrn)
        {
            var message = ConfirmMessageFactory.ConnectMeteringPoint(
                sender: Map(_actorContext.DataHub),
                receiver: Map(_actorContext.CurrentActor),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmConnectMeteringPoint, gsrn).ConfigureAwait(false);
        }

        public async Task SendConnectMeteringPointRejectAsync(string transactionId, string gsrn, IEnumerable<ErrorMessage> errors)
        {
            var message = RejectMessageFactory.ConnectMeteringPoint(
                sender: Map(_actorContext.DataHub),
                receiver: Map(_actorContext.CurrentActor),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectConnectMeteringPoint, gsrn).ConfigureAwait(false);
        }

        public async Task SendAccountingPointCharacteristicsMessageAsync(
            string transactionId,
            string businessReasonCode,
            MeteringPointDto meteringPoint,
            EnergySupplierDto energySupplier)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));
            if (energySupplier == null) throw new ArgumentNullException(nameof(energySupplier));

            var message = AccountingPointCharacteristicsMessageFactory.Create(
                transactionId,
                businessReasonCode,
                meteringPoint,
                sender: Map(_actorContext.DataHub),
                receiver: new MarketRoleParticipant(energySupplier.GlnNumber, "A10", "DDQ"), // TODO: Re-visit when we should send out AccountingPointCharacteristics updates.
                energySupplier,
                _dateTimeProvider.Now());

            await _messageHubDispatcher
                .DispatchAsync(message, DocumentType.AccountingPointCharacteristicsMessage, meteringPoint.GsrnNumber)
                .ConfigureAwait(false);
        }

        private static MarketRoleParticipant Map(Actor actor)
        {
            var codingScheme = actor.IdentificationType.ToUpperInvariant() switch
            {
                nameof(IdentificationType.GLN) => "A10",
                nameof(IdentificationType.EIC) => "A01",
                _ => throw new InvalidOperationException("Unknown party identifier type"),
            };

            var role = actor.Roles switch
            {
                nameof(Role.MeteringPointAdministrator) => "DDZ",
                nameof(Role.GridAccessProvider) => "DDM",
                _ => throw new InvalidOperationException("Unknown party role"),
            };

            return new MarketRoleParticipant(actor.Identifier, codingScheme, role);
        }
    }
}
