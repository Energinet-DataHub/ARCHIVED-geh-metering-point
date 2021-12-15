﻿// Copyright 2020 Energinet DataHub A/S
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
            return ConfirmMessageFactory.CreateMeteringPoint(
                sender: new MarketRoleParticipant(
                    Id: "DataHub GLN", // TODO: Use from actor register
                    CodingScheme: "A10",
                    Role: "DDZ"),
                receiver: new MarketRoleParticipant(
                    Id: _userContext.CurrentUser?.GlnNumber ?? glnNumber, // TODO: Use from actor register
                    CodingScheme: "A10",
                    Role: "DDQ"),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrnNumber,
                    OriginalTransaction: transactionId));
        }

        public RejectMessage CreateNewMeteringPointReject(string gsrnNumber, string effectiveDate, string transactionId, IEnumerable<ErrorMessage> errors)
        {
            var glnNumber = "8200000008842";
            return RejectMessageFactory.CreateMeteringPoint(
                sender: new MarketRoleParticipant(// TODO: Use from actor register
                    Id: "DataHub GLN",
                    CodingScheme: "A10",
                    Role: "DDZ"),
                receiver: new MarketRoleParticipant(// TODO: Use from actor register
                    Id: _userContext.CurrentUser?.GlnNumber ?? glnNumber,
                    CodingScheme: "A10",
                    Role: "DDM"),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrnNumber,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));
        }
    }
}
