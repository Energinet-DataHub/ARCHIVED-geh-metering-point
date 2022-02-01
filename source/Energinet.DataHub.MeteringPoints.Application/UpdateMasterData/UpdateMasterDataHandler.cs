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
using Energinet.DataHub.MeteringPoints.Application.Common.ChildMeteringPoints;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.Policies;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.UpdateMasterData
{
    public class UpdateMasterDataHandler : IBusinessRequestHandler<UpdateMasterDataRequest>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;
        private readonly UpdateMasterDataPolicies _policies;
        private readonly UpdateMeteringPointAuthorizer _authorizer;
        private readonly ParentChildCouplingHandler _parentChildCouplingHandler;
        private readonly UpdateMasterDataProcess _updateMasterDataProcess;

        public UpdateMasterDataHandler(
            IMeteringPointRepository meteringPointRepository,
            ISystemDateTimeProvider systemDateTimeProvider,
            UpdateMasterDataPolicies policies,
            UpdateMeteringPointAuthorizer authorizer,
            ParentChildCouplingHandler parentChildCouplingHandler,
            UpdateMasterDataProcess updateMasterDataProcess)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _systemDateTimeProvider = systemDateTimeProvider ?? throw new ArgumentNullException(nameof(systemDateTimeProvider));
            _policies = policies;
            _authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
            _parentChildCouplingHandler = parentChildCouplingHandler;
            _updateMasterDataProcess = updateMasterDataProcess ?? throw new ArgumentNullException(nameof(updateMasterDataProcess));
        }

        public async Task<BusinessProcessResult> Handle(UpdateMasterDataRequest request, CancellationToken cancellationToken)
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

            var result = await _updateMasterDataProcess.UpdateAsync(targetMeteringPoint, masterDataUpdater).ConfigureAwait(false);
            if (result.Success == false)
            {
                return BusinessProcessResult.Fail(request.TransactionId, result.Errors.ToArray());
            }

            var parentCouplingResult = await HandleParentChildCouplingAsync(request, targetMeteringPoint).ConfigureAwait(false);
            return parentCouplingResult.Success == false ? parentCouplingResult : BusinessProcessResult.Ok(request.TransactionId);
        }

        private static MasterDataUpdater CreateMasterDataUpdater(UpdateMasterDataRequest request, MeteringPoint targetMeteringPoint)
        {
            var masterDataUpdater = new MasterDataUpdater(
                new MasterDataFieldSelector().GetMasterDataFieldsFor(targetMeteringPoint.MeteringPointType),
                targetMeteringPoint.MasterData);

            masterDataUpdater
                .EffectiveOn(request.EffectiveDate)
                .WithMeteringConfiguration(request.MeteringMethod, request.MeterNumber)
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
                    request.Address?.CountryCode,
                    request.Address?.Floor,
                    request.Address?.Room,
                    request.Address?.MunicipalityCode,
                    request.Address?.IsActual,
                    request.Address?.GeoInfoReference,
                    request.Address?.LocationDescription);
            return masterDataUpdater;
        }

        private async Task<MeteringPoint?> FetchTargetMeteringPointAsync(UpdateMasterDataRequest request)
        {
            return await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false);
        }

        private Task<BusinessProcessResult> HandleParentChildCouplingAsync(UpdateMasterDataRequest request, MeteringPoint meteringPoint)
        {
            if (request.ParentRelatedMeteringPoint is null)
                return Task.FromResult(BusinessProcessResult.Ok(request.TransactionId));

            if (request.ParentRelatedMeteringPoint.Length == 0)
            {
                return _parentChildCouplingHandler.DecoupleFromParentAsync(meteringPoint, request.TransactionId);
            }

            return _parentChildCouplingHandler.TryCoupleToParentAsync(meteringPoint, request.ParentRelatedMeteringPoint, request.TransactionId);
        }
    }
}
