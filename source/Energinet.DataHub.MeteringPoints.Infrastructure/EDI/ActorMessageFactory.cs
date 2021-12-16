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
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Parties;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI
{
    public class ActorMessageFactory : IActorMessageFactory
    {
        private readonly ISystemDateTimeProvider _dateTimeProvider;

        public ActorMessageFactory(ISystemDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public ConfirmMessage CreateNewMeteringPointConfirmation(
            string gsrnNumber,
            string effectiveDate,
            string transactionId,
            Party sender,
            Party receiver)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));

            return ConfirmMessageFactory.CreateMeteringPoint(
                sender: Map(sender),
                receiver: Map(receiver),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrnNumber,
                    OriginalTransaction: transactionId));
        }

        public RejectMessage CreateNewMeteringPointReject(
            string gsrnNumber,
            string effectiveDate,
            string transactionId,
            IEnumerable<ErrorMessage> errors,
            Party sender,
            Party receiver)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));

            return RejectMessageFactory.CreateMeteringPoint(
                sender: Map(sender),
                receiver: Map(receiver),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrnNumber,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));
        }

        private static MarketRoleParticipant Map(Party party)
        {
            var codingScheme = party.IdentificationType switch
            {
                nameof(IdentificationType.GLN) => "A10",
                nameof(IdentificationType.EIC) => "A01",
                _ => throw new InvalidOperationException("Unknown party identifier type"),
            };

            var role = party.Role switch
            {
                nameof(Role.MeteringPointAdministrator) => "DDZ",
                nameof(Role.GridAccessProvider) => "DDM",
                _ => throw new InvalidOperationException("Unknown party role"),
            };

            return new MarketRoleParticipant(party.IdentificationNumber, codingScheme, role);
        }
    }
}
