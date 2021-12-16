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

using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI
{
    /// <summary>
    /// Factory for creation actor (EDI) messages
    /// </summary>
    public interface IActorMessageFactory
    {
        /// <summary>
        /// Creates a confirmation message for the create new metering point business process
        /// </summary>
        /// <returns><see cref="ConfirmMessage"/></returns>
        ConfirmMessage CreateNewMeteringPointConfirmation(string gsrnNumber, string effectiveDate, string transactionId, Actor sender, Actor receiver);

        /// <summary>
        /// Creates a reject message for the create new metering point business process
        /// </summary>
        /// <param name="gsrnNumber">GSRN number of metering point</param>
        /// <param name="effectiveDate"></param>
        /// <param name="transactionId"></param>
        /// <param name="errors"></param>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        /// <returns><see cref="RejectMessage"/></returns>
        RejectMessage CreateNewMeteringPointReject(string gsrnNumber, string effectiveDate, string transactionId, IEnumerable<ErrorMessage> errors, Actor sender, Actor receiver);
    }
}
