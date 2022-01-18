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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.Policies;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption
{
    public class ChangeMasterDataHandler : IBusinessRequestHandler<ChangeMasterDataRequest>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;
        private readonly ChangeMasterDataSettings _settings;
        private readonly ChangeMeteringPointAuthorizer _authorizer;

        public ChangeMasterDataHandler(
            IMeteringPointRepository meteringPointRepository,
            ISystemDateTimeProvider systemDateTimeProvider,
            ChangeMasterDataSettings settings,
            ChangeMeteringPointAuthorizer authorizer)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _systemDateTimeProvider = systemDateTimeProvider ?? throw new ArgumentNullException(nameof(systemDateTimeProvider));
            _settings = settings;
            _authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
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

            var authorizationResult = await _authorizer.AuthorizeAsync(targetMeteringPoint).ConfigureAwait(false);
            if (authorizationResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, authorizationResult.Errors);
            }

            var masterDataUpdater = CreateMasterDataUpdater(request, targetMeteringPoint);

            var valueValidation = masterDataUpdater.Validate();
            if (valueValidation.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, valueValidation.Errors);
            }

            var updatedMasterData = masterDataUpdater.Build();

            var policyCheckResult = await CheckPoliciesAsync(updatedMasterData).ConfigureAwait(false);
            if (policyCheckResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, policyCheckResult.Errors);
            }

            var validationResult = targetMeteringPoint.CanUpdateMasterData(updatedMasterData, GetMasterValidator());
            if (validationResult.Success != true)
            {
                return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
            }

            targetMeteringPoint.UpdateMasterData(updatedMasterData, GetMasterValidator());
            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private static MasterDataValidator GetMasterValidator()
        {
            return new MasterDataValidator();
        }

        private static MasterDataUpdater CreateMasterDataUpdater(ChangeMasterDataRequest request, MeteringPoint targetMeteringPoint)
        {
            var masterDataUpdater = new MasterDataUpdater(
                new MasterDataFieldSelector().GetMasterDataFieldsFor(targetMeteringPoint.MeteringPointType),
                targetMeteringPoint.MasterData);

            masterDataUpdater
                .EffectiveOn(request.EffectiveDate)
                .WithMeteringConfiguration(request.MeteringMethod, request.MeterId)
                .WithProductType(request.ProductType)
                .WithMeasurementUnitType(request.UnitType)
                .WithAssetType(request.AssetType)
                .WithReadingPeriodicity(request.ReadingPeriodicity)
                .WithPowerLimit(request.MaximumPower, request.MaximumCurrent)
                .WithPowerPlant(request.PowerPlantGsrnNumber)
                .WithCapacity(request.CapacityInKw)
                .WithSettlementMethod(request.SettlementMethod)
                .WithScheduledMeterReadingDate(request.ScheduledMeterReadingDate)
                .WithConnectionType(request.ConnectionType)
                .WithDisconnectionType(request.DisconnectionType)
                .WithNetSettlementGroup(request.NetSettlementGroup)
                .WithProductionObligation(request.ProductionObligation)
                .WithAddress(
                    request.Address?.StreetName,
                    request.Address?.StreetCode,
                    request.Address?.BuildingNumber,
                    request.Address?.City,
                    request.Address?.CitySubDivision,
                    request.Address?.PostCode,
                    string.IsNullOrEmpty(request.Address?.CountryCode) ? null : EnumerationType.FromName<CountryCode>(request.Address.CountryCode),
                    request.Address?.Floor,
                    request.Address?.Room,
                    request.Address?.MunicipalityCode,
                    request.Address?.IsActual,
                    request.Address?.GeoInfoReference,
                    request.Address?.LocationDescription);
            return masterDataUpdater;
        }

        private async Task<MeteringPoint?> FetchTargetMeteringPointAsync(ChangeMasterDataRequest request)
        {
            return await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false);
        }

        private Task<BusinessRulesValidationResult> CheckPoliciesAsync(MasterData masterData)
        {
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            var validationResults = new List<BusinessRulesValidationResult>()
            {
                new EffectiveDatePolicy(_settings.NumberOfDaysEffectiveDateIsAllowedToBeforeToday).Check(_systemDateTimeProvider.Now(), masterData.EffectiveDate!),
            };

            var validationErrors = validationResults.SelectMany(results => results.Errors).ToList();
            return Task.FromResult<BusinessRulesValidationResult>(new BusinessRulesValidationResult(validationErrors));
        }
    }
}
