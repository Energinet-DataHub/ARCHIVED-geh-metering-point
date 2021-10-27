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
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.MarketDocuments.Validation
{
    public class MeasureUnitTypeRule : AbstractValidator<MasterDataDocument>
    {
        public MeasureUnitTypeRule()
        {
            RuleFor(request => request.MeasureUnitType)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithState(createMeteringPoint => new MeasureUnitTypeMandatoryValidationError(createMeteringPoint.GsrnNumber))
                .Must(measureUnitType => AllowedMeasureUnitTypes().Contains(measureUnitType))
                .WithState(createMeteringPoint => new MeasureUnitTypeInvalidValueValidationError(createMeteringPoint.MeasureUnitType, createMeteringPoint.GsrnNumber));

            When(point => point.TypeOfMeteringPoint == MeteringPointType.Exchange.Name, () =>
            {
                RuleFor(request => request.MeasureUnitType)
                    .Must(measureUnitType => measureUnitType == MeasurementUnitType.KWh.Name || measureUnitType == MeasurementUnitType.KVArh.Name)
                    .WithState(createMeteringPoint => new MeasureUnitTypeInvalidValueValidationError(createMeteringPoint.MeasureUnitType, createMeteringPoint.GsrnNumber));
            });

            When(point => point.TypeOfMeteringPoint == MeteringPointType.OtherConsumption.Name || point.TypeOfMeteringPoint == MeteringPointType.OtherProduction.Name, () =>
            {
                RuleFor(request => request.MeasureUnitType)
                    .Must(measureUnitType => measureUnitType == MeasurementUnitType.KWh.Name || measureUnitType == MeasurementUnitType.MWh.Name)
                    .WithState(createMeteringPoint => new MeasureUnitTypeInvalidValueValidationError(createMeteringPoint.MeasureUnitType, createMeteringPoint.GsrnNumber));
            });

            When(point => point.TypeOfMeteringPoint == MeteringPointType.ExchangeReactiveEnergy.Name, () =>
            {
                RuleFor(request => request.MeasureUnitType)
                    .Must(measureUnitType => measureUnitType == MeasurementUnitType.KVArh.Name)
                    .WithState(createMeteringPoint => new MeasureUnitTypeInvalidValueValidationError(createMeteringPoint.MeasureUnitType, createMeteringPoint.GsrnNumber));
            });
        }

        private static HashSet<string> AllowedMeasureUnitTypes()
        {
            return new()
            {
                MeasurementUnitType.KWh.Name,
                MeasurementUnitType.KVArh.Name,
                MeasurementUnitType.KW.Name,
                MeasurementUnitType.MW.Name,
                MeasurementUnitType.MWh.Name,
                MeasurementUnitType.Tonne.Name,
                MeasurementUnitType.MVAr.Name,
                MeasurementUnitType.DanishTariffCode.Name,
            };
        }
    }
}
