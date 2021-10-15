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
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class PowerPlantMustBeValidRule : AbstractValidator<MasterDataDocument>
    {
        public PowerPlantMustBeValidRule()
        {
            When(
                MandatoryGroupOfMeteringPointTypes,
                PowerPlantMustNotBeEmpty);
            When(
                NotAllowedGroupOfMeteringPointTypes,
                PowerPlantMustBeEmpty);
            When(
                createMeteringPoint => createMeteringPoint.PowerPlant?.Length > 0,
                PowerPlantValueMustBeValid);
        }

        private static bool MandatoryGroupOfMeteringPointTypes(MasterDataDocument masterDataDocument)
        {
            return new HashSet<string>
                    {
                        MeteringPointType.Production.Name,
                        MeteringPointType.VEProduction.Name,
                    }
                .Contains(masterDataDocument.TypeOfMeteringPoint);
        }

        private static bool NotAllowedGroupOfMeteringPointTypes(MasterDataDocument masterDataDocument)
        {
            return new HashSet<string>
                    {
                        MeteringPointType.Analysis.Name,
                        MeteringPointType.Exchange.Name,
                        MeteringPointType.ExchangeReactiveEnergy.Name,
                        MeteringPointType.InternalUse.Name,
                        MeteringPointType.NetConsumption.Name,
                    }
                .Contains(masterDataDocument.TypeOfMeteringPoint);
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
                .Must(powerPlantGsrn => GsrnNumber.CheckRules(powerPlantGsrn!).Success)
                .WithState(
                    createMeteringPoint =>
                        new PowerPlantGsrnEan18ValidValidationError(
                            createMeteringPoint.GsrnNumber,
                            createMeteringPoint.PowerPlant!));
        }
    }
}
