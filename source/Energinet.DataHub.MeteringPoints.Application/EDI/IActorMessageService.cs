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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Application.EDI
{
    /// <summary>
    /// Create business documents.
    /// </summary>
    public interface IActorMessageService
    {
        /// <summary>
        /// Generic Notification (RSM004).
        /// </summary>
        Task SendGenericNotificationMessageAsync(
            string transactionId,
            string gsrn,
            Instant startDateAndOrTime);

        /// <summary>
        /// Confirmation of create metering point.
        /// </summary>
        Task SendCreateMeteringPointConfirmAsync(
            string transactionId,
            string gsrn);

        /// <summary>
        /// Rejection of create metering point.
        /// </summary>
        Task SendCreateMeteringPointRejectAsync(
            string transactionId,
            string gsrn,
            IEnumerable<ErrorMessage> errors);

        /// <summary>
        /// Confirmation of update metering point.
        /// </summary>
        Task SendUpdateMeteringPointConfirmAsync(
            string transactionId,
            string gsrn);

        /// <summary>
        /// Rejection of update metering point.
        /// </summary>
        Task SendUpdateMeteringPointRejectAsync(
            string transactionId,
            string gsrn,
            IEnumerable<ErrorMessage> errors);

        /// <summary>
        /// Confirmation of connect metering point.
        /// </summary>
        Task SendConnectMeteringPointConfirmAsync(
            string transactionId,
            string gsrn);

        /// <summary>
        /// Rejection of connect metering point.
        /// </summary>
        Task SendConnectMeteringPointRejectAsync(
            string transactionId,
            string gsrn,
            IEnumerable<ErrorMessage> errors);

        /// <summary>
        /// Send accounting point characteristics message.
        /// </summary>
        Task SendAccountingPointCharacteristicsMessageAsync(
            string transactionId,
            string businessReasonCode,
            MeteringPointDto meteringPoint,
            EnergySupplierDto energySupplier);
    }
}
