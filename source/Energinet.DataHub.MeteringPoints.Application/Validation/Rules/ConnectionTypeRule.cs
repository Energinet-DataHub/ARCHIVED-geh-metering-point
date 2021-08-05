// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class ConnectionTypeRule : AbstractValidator<CreateMeteringPoint>
    {
        public ConnectionTypeRule()
        {
            When(ConnectionTypeIsMandatory, () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint.ConnectionType)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithState(createMeteringPoint =>
                        new ConnectionTypeMandatoryValidationError(
                            createMeteringPoint.GsrnNumber,
                            createMeteringPoint.ConnectionType));
            });

            When(ConnectionTypeIsNotAllowed, () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint.ConnectionType)
                    .Cascade(CascadeMode.Stop)
                    .Empty()
                    .WithState(createMeteringPoint =>
                        new ConnectionTypeMandatoryValidationError(
                            createMeteringPoint.GsrnNumber,
                            createMeteringPoint.ConnectionType));
            });

            When(createMeteringPoint => createMeteringPoint.ConnectionType.Length > 0, () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint.ConnectionType)
                    .Must(AllowedConnectionTypes)
                    .WithState(createMeteringPoint =>
                        new ConnectionTypeWrongValueValidationError(
                            createMeteringPoint.GsrnNumber,
                            createMeteringPoint.ConnectionType));
            });
        }

        private static bool ConnectionTypeIsNotAllowed(CreateMeteringPoint createMeteringPoint)
        {
            return NetSettlementGroupIsZero(createMeteringPoint) &&
                   ProductOrConsumptionMeteringPointTypes(createMeteringPoint);
        }

        private static bool AllowedConnectionTypes(string connectionType)
        {
            return new HashSet<string>
                {
                    ConnectionType.Direct.Name,
                    ConnectionType.Installation.Name,
                }
                .Contains(connectionType);
        }

        private static bool ConnectionTypeIsMandatory(CreateMeteringPoint createMeteringPoint)
        {
            return ProductOrConsumptionMeteringPointTypes(createMeteringPoint) &&
                   !NetSettlementGroupIsZero(createMeteringPoint);
        }

        private static bool NetSettlementGroupIsZero(CreateMeteringPoint createMeteringPoint)
        {
            return NetSettlementGroup.Zero.Name.Equals(createMeteringPoint.NetSettlementGroup, StringComparison.Ordinal);
        }

        private static bool ProductOrConsumptionMeteringPointTypes(CreateMeteringPoint createMeteringPoint)
        {
            return new HashSet<string>
                {
                    MeteringPointType.Production.Name,
                    MeteringPointType.Consumption.Name,
                }
                .Contains(createMeteringPoint.TypeOfMeteringPoint);
        }
    }
}
