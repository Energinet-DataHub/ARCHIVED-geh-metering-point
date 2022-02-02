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
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Extensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GenericNotification;
using NodaTime;
using Actor = Energinet.DataHub.Core.App.Common.Abstractions.Actor.Actor;
using IdentificationType = Energinet.DataHub.Core.App.Common.Abstractions.Actor.IdentificationType;

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
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: new MarketRoleParticipant(receiverGln, "A10", "DDQ"), // TODO: Re-visit when actor context has been implemented properly
                createdDateTime: _dateTimeProvider.Now(),
                gsrn,
                startDateAndOrTime,
                transactionId);

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.GenericNotification, receiverGln, gsrn).ConfigureAwait(false);
        }

        public async Task SendCreateMeteringPointConfirmAsync(string transactionId, string gsrn)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = ConfirmMessageFactory.CreateMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmCreateMeteringPoint, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendCreateMeteringPointRejectAsync(
            string transactionId,
            string gsrn,
            IEnumerable<ErrorMessage> errors)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = RejectMessageFactory.ConnectMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectCreateMeteringPoint, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendUpdateMeteringPointConfirmAsync(string transactionId, string gsrn)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = ConfirmMessageFactory.CreateMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmChangeMasterData, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendUpdateMeteringPointRejectAsync(string transactionId, string gsrn, IEnumerable<ErrorMessage> errors)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = RejectMessageFactory.ConnectMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectChangeMasterData, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendConnectMeteringPointConfirmAsync(string transactionId, string gsrn)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = ConfirmMessageFactory.ConnectMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmConnectMeteringPoint, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendConnectMeteringPointRejectAsync(string transactionId, string gsrn, IEnumerable<ErrorMessage> errors)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = RejectMessageFactory.ConnectMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectConnectMeteringPoint, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendDisconnectMeteringPointConfirmAsync(string transactionId, string gsrn)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = ConfirmMessageFactory.DisconnectMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmDisconnectMeteringPoint, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendDisconnectMeteringPointRejectAsync(string transactionId, string gsrn, IEnumerable<ErrorMessage> errors)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = RejectMessageFactory.DisconnectMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectDisconnectMeteringPoint, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendReconnectMeteringPointConfirmAsync(string transactionId, string gsrn)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = ConfirmMessageFactory.ReconnectMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmReconnectMeteringPoint, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendReconnectMeteringPointRejectAsync(string transactionId, string gsrn, IEnumerable<ErrorMessage> errors)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = RejectMessageFactory.ReconnectMeteringPoint(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectReconnectMeteringPoint, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
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
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: new MarketRoleParticipant(energySupplier.GlnNumber, "A10", "DDQ"), // TODO: Re-visit when we should send out AccountingPointCharacteristics updates.
                energySupplier,
                _dateTimeProvider.Now());

            await _messageHubDispatcher
                .DispatchAsync(message, DocumentType.AccountingPointCharacteristicsMessage, energySupplier.GlnNumber, meteringPoint.GsrnNumber)
                .ConfigureAwait(false);
        }

        public async Task SendRequestCloseDownAcceptedAsync(string transactionId, string gsrnNumber)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = ConfirmMessageFactory.RequestCloseDown(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrnNumber,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.AcceptCloseDownRequest, _actorContext.CurrentActor.Identifier, gsrnNumber).ConfigureAwait(false);
        }

        public async Task SendRequestCloseDownRejectedAsync(string transactionId, string gsrnNumber, IEnumerable<ErrorMessage> errors)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = RejectMessageFactory.RequestCloseDown(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrnNumber,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectCloseDownRequest, _actorContext.CurrentActor.Identifier, gsrnNumber).ConfigureAwait(false);
        }

        private static MarketRoleParticipant Map(Actor actor, Role documentRole)
        {
            var codingScheme = actor.IdentificationType.ToUpperInvariant() switch
            {
                nameof(IdentificationType.GLN) => "A10",
                nameof(IdentificationType.EIC) => "A01",
                _ => throw new InvalidOperationException($"Unknown party identifier type: {actor.IdentificationType}"),
            };

            var currentRole = actor.GetRole(documentRole);
            var role = currentRole.Name switch
            {
                nameof(Role.MeteringPointAdministrator) => "DDZ",
                nameof(Role.GridAccessProvider) => "DDM",
                nameof(Role.BalancePowerSupplier) => "DDQ",
                nameof(Role.SystemOperator) => "EZ",
                nameof(Role.MeteredDataResponsible) => "MDR",
                _ => throw new InvalidOperationException($"Unknown party role: {currentRole.Name}"),
            };

            return new MarketRoleParticipant(actor.Identifier, codingScheme, role);
        }
    }
}
