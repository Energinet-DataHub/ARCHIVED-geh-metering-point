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
using Energinet.DataHub.MeteringPoints.Application.Common.Users;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI
{
    public class ActorMessageFactory : IActorMessageFactory
    {
        private readonly IUserContext _userContext;
        private readonly ISystemDateTimeProvider _dateTimeProvider;
        private readonly ICorrelationContext _correlationContext;

        public ActorMessageFactory(IUserContext userContext, ISystemDateTimeProvider dateTimeProvider, ICorrelationContext correlationContext)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        }

        public ConfirmMessage CreateNewMeteringPointConfirmation(string gsrnNumber, string effectiveDate, string transactionId)
        {
            var glnNumber = "8200000008842";
            return new ConfirmMessage(
                DocumentName: "ConfirmRequestChangeAccountingPointCharacteristics_MarketDocument",
                Id: Guid.NewGuid().ToString(),
                Type: "414",
                ProcessType: "E65",
                BusinessSectorType: "E21",
                Sender: new MarketRoleParticipant(
                    Id: "DataHub GLN", // TODO: Use correct GLN
                    CodingScheme: "9",
                    Role: "EZ"),
                Receiver: new MarketRoleParticipant(
                    Id: _userContext.CurrentUser?.GlnNumber ?? glnNumber, // TODO: Hardcoded
                    CodingScheme: "9",
                    Role: "DDQ"),
                CreatedDateTime: _dateTimeProvider.Now(),
                ReasonCode: "39",
                MarketActivityRecord: new MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    BusinessProcessReference: _correlationContext.Id, // TODO: is correlation id the same as BusinessProcessReference?
                    MarketEvaluationPoint: gsrnNumber,
                    StartDateAndOrTime: effectiveDate,
                    OriginalTransaction: transactionId));
        }

        public RejectMessage CreateNewMeteringPointReject(string gsrnNumber, string effectiveDate, string transactionId, IEnumerable<ErrorMessage> errors)
        {
            var glnNumber = "8200000008842";
            return new RejectMessage(
                DocumentName: "RejectRequestChangeAccountingPointCharacteristics_MarketDocument",
                Id: Guid.NewGuid().ToString(),
                Type: "414",
                ProcessType: "E65",
                BusinessSectorType: "E21",
                Sender: new MarketRoleParticipant(
                    Id: "DataHub GLN", // TODO: Use correct GLN
                    CodingScheme: "9",
                    Role: "EZ"),
                Receiver: new MarketRoleParticipant(
                    Id: _userContext.CurrentUser?.GlnNumber ?? glnNumber, // TODO: Hardcoded
                    CodingScheme: "9",
                    Role: "DDQ"),
                CreatedDateTime: _dateTimeProvider.Now(),
                Reason: new Reason(
                    Code: "41",
                    Text: string.Empty),
                MarketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    BusinessProcessReference: _correlationContext.Id, // TODO: is correlation id the same as BusinessProcessReference?
                    MarketEvaluationPoint: gsrnNumber,
                    StartDateAndOrTime: effectiveDate,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));
        }
    }
}
