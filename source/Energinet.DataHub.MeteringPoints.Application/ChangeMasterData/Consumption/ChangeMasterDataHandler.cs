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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.Policies;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption
{
    #pragma warning disable
    public class ChangeMasterDataHandler : IBusinessRequestHandler<ChangeMasterDataRequest>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;

        public ChangeMasterDataHandler(IMeteringPointRepository meteringPointRepository, ISystemDateTimeProvider systemDateTimeProvider)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _systemDateTimeProvider = systemDateTimeProvider ?? throw new ArgumentNullException(nameof(systemDateTimeProvider));
        }

        public async Task<BusinessProcessResult> Handle(ChangeMasterDataRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var targetMeteringPoint = await FetchTargetMeteringPointAsync(request).ConfigureAwait(false);
            if (targetMeteringPoint == null)
            {
                return new BusinessProcessResult(request.TransactionId, new List<ValidationError>()
                {
                    new MeteringPointMustBeKnownValidationError(request.GsrnNumber),
                });
            }

            var validationResult = await ValidateAsync(request, targetMeteringPoint).ConfigureAwait(false);
            if (!validationResult.Success)
            {
                return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
            }

            return await ChangeMasterDataAsync(request, targetMeteringPoint).ConfigureAwait(false);
        }

        private static MasterDataDetails CreateChangeDetails(ChangeMasterDataRequest request, ConsumptionMeteringPoint targetMeteringPoint)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new MasterDataDetails(EffectiveDate.Create(request.EffectiveDate))
                with
                {
                    Address = CreateNewAddressFrom(request, targetMeteringPoint),
                };
        }

        private static Task<BusinessProcessResult> ChangeMasterDataAsync(ChangeMasterDataRequest request, ConsumptionMeteringPoint targetMeteringPoint)
        {
            targetMeteringPoint.Change(CreateChangeDetails(request, targetMeteringPoint));
            return Task.FromResult(BusinessProcessResult.Ok(request.TransactionId));
        }

        private static Domain.Addresses.Address? CreateNewAddressFrom(ChangeMasterDataRequest request, ConsumptionMeteringPoint targetMeteringPoint)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Address is null)
            {
                return null;
            }

            return targetMeteringPoint.Address.MergeFrom(Domain.Addresses.Address.Create(
                request.Address.StreetName,
                request.Address.StreetCode,
                request.Address.BuildingNumber,
                request.Address.City,
                request.Address.CitySubDivision,
                request.Address.PostCode,
                string.IsNullOrEmpty(request.Address.CountryCode) ? null : EnumerationType.FromName<CountryCode>(request.Address.CountryCode),
                request.Address.Floor,
                request.Address.Room,
                request.Address.MunicipalityCode,
                request.Address.IsActual.GetValueOrDefault(),
                request.Address.GeoInfoReference));
        }

        private Task<BusinessRulesValidationResult> ValidateAsync(ChangeMasterDataRequest request, ConsumptionMeteringPoint targetMeteringPoint)
        {
            var result = TimePeriodPolicy.Check( _systemDateTimeProvider.Now(), EffectiveDate.Create(request.EffectiveDate));
            if (result.Success == false)
            {
                return Task.FromResult(result);
            }

            return Task.FromResult(new BusinessRulesValidationResult(targetMeteringPoint.CanChange(CreateChangeDetails(request, targetMeteringPoint)).Errors));
        }

        private async Task<ConsumptionMeteringPoint?> FetchTargetMeteringPointAsync(ChangeMasterDataRequest request)
        {
            return await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false) as ConsumptionMeteringPoint;
        }
    }
}
