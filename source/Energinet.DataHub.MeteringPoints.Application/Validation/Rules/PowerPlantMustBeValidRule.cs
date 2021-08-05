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
                MandatoryGroupOfMeteringPointTypes,
                PowerPlantMustNotBeEmpty);
            When(
                NotAllowedGroupOfMeteringPointTypes,
                PowerPlantMustBeEmpty);
            When(
                createMeteringPoint => createMeteringPoint.PowerPlant.Length > 0,
                PowerPlantValueMustBeValid);
        }

        private static bool MandatoryGroupOfMeteringPointTypes(CreateMeteringPoint createMeteringPoint)
        {
            return new HashSet<string>
                    {
                        MeteringPointType.Production.Name,
                        MeteringPointType.VEProduction.Name,
                    }
                .Contains(createMeteringPoint.TypeOfMeteringPoint) ||
                IsConsumptionAndNotZeroOrNinetyNine(createMeteringPoint);
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

        private static bool NotAllowedGroupOfMeteringPointTypes(CreateMeteringPoint createMeteringPoint)
        {
            return new HashSet<string>
                    {
                        MeteringPointType.Analysis.Name,
                        MeteringPointType.Exchange.Name,
                        MeteringPointType.ExchangeReactiveEnergy.Name,
                        MeteringPointType.InternalUse.Name,
                        MeteringPointType.NetConsumption.Name,
                    }
                .Contains(createMeteringPoint.TypeOfMeteringPoint);
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
            RuleFor(createMeteringPoint => createMeteringPoint.PowerPlant)
                .Empty()
                .WithState(createMeteringPoint => new PowerPlantValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.PowerPlant));
        }

        private void PowerPlantMustNotBeEmpty()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.PowerPlant)
                .NotEmpty()
                .WithState(createMeteringPoint => new PowerPlantValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.PowerPlant));
        }

        private void PowerPlantValueMustBeValid()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.PowerPlant)
                .Cascade(CascadeMode.Stop)
                .Must(powerPlant => powerPlant.StartsWith("57", StringComparison.Ordinal))
                .WithState(createMeteringPoint =>
                    new PowerPlantGsrnEan18ValidValidationError(
                        createMeteringPoint.GsrnNumber,
                        createMeteringPoint.PowerPlant));

            RuleFor(createMeteringPoint => createMeteringPoint.PowerPlant)
                .Must(powerPlant => powerPlant.Length == RequiredIdLength && CheckSumIsValid(powerPlant))
                .WithState(createMeteringPoint =>
                    new PowerPlantGsrnEan18ValidValidationError(
                        createMeteringPoint.GsrnNumber,
                        createMeteringPoint.PowerPlant));
        }
    }
}
