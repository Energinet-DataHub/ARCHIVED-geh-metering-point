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
using Dapper;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;
using AssetType = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.AssetType;
using ConnectionState = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.ConnectionState;
using ConnectionType = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.ConnectionType;
using DisconnectionType = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.DisconnectionType;
using MeteringMethod = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.MeteringMethod;
using MeteringPointType = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.MeteringPointType;
using NetSettlementGroup = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.NetSettlementGroup;
using ReadingOccurrence = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.ReadingOccurrence;
using SettlementMethod = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.SettlementMethod;
using Unit = Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums.Unit;

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
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string sql = $@"{MeteringPointDtoQueryHelper.Sql}
                            WHERE GsrnNumber = @GsrnNumber";

            var result = await _connectionFactory
                .GetOpenConnection()
                .QuerySingleOrDefaultAsync<MeteringPointDto>(sql, new { request.GsrnNumber })
                .ConfigureAwait(false);

            string parentSql = $@"{MeteringPointDtoQueryHelper.Sql}
                                  WHERE Id = @meteringPointId";

            var parent = await _connectionFactory
                .GetOpenConnection()
                .QuerySingleOrDefaultAsync<MeteringPointDto>(parentSql, new { meteringPointId = result.ParentMeteringPointId })
                .ConfigureAwait(false);

            string childrenSql = $@"{MeteringPointDtoQueryHelper.Sql}
                                    WHERE ParentRelatedMeteringPoint = @meteringPointId";

            var children = await _connectionFactory
                .GetOpenConnection()
                .QueryAsync<MeteringPointDto>(childrenSql, new { result.MeteringPointId })
                .ConfigureAwait(false);

            return result != null
                ? MapToCimDto(result, parent, children)
                : null;
        }

        private static MeteringPointCimDto MapToCimDto(MeteringPointDto meteringPointDto, MeteringPointDto? parent, IEnumerable<MeteringPointDto>? children)
        {
            var meteringPointSubType = ConvertEnumerationTypeToEnum<MeteringMethod, Domain.MasterDataHandling.Components.MeteringDetails.MeteringMethod>(meteringPointDto.MeteringPointSubType);
            var connectionState = ConvertEnumerationTypeToEnum<ConnectionState, PhysicalState>(meteringPointDto.PhysicalState);
            var meteringPointType = ConvertEnumerationTypeToEnum<MeteringPointType, Domain.MeteringPoints.MeteringPointType>(meteringPointDto.MeteringPointType);
            var unitType = ConvertEnumerationTypeToEnum<Unit, MeasurementUnitType>(meteringPointDto.UnitType);
            var assetType = ConvertNullableEnumerationTypeToEnum<AssetType, Domain.MasterDataHandling.Components.AssetType>(meteringPointDto.AssetType);
            var settlementMethod = ConvertNullableEnumerationTypeToEnum<SettlementMethod, Domain.MasterDataHandling.Components.SettlementMethod>(meteringPointDto.SettlementMethod);
            var netSettlementGroup = ConvertNullableEnumerationTypeToEnum<NetSettlementGroup, Domain.MasterDataHandling.Components.NetSettlementGroup>(meteringPointDto.NetSettlementGroup);
            var connectionType = ConvertNullableEnumerationTypeToEnum<ConnectionType, Domain.MasterDataHandling.Components.ConnectionType>(meteringPointDto.ConnectionType);
            var disconnectionType = ConvertNullableEnumerationTypeToEnum<DisconnectionType, Domain.MasterDataHandling.Components.DisconnectionType>(meteringPointDto.DisconnectionType);
            var readingOccurrence = ConvertEnumerationTypeToEnum<ReadingOccurrence, Domain.MasterDataHandling.Components.ReadingOccurrence>(meteringPointDto.ReadingOccurrence);
            var productId = ConvertEnumerationTypeToEnum<ProductId, Domain.MasterDataHandling.Components.ProductType>(meteringPointDto.Product);

            var childrenList = children?.Select(x =>
                    new MeteringPointSimpleCimDto(
                    x.MeteringPointId,
                    x.GsrnNumber,
                    ConvertEnumerationTypeToEnum<ConnectionState, PhysicalState>(x.PhysicalState),
                    ConvertEnumerationTypeToEnum<MeteringPointType, Domain.MeteringPoints.MeteringPointType>(
                        x.MeteringPointType),
                    x.EffectiveDate.ToDateTimeUtc()));

            var parentMeteringPoint = parent != null
                ? new MeteringPointSimpleCimDto(
                    parent.MeteringPointId,
                    parent.GsrnNumber,
                    ConvertEnumerationTypeToEnum<ConnectionState, PhysicalState>(parent.PhysicalState),
                    ConvertEnumerationTypeToEnum<MeteringPointType, Domain.MeteringPoints.MeteringPointType>(
                        parent.MeteringPointType),
                    parent.EffectiveDate.ToDateTimeUtc()) : null;

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
                productId,
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
                meteringPointDto.FromGridAreaCode,
                meteringPointDto.ToGridAreaCode,
                netSettlementGroup,
                meteringPointDto.SupplyStart,
                connectionType,
                disconnectionType,
                meteringPointDto.ProductionObligation,
                childrenList,
                parentMeteringPoint,
                meteringPointDto.PowerPlantGsrnNumber);
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
            if (string.IsNullOrEmpty(enumerationTypeName))
                return null;
            return ConvertEnumerationTypeToEnum<TEnum, TEnumerationType>(enumerationTypeName);
        }
    }
}
