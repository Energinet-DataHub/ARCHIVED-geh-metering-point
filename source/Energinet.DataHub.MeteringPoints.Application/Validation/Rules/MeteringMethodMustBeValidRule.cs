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
using System.Data;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class MeteringMethodMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        public MeteringMethodMustBeValidRule()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.MeteringMethod)
                .NotEmpty()
                .WithState(createMeteringPoint => new MeteringPointSubTypeMandatoryValidationError(createMeteringPoint.GsrnNumber));

            When(GroupOfMeteringPointTypesThatMustBeSubTypeVirtualOrCalculated, () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint)
                    .Must(SubTypeIsVirtualOrCalculated)
                    .WithState(createMeteringPoint =>
                        new MeteringPointSubTypeValueMustBeValidValidationError(
                            createMeteringPoint.GsrnNumber,
                            createMeteringPoint.MeteringMethod));
            });

            When(GroupOfMeteringPointTypesThatMustBeSubtypePhysicalOrVirtual, () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint)
                    .Must(SubTypeIsVirtualOrPhysical)
                    .WithState(createMeteringPoint => new MeteringPointSubTypeMustBePhysicalOrVirtualValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.MeteringMethod));
            });

            When(MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNine, () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint)
                    .Must(SubTypeIsVirtualOrCalculated)
                    .WithState(createMeteringPoint =>
                        new MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNineMustBeSubtypeVirtualOrCalculatedValidationError(
                            createMeteringPoint.GsrnNumber,
                            createMeteringPoint.MeteringMethod,
                            createMeteringPoint.TypeOfMeteringPoint));
            });

            When(ExpectedValueForAllOtherMeteringPointTypes, () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint)
                    .Must(ExpectedSubTypes)
                    .WithState(createMeteringPoint =>
                        new MeteringPointSubTypeValueMustBeValidValidationError(
                            createMeteringPoint.GsrnNumber,
                            createMeteringPoint.MeteringMethod));
            });
        }

        private static bool ExpectedValueForAllOtherMeteringPointTypes(CreateMeteringPoint createMeteringPoint)
        {
            return !GroupOfMeteringPointTypesThatMustBeSubtypePhysicalOrVirtual(createMeteringPoint) &&
                   !GroupOfMeteringPointTypesThatMustBeSubTypeVirtualOrCalculated(createMeteringPoint) &&
                   !MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNine(createMeteringPoint) &&
                   !IsMeteringPointExchangeReactiveEnergy(createMeteringPoint);
        }

        private static bool IsMeteringPointExchangeReactiveEnergy(CreateMeteringPoint createMeteringPoint)
        {
            return MeteringPointType.ExchangeReactiveEnergy.Name.Equals(
                createMeteringPoint.TypeOfMeteringPoint,
                StringComparison.Ordinal);
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
                .Contains(createMeteringPoint.TypeOfMeteringPoint);
        }

        private static bool GroupOfMeteringPointTypesThatMustBeSubtypePhysicalOrVirtual(
            CreateMeteringPoint createMeteringPoint)
        {
            return new HashSet<string>
                {
                    MeteringPointType.OtherConsumption.Name,
                    MeteringPointType.OtherProduction.Name,
                    MeteringPointType.ExchangeReactiveEnergy.Name,
                }
                .Contains(createMeteringPoint.TypeOfMeteringPoint);
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

        private static bool SubTypeIsVirtualOrCalculated(CreateMeteringPoint createMeteringPoint)
        {
            return Virtual(createMeteringPoint.MeteringMethod) ||
                   Calculated(createMeteringPoint.MeteringMethod);
        }

        private static bool SubTypeIsVirtualOrPhysical(CreateMeteringPoint createMeteringPoint)
        {
            return Virtual(createMeteringPoint.MeteringMethod) ||
                   Physical(createMeteringPoint.MeteringMethod);
        }

        private static bool ExpectedSubTypes(CreateMeteringPoint createMeteringPoint)
        {
            return new HashSet<string>
                    {
                        MeteringMethod.Calculated.Name,
                        MeteringMethod.Physical.Name,
                        MeteringMethod.Virtual.Name,
                    }
                .Contains(createMeteringPoint.MeteringMethod);
        }

        private static bool Virtual(string subTypeOfMeteringPoint)
        {
            return subTypeOfMeteringPoint.Equals(MeteringMethod.Virtual.Name, StringComparison.Ordinal);
        }

        private static bool Calculated(string subTypeOfMeteringPoint)
        {
            return subTypeOfMeteringPoint.Equals(MeteringMethod.Calculated.Name, StringComparison.Ordinal);
        }

        private static bool Physical(string subTypeOfMeteringPoint)
        {
            return subTypeOfMeteringPoint.Equals(MeteringMethod.Physical.Name, StringComparison.Ordinal);
        }
    }
}
