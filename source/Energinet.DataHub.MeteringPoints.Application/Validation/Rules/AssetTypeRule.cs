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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class AssetTypeRule : AbstractValidator<CreateMeteringPoint>
    {
        public AssetTypeRule()
        {
            When(IsProduction, () =>
            {
                RuleFor(request => request.AssetType)
                    .NotEmpty()
                    .WithState(request => new AssetTypeMandatoryValidationError(request.GsrnNumber));
            });

            When(IsConsumptionWithNetSettlementGroupNotZero, () =>
            {
                RuleFor(request => request.AssetType)
                    .NotEmpty()
                    .WithState(request => new AssetTypeMandatoryValidationError(request.GsrnNumber));
            });

            When(IsNotAllowedType, () =>
            {
                RuleFor(request => request.AssetType)
                    .Null()
                    .WithState(request => new AssetTypeNotAllowedValidationError(request.GsrnNumber, request.AssetType));
            });

            RuleFor(request => request.AssetType)
                .Must(IsNullOrEmptyOrValidDomainValue)
                .WithState(request => new AssetTypeWrongValueValidationError(request.GsrnNumber, request.AssetType));
        }

        private static bool IsConsumptionWithNetSettlementGroupNotZero(CreateMeteringPoint request)
        {
            return request.TypeOfMeteringPoint == MeteringPointType.Consumption.Name && request.NetSettlementGroup != NetSettlementGroup.Zero.Name;
        }

        private static bool IsProduction(CreateMeteringPoint request)
        {
            return request.TypeOfMeteringPoint == MeteringPointType.Production.Name;
        }

        private static bool IsNullOrEmptyOrValidDomainValue(string? assetType)
        {
            if (string.IsNullOrEmpty(assetType)) return true;

            var allAssetTypes = EnumerationType.GetAll<AssetType>().Select(x => x.Name).ToHashSet();

            return allAssetTypes.Contains(assetType);
        }

        private static bool IsNotAllowedType(CreateMeteringPoint request)
        {
            var notAllowedMeteringPointTypes = new HashSet<string>
            {
                MeteringPointType.Analysis.Name,
                MeteringPointType.ExchangeReactiveEnergy.Name,
                MeteringPointType.InternalUse.Name,
                MeteringPointType.Exchange.Name,
                MeteringPointType.GridLossCorrection.Name,
                MeteringPointType.ElectricalHeating.Name,
                MeteringPointType.NetConsumption.Name,
            };

            return notAllowedMeteringPointTypes.Contains(request.TypeOfMeteringPoint);
        }
    }
}
