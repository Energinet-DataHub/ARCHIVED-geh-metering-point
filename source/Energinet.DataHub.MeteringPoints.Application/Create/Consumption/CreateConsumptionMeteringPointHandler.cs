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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.GridAreas;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;
using ConsumptionMeteringPoint = Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.ConsumptionMeteringPoint;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Consumption
{
    public class CreateConsumptionMeteringPointHandler : IBusinessRequestHandler<CreateConsumptionMeteringPoint>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IGridAreaRepository _gridAreaRepository;
        private readonly IMediator _mediator;
        private readonly IBusinessProcessValidationContext _validationContext;

        public CreateConsumptionMeteringPointHandler(
            IMeteringPointRepository meteringPointRepository,
            IGridAreaRepository gridAreaRepository,
            IMediator mediator,
            IBusinessProcessValidationContext validationContext)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _gridAreaRepository = gridAreaRepository;
            _mediator = mediator;
            _validationContext = validationContext ?? throw new ArgumentNullException(nameof(validationContext));
        }

        public async Task<BusinessProcessResult> Handle(CreateConsumptionMeteringPoint request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var validationResult = await ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            var addressValidationResult = ValidateAddress(request);
            if (!addressValidationResult.Success)
            {
                return new BusinessProcessResult(request.TransactionId, addressValidationResult.Errors);
            }

            var gridArea = await GetGridAreaAsync(request).ConfigureAwait(false);
            var gridAreaValidationResult = ValidateGridArea(request, gridArea);
            if (!gridAreaValidationResult.Success)
            {
                return new BusinessProcessResult(request.TransactionId, gridAreaValidationResult.ValidationErrors);
            }

            var meteringPointDetails = CreateDetails(request, gridArea?.DefaultLink.Id!);
            var rulesCheckResult = CheckBusinessRules(request, meteringPointDetails);
            if (!rulesCheckResult.Success)
            {
                return rulesCheckResult;
            }

            _meteringPointRepository.Add(ConsumptionMeteringPoint.Create(meteringPointDetails));

            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private static BusinessProcessResult ValidateGridArea(CreateConsumptionMeteringPoint request, GridArea? gridArea)
        {
            var validationRules = new List<IBusinessRule>
            {
                new GridAreaMustExistRule(gridArea),
            };

            return new BusinessProcessResult(request.TransactionId, validationRules);
        }

        private static BusinessProcessResult CheckBusinessRules(CreateConsumptionMeteringPoint request, ConsumptionMeteringPointDetails meteringPointDetails)
        {
            var validationResult = ConsumptionMeteringPoint.CanCreate(meteringPointDetails);
            return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
        }

        private static ConsumptionMeteringPointDetails CreateDetails(CreateConsumptionMeteringPoint request, GridAreaLinkId gridAreaLinkId)
        {
            return new ConsumptionMeteringPointDetails(
                EnumerationType.FromName<SettlementMethod>(request.SettlementMethod!),
                !string.IsNullOrEmpty(request.ScheduledMeterReadingDate) ? ScheduledMeterReadingDate.Create(request.ScheduledMeterReadingDate !) : null,
                !string.IsNullOrEmpty(request.AssetType) ? EnumerationType.FromName<AssetType>(request.AssetType) : null !,
                !string.IsNullOrWhiteSpace(request.PhysicalConnectionCapacity) ? Capacity.Create(request.PhysicalConnectionCapacity) : null,
                MeteringPointId.New(),
                GsrnNumber.Create(request.GsrnNumber),
                CreateAddress(request),
                EnumerationType.FromName<MeteringMethod>(request.MeteringMethod),
                gridAreaLinkId,
                !string.IsNullOrEmpty(request.PowerPlant) ? GsrnNumber.Create(request.PowerPlant) : null !,
                LocationDescription.Create(request.LocationDescription!),
                string.IsNullOrWhiteSpace(request.MeterNumber) ? null : MeterId.Create(request.MeterNumber),
                EnumerationType.FromName<ReadingOccurrence>(request.MeterReadingOccurrence),
                PowerLimit.Create(request.MaximumPower, request.MaximumCurrent),
                EffectiveDate.Create(request.EffectiveDate),
                EnumerationType.FromName<NetSettlementGroup>(request.NetSettlementGroup!),
                EnumerationType.FromName<DisconnectionType>(request.DisconnectionType),
                !string.IsNullOrWhiteSpace(request.ConnectionType) ? EnumerationType.FromName<ConnectionType>(request.ConnectionType!) : null);
        }

        private static Domain.Addresses.Address CreateAddress(CreateConsumptionMeteringPoint request)
        {
            return Domain.Addresses.Address.Create(
                streetName: request.StreetName,
                streetCode: request.StreetCode,
                buildingNumber: request.BuildingNumber,
                city: request.CityName,
                citySubDivision: request.CitySubDivisionName,
                postCode: request.PostCode,
                countryCode: EnumerationType.FromName<CountryCode>(request.CountryCode),
                floor: request.FloorIdentification,
                room: request.RoomIdentification,
                municipalityCode: string.IsNullOrWhiteSpace(request.MunicipalityCode) ? default : int.Parse(request.MunicipalityCode, NumberStyles.Integer, new NumberFormatInfo()),
                isActual: request.IsActualAddress.GetValueOrDefault(),
                geoInfoReference: string.IsNullOrWhiteSpace(request.GeoInfoReference) ? default : Guid.Parse(request.GeoInfoReference));
        }

        private static BusinessRulesValidationResult ValidateAddress(CreateConsumptionMeteringPoint request)
        {
            var municipalityCode = int.Parse(request.MunicipalityCode, NumberStyles.Integer, new NumberFormatInfo());
            return Domain.Addresses.Address.CheckRules(
                streetName: request.StreetName,
                streetCode: request.StreetCode,
                buildingNumber: request.BuildingNumber,
                city: request.CityName,
                citySubDivision: request.CitySubDivisionName,
                postCode: request.PostCode,
                countryCode: EnumerationType.FromName<CountryCode>(request.CountryCode),
                floor: request.FloorIdentification,
                room: request.RoomIdentification,
                municipalityCode: municipalityCode,
                locationDescription: request.LocationDescription);
        }

        private Task<GridArea?> GetGridAreaAsync(CreateConsumptionMeteringPoint request)
        {
            return _gridAreaRepository.GetByCodeAsync(request.MeteringGridArea);
        }

        private async Task<BusinessProcessResult> ValidateAsync(CreateConsumptionMeteringPoint request, CancellationToken cancellationToken)
        {
            var gsrnNumberExists = await _mediator.Send(new MeteringPointGsrnExistsQuery(request.GsrnNumber), cancellationToken).ConfigureAwait(false);

            var validationRules = new List<IBusinessRule>
            {
                new MeteringPointGsrnMustBeUniqueRule(gsrnNumberExists, request.GsrnNumber),
            };

            return new BusinessProcessResult(request.TransactionId, validationRules);
        }
    }
}
