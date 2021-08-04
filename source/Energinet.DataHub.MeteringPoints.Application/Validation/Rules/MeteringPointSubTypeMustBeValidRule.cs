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
            return IsWholeSaleServices(createMeteringPoint) ||
                   IsOwnProduction(createMeteringPoint) ||
                   IsNetFromGrid(createMeteringPoint) ||
                   IsNetToGrid(createMeteringPoint) ||
                   IsTotalConsumption(createMeteringPoint) ||
                   MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNine(createMeteringPoint);
        }

        private static bool MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNine(
            CreateMeteringPoint createMeteringPoint)
        {
            return (
                    IsConsumption(createMeteringPoint) ||
                    IsProduction(createMeteringPoint)) &&
                    NetSettlementGroupIsNotZeroOrNinetyNine(createMeteringPoint.NetSettlementGroup);
        }

        private static bool IsProduction(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.Production.Name,
                StringComparison.Ordinal);
        }

        private static bool IsConsumption(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.Consumption.Name,
                StringComparison.Ordinal);
        }

        private static bool NetSettlementGroupIsNotZeroOrNinetyNine(string netSettlement)
        {
            return netSettlement.Equals(
                NetSettlementGroup.Ninetynine.Name,
                StringComparison.Ordinal) ||
                netSettlement.Equals(
                NetSettlementGroup.Zero.Name,
                StringComparison.Ordinal);
        }

        private static bool SubTypeIsPhysicalOrVirtualOrCalculated(CreateMeteringPoint createMeteringPoint)
        {
            if (GroupOfMeteringPointTypesThatMustBeSubTypeVirtualOrCalculated(createMeteringPoint))
            {
                return SubTypeIsVirtualOrCalculated(createMeteringPoint);
            }

            return Physical(createMeteringPoint.SubTypeOfMeteringPoint) ||
                   Virtual(createMeteringPoint.SubTypeOfMeteringPoint) ||
                   Calculated(createMeteringPoint.SubTypeOfMeteringPoint);
        }

        private static bool SubTypeIsVirtualOrCalculated(CreateMeteringPoint createMeteringPoint)
        {
            return Virtual(createMeteringPoint.SubTypeOfMeteringPoint) ||
                   Calculated(createMeteringPoint.SubTypeOfMeteringPoint);
        }

        private static bool IsWholeSaleServices(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.WholesaleServices.Name,
                StringComparison.Ordinal);
        }

        private static bool IsOwnProduction(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.OwnProduction.Name,
                StringComparison.Ordinal);
        }

        private static bool IsNetFromGrid(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.NetFromGrid.Name,
                StringComparison.Ordinal);
        }

        private static bool IsNetToGrid(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.NetToGrid.Name,
                StringComparison.Ordinal);
        }

        private static bool IsTotalConsumption(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.TotalConsumption.Name,
                StringComparison.Ordinal);
        }

        private static bool Physical(string subTypeOfMeteringPoint)
        {
            return subTypeOfMeteringPoint.Equals(MeteringPointSubType.Physical.Name, StringComparison.Ordinal);
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
