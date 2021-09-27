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
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;
using ConsumptionMeteringPoint = Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.ConsumptionMeteringPoint;

namespace Energinet.DataHub.MeteringPoints.Application.Create
{
    public class CreateMeteringPointHandler : IBusinessRequestHandler<CreateMeteringPoint>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IMediator _mediator;

        public CreateMeteringPointHandler(IMeteringPointRepository meteringPointRepository, IMediator mediator)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _mediator = mediator;
        }

        public async Task<BusinessProcessResult> Handle(CreateMeteringPoint request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var meteringPointDetails = CreateDetails(request);

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

            var meteringPointType = EnumerationType.FromName<MeteringPointType>(request.TypeOfMeteringPoint);

            var rulesCheckResult = CheckBusinessRules(request, meteringPointDetails);
            if (!rulesCheckResult.Success)
            {
                return rulesCheckResult;
            }

            MeteringPoint meteringPoint;

            switch (meteringPointType.Name)
            {
                case nameof(MeteringPointType.Consumption):
                    meteringPoint = CreateConsumptionMeteringPoint(request);
                    break;
                case nameof(MeteringPointType.Exchange):
                    meteringPoint = CreateExchangeMeteringPoint(request, meteringPointType);
                    break;
                case nameof(MeteringPointType.Production):
                    meteringPoint = CreateProductionMeteringPoint(request, meteringPointType);
                    break;
                default:
                    throw new NotImplementedException(
                        $"Could not create instance of MeteringPoint for type {meteringPointType.Name}");
            }

            _meteringPointRepository.Add(meteringPoint);

            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private static BusinessProcessResult CheckBusinessRules(CreateMeteringPoint request, MeteringPointDetails meteringPointDetails)
        {
            var meteringPointType = EnumerationType.FromName<MeteringPointType>(request.TypeOfMeteringPoint);
            if (meteringPointType != MeteringPointType.Consumption)
            {
                return BusinessProcessResult.Ok(request.TransactionId);
            }

            var validationResult = ConsumptionMeteringPoint.CanCreate(meteringPointDetails);

            return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
        }

        private static ProductionMeteringPoint CreateProductionMeteringPoint(CreateMeteringPoint request, MeteringPointType meteringPointType)
        {
           return new(
                MeteringPointId.New(),
                GsrnNumber.Create(request.GsrnNumber),
                CreateAddress(request),
                request.IsOfficialAddress.GetValueOrDefault(), // TODO: change to boolean in domain?
                EnumerationType.FromName<MeteringPointSubType>(request.SubTypeOfMeteringPoint),
                meteringPointType,
                new GridAreaId(Guid.NewGuid()),
                !string.IsNullOrEmpty(request.PowerPlant) ? GsrnNumber.Create(request.PowerPlant) : null,
                request.LocationDescription,
                EnumerationType.FromName<MeasurementUnitType>(request.UnitType),
                request.MeterNumber,
                EnumerationType.FromName<ReadingOccurrence>(request.MeterReadingOccurrence),
                PowerLimit.Create(request.MaximumPower, request.MaximumCurrent),
                EffectiveDate.Create(request.EffectiveDate),
                EnumerationType.FromName<NetSettlementGroup>(request.NetSettlementGroup!),
                EnumerationType.FromName<DisconnectionType>(request.DisconnectionType),
                EnumerationType.FromName<ConnectionType>(request.ConnectionType!),
                EnumerationType.FromName<ProductType>(request.ProductType));
        }

        private static ExchangeMeteringPoint CreateExchangeMeteringPoint(CreateMeteringPoint request, MeteringPointType meteringPointType)
        {
            return new(
                MeteringPointId.New(),
                GsrnNumber.Create(request.GsrnNumber),
                CreateAddress(request),
                //PhysicalState.New,
                EnumerationType.FromName<MeteringPointSubType>(request.SubTypeOfMeteringPoint),
                meteringPointType,
                new GridAreaId(Guid.NewGuid()),
                !string.IsNullOrEmpty(request.PowerPlant) ? GsrnNumber.Create(request.PowerPlant) : null,
                request.LocationDescription,
                EnumerationType.FromName<MeasurementUnitType>(request.UnitType),
                request.MeterNumber,
                EnumerationType.FromName<ReadingOccurrence>(request.MeterReadingOccurrence),
                PowerLimit.Create(request.MaximumPower, request.MaximumCurrent),
                EffectiveDate.Create(request.EffectiveDate), // TODO: Parse date in correct format when implemented in Input Validation
                request.ToGrid,
                request.FromGrid,
                EnumerationType.FromName<ProductType>(request.ProductType));
        }

        private static ConsumptionMeteringPoint CreateConsumptionMeteringPoint(CreateMeteringPoint request)
        {
            var meteringPointDetails = CreateDetails(request);
            return ConsumptionMeteringPoint.Create(meteringPointDetails);
        }

        private static MeteringPointDetails CreateDetails(CreateMeteringPoint request)
        {
            return new MeteringPointDetails(
                MeteringPointId.New(),
                GsrnNumber.Create(request.GsrnNumber),
                CreateAddress(request),
                request.IsOfficialAddress.GetValueOrDefault(),
                EnumerationType.FromName<MeteringPointSubType>(request.SubTypeOfMeteringPoint),
                GridAreaId.New(),
                !string.IsNullOrEmpty(request.PowerPlant) ? GsrnNumber.Create(request.PowerPlant) : null !,
                request.LocationDescription,
                request.MeterNumber,
                EnumerationType.FromName<ReadingOccurrence>(request.MeterReadingOccurrence),
                PowerLimit.Create(request.MaximumPower, request.MaximumCurrent),
                EffectiveDate.Create(request.EffectiveDate),
                EnumerationType.FromName<SettlementMethod>(request.SettlementMethod!),
                EnumerationType.FromName<NetSettlementGroup>(request.NetSettlementGroup!),
                EnumerationType.FromName<DisconnectionType>(request.DisconnectionType),
                EnumerationType.FromName<ConnectionType>(request.ConnectionType!),
                !string.IsNullOrEmpty(request.AssetType) ? EnumerationType.FromName<AssetType>(request.AssetType) : null !,
                !string.IsNullOrEmpty(request.ScheduledMeterReadingDate) ? ScheduledMeterReadingDate.Create(request.ScheduledMeterReadingDate !) : null);
        }

        private static Domain.Addresses.Address CreateAddress(CreateMeteringPoint request)
        {
            return Domain.Addresses.Address.Create(
                streetName: request.StreetName,
                streetCode: request.StreetCode,
                buildingNumber: request.BuildingNumber,
                postCode: request.PostCode,
                city: request.CityName,
                citySubDivision: request.CitySubDivisionName,
                countryCode: EnumerationType.FromName<CountryCode>(request.CountryCode),
                floor: request.FloorIdentification,
                room: request.RoomIdentification,
                municipalityCode: string.IsNullOrWhiteSpace(request.MunicipalityCode) ? default : int.Parse(request.MunicipalityCode, NumberStyles.Integer, new NumberFormatInfo()),
                isOfficial: request.IsOfficialAddress.GetValueOrDefault());
        }

        private static BusinessRulesValidationResult ValidateAddress(CreateMeteringPoint request)
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

        private async Task<BusinessProcessResult> ValidateAsync(CreateMeteringPoint request, CancellationToken cancellationToken)
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
