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
using Dapper;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;
using AssetType = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.AssetType;
using ConnectionState = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.ConnectionState;
using MeteringMethod = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.MeteringMethod;
using MeteringPointType = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.MeteringPointType;
using ReadingOccurrence = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.ReadingOccurrence;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints.Queries
{
    public class MeteringPointByGsrnQueryHandler : IRequestHandler<MeteringPointByGsrnQuery, MeteringPointCimDto?>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MeteringPointByGsrnQueryHandler(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<MeteringPointCimDto?> Handle(MeteringPointByGsrnQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            string sql = $@"{MeteringPointDtoQueryHelper.Sql}
                            WHERE GsrnNumber = @GsrnNumber";

            var result = await _connectionFactory
                .GetOpenConnection()
                .QuerySingleOrDefaultAsync<MeteringPointDto>(sql, new { request.GsrnNumber })
                .ConfigureAwait(false);

            return result != null
                ? MapToCimDto(result)
                : null;
        }

        private static MeteringPointCimDto MapToCimDto(MeteringPointDto meteringPointDto)
        {
            var meteringPointSubType = ConvertEnumerationTypeToEnum<MeteringMethod, Domain.MeteringDetails.MeteringMethod>(meteringPointDto.MeteringPointSubType);
            var connectionState = ConvertEnumerationTypeToEnum<ConnectionState, PhysicalState>(meteringPointDto.PhysicalState);
            var meteringPointType = ConvertEnumerationTypeToEnum<MeteringPointType, Domain.MeteringPoints.MeteringPointType>(meteringPointDto.MeteringPointType);
            var unitType = ConvertEnumerationTypeToEnum<PriceUnit, MeasurementUnitType>(meteringPointDto.UnitType);
            var assetType = ConvertNullableEnumerationTypeToEnum<AssetType, Domain.MeteringPoints.AssetType>(meteringPointDto.AssetType);
            var settlementMethod = ConvertNullableEnumerationTypeToEnum<SettlementMethod, Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.SettlementMethod>(meteringPointDto.SettlementMethod);
            var netSettlementGroup = ConvertNullableEnumerationTypeToEnum<NetSettlementGroup, Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.NetSettlementGroup>(meteringPointDto.NetSettlementGroup);
            var connectionType = ConvertNullableEnumerationTypeToEnum<ConnectionType, Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.ConnectionType>(meteringPointDto.ConnectionType);
            var disconnectionType = ConvertNullableEnumerationTypeToEnum<DisconnectionType, Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.DisconnectionType>(meteringPointDto.DisconnectionType);
            var readingOccurrence = ConvertEnumerationTypeToEnum<ReadingOccurrence, Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ReadingOccurrence>(meteringPointDto.ReadingOccurrence);

            return new MeteringPointCimDto(
                meteringPointDto.MeteringPointId,
                meteringPointDto.GsrnNumber,
                meteringPointDto.StreetName,
                meteringPointDto.PostCode,
                meteringPointDto.CityName,
                meteringPointDto.CountryCode,
                connectionState,
                meteringPointSubType,
                readingOccurrence,
                meteringPointType,
                meteringPointDto.MaximumCurrent,
                meteringPointDto.MaximumPower,
                meteringPointDto.GridAreaName,
                meteringPointDto.GridAreaCode,
                meteringPointDto.PowerPlantGsrnNumber,
                meteringPointDto.LocationDescription,
                meteringPointDto.Product,
                unitType,
                meteringPointDto.EffectiveDate.ToDateTimeUtc(),
                meteringPointDto.MeterNumber,
                meteringPointDto.StreetCode,
                meteringPointDto.CitySubDivisionName,
                meteringPointDto.Floor,
                meteringPointDto.Suite,
                meteringPointDto.BuildingNumber,
                meteringPointDto.MunicipalityCode,
                meteringPointDto.IsActualAddress,
                meteringPointDto.GeoInfoReference,
                meteringPointDto.Capacity,
                assetType,
                settlementMethod,
                meteringPointDto.ToGridAreaCode,
                meteringPointDto.FromGridAreaCode,
                netSettlementGroup,
                meteringPointDto.SupplyStart,
                connectionType,
                disconnectionType,
                meteringPointDto.ProductionObligation,
                new List<MeteringPointSimpleCimDto>(),
                null);
        }

        private static TEnum ConvertEnumerationTypeToEnum<TEnum, TEnumerationType>(string enumerationTypeName)
            where TEnum : Enum
            where TEnumerationType : EnumerationType
        {
            var meteringMethodValueObject = EnumerationType.FromName<TEnumerationType>(enumerationTypeName);

            return (TEnum)(object)meteringMethodValueObject.Id;
        }

        private static TEnum? ConvertNullableEnumerationTypeToEnum<TEnum, TEnumerationType>(string? enumerationTypeName)
            where TEnum : struct, Enum
            where TEnumerationType : EnumerationType
        {
            if (string.IsNullOrEmpty(enumerationTypeName)) return null;
            return ConvertEnumerationTypeToEnum<TEnum, TEnumerationType>(enumerationTypeName);
        }
    }
}
