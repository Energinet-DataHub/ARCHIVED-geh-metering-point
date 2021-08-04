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
using System.Data;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class PowerPlantMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        private const int RequiredIdLength = 18;

        public PowerPlantMustBeValidRule()
        {
            When(
                PowerPlantMustNotBeEmptyGroupOfMeteringPointTypes,
                PowerPlantMustNotBeEmpty);
            When(
                PowerPlantNotAllowedGroupOfMeteringPointTypes,
                PowerPlantMustBeEmpty);
            When(
                createMeteringPoint => createMeteringPoint.PowerPlant.Length > 0,
                PowerPlantValueMustBeValid);
        }

        private static bool IsVeProduction(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.VEProduction.Name,
                StringComparison.Ordinal);
        }

        private static bool IsProduction(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.Production.Name,
                StringComparison.Ordinal);
        }

        private static bool PowerPlantMustNotBeEmptyGroupOfMeteringPointTypes(CreateMeteringPoint createMeteringPoint)
        {
            return IsProduction(createMeteringPoint) ||
                   IsVeProduction(createMeteringPoint) ||
                   IsConsumptionAndNotZeroOrNinetyNine(createMeteringPoint);
        }

        private static bool IsAnalysis(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.Analysis.Name,
                StringComparison.Ordinal);
        }

        private static bool IsInternalUse(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.InternalUse.Name,
                StringComparison.Ordinal);
        }

        private static bool IsNetConsumption(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.NetConsumption.Name,
                StringComparison.Ordinal);
        }

        private static bool IsExchangeReactiveEnergy(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.ExchangeReactiveEnergy.Name,
                StringComparison.Ordinal);
        }

        private static bool IsExchange(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.Exchange.Name,
                StringComparison.Ordinal);
        }

        private static bool IsConsumptionAndNotZeroOrNinetyNine(CreateMeteringPoint createMeteringPoint)
        {
            return IsConsumption(createMeteringPoint) &&
             !IsNetSettlementGroupZero(createMeteringPoint) &&
             !IsNetSettlementGroupNinetyNine(createMeteringPoint);
        }

        private static bool IsConsumption(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(
                MeteringPointType.Consumption.Name,
                StringComparison.Ordinal);
        }

        private static bool IsNetSettlementGroupZero(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.NetSettlementGroup.Equals(
                NetSettlementGroup.Zero.Name,
                StringComparison.Ordinal);
        }

        private static bool IsNetSettlementGroupNinetyNine(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.NetSettlementGroup.Equals(
                       NetSettlementGroup.Ninetynine.Name,
                       StringComparison.Ordinal);
        }

        private static bool PowerPlantNotAllowedGroupOfMeteringPointTypes(CreateMeteringPoint createMeteringPoint)
        {
            return IsAnalysis(createMeteringPoint) ||
                   IsInternalUse(createMeteringPoint) ||
                   IsNetConsumption(createMeteringPoint) ||
                   IsExchangeReactiveEnergy(createMeteringPoint) ||
                   IsExchange(createMeteringPoint);
        }

        private static int Parse(string input)
        {
            return int.Parse(input, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        private static bool CheckSumIsValid(string powerPlantGsrn)
        {
            int definedChecksumDigit = Parse(powerPlantGsrn.Substring(powerPlantGsrn.Length - 1));
            int calculatedChecksum = CalculateChecksum(powerPlantGsrn);
            return calculatedChecksum == definedChecksumDigit;
        }

        private static int CalculateChecksum(string powerPlantGsrn)
        {
            int sum = 0;
            bool positionIsOdd = true;
            for (int currentPosition = 1; currentPosition < RequiredIdLength; currentPosition++)
            {
                int currentValueAtPosition = Parse(powerPlantGsrn.Substring(currentPosition - 1, 1));
                if (positionIsOdd)
                {
                    sum = sum + (currentValueAtPosition * 3);
                }
                else
                {
                    sum = sum + (currentValueAtPosition * 1);
                }

                positionIsOdd = !positionIsOdd;
            }

            int equalOrHigherMultipleOf = (int)(Math.Ceiling(sum / 10.0) * 10);

            return equalOrHigherMultipleOf - sum;
        }

        private void PowerPlantMustBeEmpty()
        {
            RuleFor(createmeteringPoint => createmeteringPoint.PowerPlant)
                .Empty()
                .WithState(createMeteringPoint => new PowerPlantValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.PowerPlant));
        }

        private void PowerPlantMustNotBeEmpty()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.PowerPlant)
                .NotEmpty()
                .WithState(createMeteringPoint => new PowerPlantValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.PowerPlant));
        }

        private void PowerPlantMustStartWith57()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.PowerPlant)
                .Must(powerPlant => powerPlant.StartsWith("57", StringComparison.Ordinal))
                .WithState(createMeteringPoint =>
                    new PowerPlantGsrnEan18ValidValidationError(
                        createMeteringPoint.GsrnNumber,
                        createMeteringPoint.PowerPlant));
        }

        private void PowerPlantIsValidGsrnEan18Code()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.PowerPlant)
                .Must(powerPlant => powerPlant.Length == RequiredIdLength && CheckSumIsValid(powerPlant))
                .WithState(createMeteringPoint =>
                    new PowerPlantGsrnEan18ValidValidationError(
                        createMeteringPoint.GsrnNumber,
                        createMeteringPoint.PowerPlant));
        }

        private void PowerPlantValueMustBeValid()
        {
            PowerPlantMustStartWith57();
            PowerPlantIsValidGsrnEan18Code();
        }
    }
}
