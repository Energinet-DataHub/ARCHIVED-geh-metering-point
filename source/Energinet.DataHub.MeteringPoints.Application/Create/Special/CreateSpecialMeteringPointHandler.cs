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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Special;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Special
{
    public class CreateSpecialMeteringPointHandler : IBusinessRequestHandler<CreateSpecialMeteringPoint>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IGridAreaRepository _gridAreaRepository;
        private readonly IMediator _mediator;
        private readonly IBusinessProcessValidationContext _validationContext;

        public CreateSpecialMeteringPointHandler(
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

        public async Task<BusinessProcessResult> Handle(CreateSpecialMeteringPoint request, CancellationToken cancellationToken)
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

            var gridArea = await GetGridAreasAsync(request).ConfigureAwait(false);

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

            _meteringPointRepository.Add(SpecialMeteringPoint.Create(meteringPointDetails));

            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private static BusinessProcessResult ValidateGridArea(IBusinessRequest request, GridArea? gridArea)
        {
            var validationRules = new List<IBusinessRule> { new GridAreaMustExistRule(gridArea), };

            return new BusinessProcessResult(request.TransactionId, validationRules);
        }

        private static BusinessProcessResult CheckBusinessRules(IBusinessRequest request, SpecialMeteringPointDetails meteringPointDetails)
        {
            var validationResult = SpecialMeteringPoint.CanCreate(meteringPointDetails);
            return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
        }

        private static SpecialMeteringPointDetails CreateDetails(CreateSpecialMeteringPoint request, GridAreaLinkId gridAreaLinkId)
        {
            return new SpecialMeteringPointDetails(
                MeteringPointId.New(),
                EnumerationType.FromName<MeteringPointType>(request.MeteringPointType),
                GsrnNumber.Create(request.GsrnNumber),
                CreateAddress(request),
                EnumerationType.FromName<MeteringMethod>(request.MeteringMethod),
                gridAreaLinkId,
                LocationDescription.Create(request.LocationDescription!),
                string.IsNullOrWhiteSpace(request.MeterNumber) ? null : MeterId.Create(request.MeterNumber),
                EnumerationType.FromName<ReadingOccurrence>(request.MeterReadingOccurrence),
                PowerLimit.Create(request.MaximumPower, request.MaximumCurrent),
                EffectiveDate.Create(request.EffectiveDate),
                !string.IsNullOrEmpty(request.PowerPlant) ? GsrnNumber.Create(request.PowerPlant) : null!,
                !string.IsNullOrWhiteSpace(request.PhysicalConnectionCapacity) ? Capacity.Create(request.PhysicalConnectionCapacity) : null!,
                !string.IsNullOrEmpty(request.AssetType) ? EnumerationType.FromName<AssetType>(request.AssetType) : null!);
        }

        private static Domain.Addresses.Address CreateAddress(CreateSpecialMeteringPoint request)
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

        private static BusinessRulesValidationResult ValidateAddress(CreateSpecialMeteringPoint request)
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
                municipalityCode: municipalityCode);
        }

        private async Task<GridArea?> GetGridAreasAsync(CreateSpecialMeteringPoint request)
        {
            var gridArea = await _gridAreaRepository.GetByCodeAsync(request.MeteringGridArea).ConfigureAwait(false);

            return gridArea;
        }

        private async Task<BusinessProcessResult> ValidateAsync(ICreateMeteringPointRequest request, CancellationToken cancellationToken)
        {
            var gsrnNumberExists = await _mediator.Send(new MeteringPointGsrnExistsQuery(request.GsrnNumber), cancellationToken).ConfigureAwait(false);

            var validationRules = new List<IBusinessRule> { new MeteringPointGsrnMustBeUniqueRule(gsrnNumberExists, request.GsrnNumber), };

            return new BusinessProcessResult(request.TransactionId, validationRules);
        }
    }
}
