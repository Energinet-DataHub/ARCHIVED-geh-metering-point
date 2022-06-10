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
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Queries;

namespace Energinet.DataHub.MeteringPoints.Application.RequestMasterData;

public class GetMasterDataQueryHandler : IQueryHandler<GetMasterDataQuery, MasterDataDto>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetMasterDataQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<MasterDataDto> Handle(GetMasterDataQuery request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        var selectStatement = $"SELECT " +
                              $"[GsrnNumber] AS {nameof(MasterDataDto.GsrnNumber)}, " +
                              $"[StreetName] AS {nameof(MasterDataDto.StreetName)}, " +
                              $"[PostCode] AS {nameof(MasterDataDto.PostCode)}, " +
                              $"[CityName] AS {nameof(MasterDataDto.City)}, " +
                              $"[CountryCode] AS {nameof(MasterDataDto.CountryCode)}, " +
                              $"[ConnectionState_PhysicalState] AS {nameof(MasterDataDto.ConnectionState)}, " +
                              $"[MeteringPointSubType] AS {nameof(MasterDataDto.MeteringMethod)}, " +
                              $"[MeterReadingOccurrence] AS {nameof(MasterDataDto.ReadingPeriodicity)}, " +
                              $"[TypeOfMeteringPoint] AS {nameof(MasterDataDto.Type)}, " +
                              $"[MaximumCurrent] AS {nameof(MasterDataDto.MaximumCurrent)}, " +
                              $"[MaximumPower] AS {nameof(MasterDataDto.MaximumPower)}, " +
                              $"[MeteringGridArea] AS {nameof(MasterDataDto.GridAreaCode)}, " +
                              $"[PowerPlant] AS {nameof(MasterDataDto.PowerPlantGsrnNumber)}, " +
                              $"[LocationDescription] AS {nameof(MasterDataDto.LocationDescription)}, " +
                              $"[ProductType] AS {nameof(MasterDataDto.Product)}, " +
                              $"[ParentRelatedMeteringPoint] AS {nameof(MasterDataDto.ParentMeteringPointId)}, " +
                              $"[UnitType] AS {nameof(MasterDataDto.UnitType)}, " +
                              $"[EffectiveDate] AS {nameof(MasterDataDto.EffectiveDate)}, " +
                              $"[MeterNumber] AS {nameof(MasterDataDto.MeterNumber)}, " +
                              $"[StreetCode] AS {nameof(MasterDataDto.StreetCode)}, " +
                              $"[CitySubDivision] AS {nameof(MasterDataDto.CitySubDivision)}, " +
                              $"[Floor] AS {nameof(MasterDataDto.Floor)}, " +
                              $"[Room] AS {nameof(MasterDataDto.Room)}, " +
                              $"[BuildingNumber] AS {nameof(MasterDataDto.BuildingNumber)}, " +
                              $"[MunicipalityCode] AS {nameof(MasterDataDto.MunicipalityCode)}, " +
                              $"[IsActualAddress] AS {nameof(MasterDataDto.IsActualAddress)}, " +
                              $"[GeoInfoReference] AS {nameof(MasterDataDto.GeoInfoReference)}, " +
                              $"[Capacity] AS {nameof(MasterDataDto.Capacity)}, " +
                              $"[AssetType] AS {nameof(MasterDataDto.AssetType)}, " +
                              $"[SettlementMethod] AS {nameof(MasterDataDto.SettlementMethod)}, " +
                              $"[ScheduledMeterReadingDate] AS {nameof(MasterDataDto.ScheduledMeterReadingDate)}, " +
                              $"[ProductionObligation] AS {nameof(MasterDataDto.ProductionObligation)}, " +
                              $"[NetSettlementGroup] AS {nameof(MasterDataDto.NetSettlementGroup)}, " +
                              $"[DisconnectionType] AS {nameof(MasterDataDto.DisconnectionType)}, " +
                              $"[ConnectionType] AS {nameof(MasterDataDto.ConnectionType)}, " +
                              $"[StartOfSupplyDate] AS {nameof(MasterDataDto.SupplyStart)}, " +
                              $"[ToGrid] AS {nameof(MasterDataDto.ToGridAreaCode)}, " +
                              $"[FromGrid] AS {nameof(MasterDataDto.FromGridAreaCode)} " +
                              $"FROM [dbo].[MeteringPoints] " +
                              $"WHERE GsrnNumber = @GsrnNumber";

        var result = await _connectionFactory.GetOpenConnection()
            .QuerySingleOrDefaultAsync<MasterDataDto>(
                selectStatement,
                new
                {
                    GsrnNumber = request.GsrnNumber,
                }).ConfigureAwait(false);

        return result;
    }
}

public record MasterDataDto(
    string GsrnNumber,
    string StreetName,
    string PostCode,
    string City,
    string CountryCode,
    string ConnectionState,
    string MeteringMethod,
    string ReadingPeriodicity,
    string Type,
    int MaximumCurrent,
    int MaximumPower,
    Guid GridAreaCode,
    string PowerPlantGsrnNumber,
    string LocationDescription,
    string Product,
    Guid ParentMeteringPointId,
    string UnitType,
    DateTime EffectiveDate,
    string MeterNumber,
    string StreetCode,
    string CitySubDivision,
    string Floor,
    string Room,
    string BuildingNumber,
    int MunicipalityCode,
    bool IsActualAddress,
    Guid GeoInfoReference,
    double Capacity,
    string AssetType,
    string SettlementMethod,
    string ScheduledMeterReadingDate,
    bool ProductionObligation,
    string NetSettlementGroup,
    string DisconnectionType,
    string ConnectionType,
    DateTime SupplyStart,
    Guid ToGridAreaCode,
    Guid FromGridAreaCode);

public record Address(
    string StreetName,
    string PostCode,
    string City,
    string StreetCode,
    string BuildingNumber,
    string CitySubDivision,
    string CountryCode,
    string Floor,
    string Room,
    int MunicipalityCode,
    bool IsActualAddress,
    string GeoInfoReference,
    string LocationDescription);
