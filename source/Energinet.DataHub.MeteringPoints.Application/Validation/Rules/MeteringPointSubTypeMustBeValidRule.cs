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
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class MeteringPointSubTypeMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        public MeteringPointSubTypeMustBeValidRule()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.SubTypeOfMeteringPoint)
                .NotEmpty()
                .WithState(createMeteringPoint => new MeteringPointSubTypeMandatoryValidationError(createMeteringPoint.GsrnNumber));

            RuleFor(createMeteringPoint => createMeteringPoint)
                .Must(SubTypeIsPhysicalOrVirtualOrCalculated)
                .WithState(createMeteringPoint => new MeteringPointSubTypeValueMustBeValidValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.SubTypeOfMeteringPoint));
        }

        private static bool GroupOfMeteringPointTypesThatMustBeSubTypeVirtualOrCalculated(CreateMeteringPoint createMeteringPoint)
        {
            return new HashSet<string>
                    {
                        MeteringPointType.WholesaleServices.Name,
                        MeteringPointType.OwnProduction.Name,
                        MeteringPointType.NetFromGrid.Name,
                        MeteringPointType.NetToGrid.Name,
                        MeteringPointType.TotalConsumption.Name,
                    }
                .Contains(createMeteringPoint.TypeOfMeteringPoint) ||
                MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNine(createMeteringPoint);
        }

        private static bool MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNine(
            CreateMeteringPoint createMeteringPoint)
        {
            return IsProductionOrConsumption(createMeteringPoint) &&
                   !NetSettlementGroupIsZeroOrNinetyNine(createMeteringPoint.NetSettlementGroup);
        }

        private static bool IsProductionOrConsumption(CreateMeteringPoint createMeteringPoint)
        {
            return new HashSet<string>
                    {
                        MeteringPointType.Consumption.Name,
                        MeteringPointType.Production.Name,
                    }
                .Contains(createMeteringPoint.TypeOfMeteringPoint);
        }

        private static bool IsConsumption(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.Consumption.Name,
                StringComparison.Ordinal);
        }

        private static bool NetSettlementGroupIsZeroOrNinetyNine(string? netSettlement)
        {
            if (string.IsNullOrEmpty(netSettlement)) return false;

            return new HashSet<string>
                    {
                        NetSettlementGroup.Ninetynine.Name,
                        NetSettlementGroup.Zero.Name,
                    }
                .Contains(netSettlement);
        }

        private static bool SubTypeIsPhysicalOrVirtualOrCalculated(CreateMeteringPoint createMeteringPoint)
        {
            return GroupOfMeteringPointTypesThatMustBeSubTypeVirtualOrCalculated(createMeteringPoint)
                ? SubTypeIsVirtualOrCalculated(createMeteringPoint)
                : ExpectedSubTypes(createMeteringPoint);
        }

        private static bool SubTypeIsVirtualOrCalculated(CreateMeteringPoint createMeteringPoint)
        {
            return Virtual(createMeteringPoint.SubTypeOfMeteringPoint) ||
                   Calculated(createMeteringPoint.SubTypeOfMeteringPoint);
        }

        private static bool ExpectedSubTypes(CreateMeteringPoint createMeteringPoint)
        {
            return new HashSet<string>
                    {
                        MeteringPointSubType.Calculated.Name,
                        MeteringPointSubType.Physical.Name,
                        MeteringPointSubType.Virtual.Name,
                    }
                .Contains(createMeteringPoint.SubTypeOfMeteringPoint);
        }

        private static bool Virtual(string subTypeOfMeteringPoint)
        {
            return subTypeOfMeteringPoint.Equals(MeteringPointSubType.Virtual.Name, StringComparison.Ordinal);
        }

        private static bool Calculated(string subTypeOfMeteringPoint)
        {
            return subTypeOfMeteringPoint.Equals(MeteringPointSubType.Calculated.Name, StringComparison.Ordinal);
        }
    }
}
