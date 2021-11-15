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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exchange;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Exchange
{
    public class CreateExchangeMeteringPointHandler : IBusinessRequestHandler<CreateExchangeMeteringPoint>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IGridAreaRepository _gridAreaRepository;
        private readonly IMediator _mediator;
        private readonly IBusinessProcessValidationContext _validationContext;

        public CreateExchangeMeteringPointHandler(
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

        public async Task<BusinessProcessResult> Handle(CreateExchangeMeteringPoint request, CancellationToken cancellationToken)
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

            var (gridArea, toGridArea, fromGridArea) = await GetGridAreasAsync(request).ConfigureAwait(false);

            var gridAreaValidationResult = ValidateGridAreas(request, gridArea, fromGridArea, toGridArea);
            if (!gridAreaValidationResult.Success)
            {
                return new BusinessProcessResult(request.TransactionId, gridAreaValidationResult.ValidationErrors);
            }

            var meteringPointDetails = CreateDetails(request, gridArea?.DefaultLink.Id!, toGridArea?.DefaultLink.Id!, gridArea?.DefaultLink.Id!);
            var rulesCheckResult = CheckBusinessRules(request, meteringPointDetails);
            if (!rulesCheckResult.Success)
            {
                return rulesCheckResult;
            }

            _meteringPointRepository.Add(ExchangeMeteringPoint.Create(meteringPointDetails));

            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private static BusinessProcessResult ValidateGridAreas(IBusinessRequest request, GridArea? gridArea, GridArea? fromGrid, GridArea? toGrid)
        {
            var validationRules = new List<IBusinessRule> { new GridAreaMustExistRule(gridArea), new FromGridAreaMustExistRule(fromGrid), new ToGridAreaMustExistRule(toGrid) };

            return new BusinessProcessResult(request.TransactionId, validationRules);
        }

        private static BusinessProcessResult CheckBusinessRules(IBusinessRequest request, ExchangeMeteringPointDetails meteringPointDetails)
        {
            var validationResult = ExchangeMeteringPoint.CanCreate(meteringPointDetails);
            return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
        }

        private static ExchangeMeteringPointDetails CreateDetails(CreateExchangeMeteringPoint request, GridAreaLinkId gridAreaLinkId, GridAreaLinkId toGrid, GridAreaLinkId fromGrid)
        {
            return new ExchangeMeteringPointDetails(
                MeteringPointId.New(),
                GsrnNumber.Create(request.GsrnNumber),
                CreateAddress(request),
                EnumerationType.FromName<MeteringMethod>(request.MeteringMethod),
                gridAreaLinkId,
                string.IsNullOrWhiteSpace(request.MeterNumber) ? null : MeterId.Create(request.MeterNumber),
                EnumerationType.FromName<ReadingOccurrence>(request.MeterReadingOccurrence),
                PowerLimit.Create(request.MaximumPower, request.MaximumCurrent),
                EffectiveDate.Create(request.EffectiveDate),
                toGrid,
                fromGrid);
        }

        private static Domain.Addresses.Address CreateAddress(CreateExchangeMeteringPoint request)
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
                geoInfoReference: string.IsNullOrWhiteSpace(request.GeoInfoReference) ? default : Guid.Parse(request.GeoInfoReference),
                locationDescription: request.LocationDescription);
        }

        private static BusinessRulesValidationResult ValidateAddress(CreateExchangeMeteringPoint request)
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

        private async Task<(GridArea? GridArea, GridArea? ToGridArea, GridArea? FromGridArea)> GetGridAreasAsync(CreateExchangeMeteringPoint request)
        {
            var gridArea = await _gridAreaRepository.GetByCodeAsync(request.MeteringGridArea).ConfigureAwait(false);
            var toGridArea = await _gridAreaRepository.GetByCodeAsync(request.ToGrid).ConfigureAwait(false);
            var fromGridArea = await _gridAreaRepository.GetByCodeAsync(request.FromGrid).ConfigureAwait(false);

            return (gridArea, toGridArea, fromGridArea);
        }

        private async Task<BusinessProcessResult> ValidateAsync(CreateExchangeMeteringPoint request, CancellationToken cancellationToken)
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
