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
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.MeteringPoints.Application.Providers.MeteringPointOwnership;
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
            if (request == null) throw new ArgumentNullException(nameof(request));

            var sql = $@"{MeteringPointDtoQueryHelper.Sql} mp
                        INNER JOIN GridAreaLinks gl ON mp.MeteringGridArea = gl.GridAreaId
                        INNER JOIN GridAreas ga ON gl.GridAreaId = ga.Id
                        INNER JOIN UserActor ua ON ga.ActorId = ua.ActorId
                        WHERE mp.GsrnNumber = @GsrnNumber AND ua.UserId = @UserId";

            var meteringPointDto = await _connectionFactory
                .GetOpenConnection()
                .QuerySingleOrDefaultAsync<MeteringPointDto>(sql, new { request.GsrnNumber })
                .ConfigureAwait(false);

            if (meteringPointDto == null)
                return null;

            MeteringPointSimpleCimDto? parentMeteringPoint = null;
            var childMeteringPoints = new List<MeteringPointSimpleCimDto>();

            if (meteringPointDto.ParentMeteringPointId.HasValue)
            {
                parentMeteringPoint = await GetParentMeteringPointAsync(meteringPointDto.ParentMeteringPointId.Value).ConfigureAwait(false);
            }
            else
            {
                childMeteringPoints = await GetChildMeteringPointsAsync(meteringPointDto.MeteringPointId).ConfigureAwait(false);
            }

            return MapToCimDto(meteringPointDto, parentMeteringPoint, childMeteringPoints);
        }

        private static MeteringPointCimDto MapToCimDto(MeteringPointDto meteringPoint, MeteringPointSimpleCimDto? parentMeteringPoint, IEnumerable<MeteringPointSimpleCimDto> childMeteringPoints)
        {
            var meteringPointSubType = ConvertEnumerationTypeToEnum<MeteringMethod, Domain.MasterDataHandling.Components.MeteringDetails.MeteringMethod>(meteringPoint.MeteringPointSubType);
            var connectionState = ConvertEnumerationTypeToEnum<ConnectionState, PhysicalState>(meteringPoint.PhysicalState);
            var meteringPointType = ConvertEnumerationTypeToEnum<MeteringPointType, Domain.MeteringPoints.MeteringPointType>(meteringPoint.MeteringPointType);
            var unitType = ConvertEnumerationTypeToEnum<Unit, MeasurementUnitType>(meteringPoint.UnitType);
            var assetType = ConvertNullableEnumerationTypeToEnum<AssetType, Domain.MasterDataHandling.Components.AssetType>(meteringPoint.AssetType);
            var settlementMethod = ConvertNullableEnumerationTypeToEnum<SettlementMethod, Domain.MasterDataHandling.Components.SettlementMethod>(meteringPoint.SettlementMethod);
            var netSettlementGroup = ConvertNullableEnumerationTypeToEnum<NetSettlementGroup, Domain.MasterDataHandling.Components.NetSettlementGroup>(meteringPoint.NetSettlementGroup);
            var connectionType = ConvertNullableEnumerationTypeToEnum<ConnectionType, Domain.MasterDataHandling.Components.ConnectionType>(meteringPoint.ConnectionType);
            var disconnectionType = ConvertNullableEnumerationTypeToEnum<DisconnectionType, Domain.MasterDataHandling.Components.DisconnectionType>(meteringPoint.DisconnectionType);
            var readingOccurrence = ConvertEnumerationTypeToEnum<ReadingOccurrence, Domain.MasterDataHandling.Components.ReadingOccurrence>(meteringPoint.ReadingOccurrence);
            var productId = ConvertEnumerationTypeToEnum<ProductId, Domain.MasterDataHandling.Components.ProductType>(meteringPoint.Product);

            return new MeteringPointCimDto(
                meteringPoint.MeteringPointId,
                meteringPoint.GsrnNumber,
                meteringPoint.StreetName,
                meteringPoint.PostCode,
                meteringPoint.CityName,
                meteringPoint.CountryCode,
                connectionState,
                meteringPointSubType,
                readingOccurrence,
                meteringPointType,
                meteringPoint.MaximumCurrent,
                meteringPoint.MaximumPower,
                meteringPoint.GridAreaName,
                meteringPoint.GridAreaCode,
                meteringPoint.PowerPlantGsrnNumber,
                meteringPoint.LocationDescription,
                productId,
                unitType,
                meteringPoint.EffectiveDate.ToDateTimeUtc(),
                meteringPoint.MeterNumber,
                meteringPoint.StreetCode,
                meteringPoint.CitySubDivisionName,
                meteringPoint.Floor,
                meteringPoint.Suite,
                meteringPoint.BuildingNumber,
                meteringPoint.MunicipalityCode,
                meteringPoint.IsActualAddress,
                meteringPoint.GeoInfoReference,
                meteringPoint.Capacity,
                assetType,
                settlementMethod,
                meteringPoint.FromGridAreaCode,
                meteringPoint.ToGridAreaCode,
                netSettlementGroup,
                meteringPoint.SupplyStart?.ToDateTimeUtc(),
                connectionType,
                disconnectionType,
                meteringPoint.ProductionObligation,
                childMeteringPoints,
                parentMeteringPoint,
                meteringPoint.PowerPlantGsrnNumber);
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

        private static List<MeteringPointSimpleCimDto> MapToSimpleCimDtoList(IEnumerable<MeteringPointSimpleDto> result)
        {
            return result.Select(MapToSimpleCimDto).ToList();
        }

        private static MeteringPointSimpleCimDto MapToSimpleCimDto(MeteringPointSimpleDto dto)
        {
            var connectionState = ConvertEnumerationTypeToEnum<ConnectionState, PhysicalState>(dto.PhysicalState);
            var meteringPointType = ConvertEnumerationTypeToEnum<MeteringPointType, Domain.MeteringPoints.MeteringPointType>(dto.MeteringPointType);

            return new MeteringPointSimpleCimDto(dto.MeteringPointId, dto.GsrnNumber, connectionState, meteringPointType, dto.EffectiveDate.ToDateTimeUtc());
        }

        private async Task<List<MeteringPointSimpleCimDto>> GetChildMeteringPointsAsync(Guid meteringPointId)
        {
            var sql = $@"{MeteringPointSimpleDtoQueryHelper.Sql}
                            WHERE ParentRelatedMeteringPoint = @MeteringPointId";

            var result = await _connectionFactory
                .GetOpenConnection()
                .QueryAsync<MeteringPointSimpleDto>(sql, new { MeteringPointId = meteringPointId })
                .ConfigureAwait(false);

            return result != null ? MapToSimpleCimDtoList(result) : new List<MeteringPointSimpleCimDto>();
        }

        private async Task<MeteringPointSimpleCimDto?> GetParentMeteringPointAsync(Guid meteringPointId)
        {
            var sql = $@"{MeteringPointSimpleDtoQueryHelper.Sql}
                            WHERE Id = @MeteringPointId";

            var result = await _connectionFactory
                .GetOpenConnection()
                .QuerySingleOrDefaultAsync<MeteringPointSimpleDto>(sql, new { MeteringPointId = meteringPointId })
                .ConfigureAwait(false);

            return result != null ? MapToSimpleCimDto(result) : null;
        }
    }
}
