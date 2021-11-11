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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.Policies;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption
{
    public class ChangeMasterDataHandler : IBusinessRequestHandler<ChangeMasterDataRequest>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;
        private readonly ChangeMasterDataSettings _settings;

        public ChangeMasterDataHandler(IMeteringPointRepository meteringPointRepository, ISystemDateTimeProvider systemDateTimeProvider, ChangeMasterDataSettings settings)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _systemDateTimeProvider = systemDateTimeProvider ?? throw new ArgumentNullException(nameof(systemDateTimeProvider));
            _settings = settings;
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

            var details = CreateChangeDetailsFrom(request);
            var changeHandler =
                new ChangeMasterDataFacade(targetMeteringPoint, details);

            var checkResult = await CheckIfMasterDataCanBeChangedAsync(changeHandler, details).ConfigureAwait(false);
            if (checkResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, checkResult.Errors);
            }

            changeHandler.Change();
            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private static MasterDataDetails CreateChangeDetailsFrom(ChangeMasterDataRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new MasterDataDetails(EffectiveDate.Create(request.EffectiveDate))
                with
                {
                    Address = CreateNewAddressFrom(request),
                    MeterId = request.MeterId.Length == 0 ? MeterId.Empty() : null,
                };
        }

        private static Domain.Addresses.Address? CreateNewAddressFrom(ChangeMasterDataRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Address is null)
            {
                return null;
            }

            return Domain.Addresses.Address.Create(
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
                request.Address.GeoInfoReference);
        }

        private async Task<ConsumptionMeteringPoint?> FetchTargetMeteringPointAsync(ChangeMasterDataRequest request)
        {
            return await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false) as ConsumptionMeteringPoint;
        }

        private Task<BusinessRulesValidationResult> CheckIfMasterDataCanBeChangedAsync(ChangeMasterDataFacade handler, MasterDataDetails details)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (details == null) throw new ArgumentNullException(nameof(details));
            var validationResults = new List<BusinessRulesValidationResult>()
            {
                handler.CanChange(),
                new EffectiveDatePolicy(_settings.NumberOfDaysEffectiveDateIsAllowedToBeforeToday).Check(_systemDateTimeProvider.Now(), details.EffectiveDate),
            };

            var validationErrors = validationResults.SelectMany(results => results.Errors).ToList();
            return Task.FromResult<BusinessRulesValidationResult>(new BusinessRulesValidationResult(validationErrors));
        }
    }
}
