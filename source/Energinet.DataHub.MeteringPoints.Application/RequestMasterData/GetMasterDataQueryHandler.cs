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

public class GetMasterDataQueryHandler : IQueryHandler<GetMasterDataQuery, MasterData>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetMasterDataQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<MasterData> Handle(GetMasterDataQuery request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        var selectStatement = $"SELECT " +
                              $"[GsrnNumber] AS {nameof(DataModel.GsrnNumber)}, " +
                              $"[StreetName] AS {nameof(DataModel.StreetName)}, " +
                              $"[StreetCode] AS {nameof(DataModel.StreetCode)}, " +
                              $"[PostCode] AS {nameof(DataModel.PostCode)}, " +
                              $"[CityName] AS {nameof(DataModel.City)}, " +
                              $"[CountryCode] AS {nameof(DataModel.CountryCode)}, " +
                              $"[CitySubDivision] AS {nameof(DataModel.CitySubDivision)}, " +
                              $"[Floor] AS {nameof(DataModel.Floor)}, " +
                              $"[Room] AS {nameof(DataModel.Room)}, " +
                              $"[BuildingNumber] AS {nameof(DataModel.BuildingNumber)}, " +
                              $"[MunicipalityCode] AS {nameof(DataModel.MunicipalityCode)}, " +
                              $"[IsActualAddress] AS {nameof(DataModel.IsActualAddress)}, " +
                              $"[GeoInfoReference] AS {nameof(DataModel.GeoInfoReference)}, " +
                              $"[ConnectionState_PhysicalState] AS {nameof(DataModel.ConnectionState)}, " +
                              $"[MeteringPointSubType] AS {nameof(DataModel.MeteringMethod)}, " +
                              $"[MeterReadingOccurrence] AS {nameof(DataModel.ReadingPeriodicity)}, " +
                              $"[TypeOfMeteringPoint] AS {nameof(DataModel.Type)}, " +
                              $"[MaximumCurrent] AS {nameof(DataModel.MaximumCurrent)}, " +
                              $"[MaximumPower] AS {nameof(DataModel.MaximumPower)}, " +
                              $"[MeteringGridArea] AS {nameof(DataModel.GridAreaCode)}, " +
                              $"[PowerPlant] AS {nameof(DataModel.PowerPlantGsrnNumber)}, " +
                              $"[LocationDescription] AS {nameof(DataModel.LocationDescription)}, " +
                              $"[ProductType] AS {nameof(DataModel.Product)}, " +
                              $"[ParentRelatedMeteringPoint] AS {nameof(DataModel.ParentMeteringPointId)}, " +
                              $"[UnitType] AS {nameof(DataModel.UnitType)}, " +
                              $"[EffectiveDate] AS {nameof(DataModel.EffectiveDate)}, " +
                              $"[MeterNumber] AS {nameof(DataModel.MeterNumber)}, " +
                              $"[Capacity] AS {nameof(DataModel.Capacity)}, " +
                              $"[AssetType] AS {nameof(DataModel.AssetType)}, " +
                              $"[SettlementMethod] AS {nameof(DataModel.SettlementMethod)}, " +
                              $"[ScheduledMeterReadingDate] AS {nameof(DataModel.ScheduledMeterReadingDate)}, " +
                              $"[ProductionObligation] AS {nameof(DataModel.ProductionObligation)}, " +
                              $"[NetSettlementGroup] AS {nameof(DataModel.NetSettlementGroup)}, " +
                              $"[DisconnectionType] AS {nameof(DataModel.DisconnectionType)}, " +
                              $"[ConnectionType] AS {nameof(DataModel.ConnectionType)}, " +
                              $"[ToGrid] AS {nameof(DataModel.ToGridAreaCode)}, " +
                              $"[FromGrid] AS {nameof(DataModel.FromGridAreaCode)} " +
                              $"FROM [dbo].[MeteringPoints] " +
                              $"WHERE GsrnNumber = @GsrnNumber";

        var dataModel = await _connectionFactory.GetOpenConnection()
            .QuerySingleOrDefaultAsync<DataModel>(
                selectStatement,
                new
                {
                    GsrnNumber = request.GsrnNumber,
                }).ConfigureAwait(false);

        return MapFrom(dataModel);
    }

    private static MasterData MapFrom(DataModel dataModel)
    {
        var address = new Address(
            StreetName: dataModel.StreetName,
            StreetCode: dataModel.StreetCode,
            PostCode: dataModel.PostCode,
            City: dataModel.City,
            CountryCode: dataModel.CountryCode,
            CitySubDivision: dataModel.CitySubDivision,
            Floor: dataModel.Floor,
            Room: dataModel.Room,
            BuildingNumber: dataModel.BuildingNumber,
            MunicipalityCode: dataModel.MunicipalityCode,
            IsActualAddress: dataModel.IsActualAddress,
            GeoInfoReference: dataModel.GeoInfoReference,
            LocationDescription: dataModel.LocationDescription);

        return new MasterData(
            GsrnNumber: dataModel.GsrnNumber,
            Address: address,
            ConnectionState: dataModel.ConnectionState,
            MeteringMethod: dataModel.MeteringMethod,
            ReadingPeriodicity: dataModel.ReadingPeriodicity,
            Type: dataModel.Type,
            MaximumCurrent: dataModel.MaximumCurrent,
            MaximumPower: dataModel.MaximumPower,
            GridAreaCode: dataModel.GridAreaCode,
            PowerPlantGsrnNumber: dataModel.PowerPlantGsrnNumber,
            Product: dataModel.Product,
            ParentMeteringPointId: dataModel.ParentMeteringPointId,
            UnitType: dataModel.UnitType,
            EffectiveDate: dataModel.EffectiveDate,
            MeterNumber: dataModel.MeterNumber,
            Capacity: dataModel.Capacity,
            AssetType: dataModel.AssetType,
            SettlementMethod: dataModel.SettlementMethod,
            ScheduledMeterReadingDate: dataModel.ScheduledMeterReadingDate,
            ProductionObligation: dataModel.ProductionObligation,
            NetSettlementGroup: dataModel.NetSettlementGroup,
            DisconnectionType: dataModel.DisconnectionType,
            ConnectionType: dataModel.ConnectionType,
            ToGridAreaCode: dataModel.ToGridAreaCode,
            FromGridAreaCode: dataModel.FromGridAreaCode);
    }
}

public record DataModel(
    string GsrnNumber,
    string StreetName,
    string StreetCode,
    string PostCode,
    string City,
    string CountryCode,
    string CitySubDivision,
    string Floor,
    string Room,
    string BuildingNumber,
    int MunicipalityCode,
    bool IsActualAddress,
    Guid GeoInfoReference,
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
    double Capacity,
    string AssetType,
    string SettlementMethod,
    string ScheduledMeterReadingDate,
    bool ProductionObligation,
    string NetSettlementGroup,
    string DisconnectionType,
    string ConnectionType,
    Guid ToGridAreaCode,
    Guid FromGridAreaCode);

public record MasterData(
    string GsrnNumber,
    Address Address,
    string ConnectionState,
    string MeteringMethod,
    string ReadingPeriodicity,
    string Type,
    int MaximumCurrent,
    int MaximumPower,
    Guid GridAreaCode,
    string PowerPlantGsrnNumber,
    string Product,
    Guid ParentMeteringPointId,
    string UnitType,
    DateTime EffectiveDate,
    string MeterNumber,
    double Capacity,
    string AssetType,
    string SettlementMethod,
    string ScheduledMeterReadingDate,
    bool ProductionObligation,
    string NetSettlementGroup,
    string DisconnectionType,
    string ConnectionType,
    Guid ToGridAreaCode,
    Guid FromGridAreaCode);

public record Address(
    string StreetName,
    string StreetCode,
    string PostCode,
    string City,
    string CountryCode,
    string CitySubDivision,
    string Floor,
    string Room,
    string BuildingNumber,
    int MunicipalityCode,
    bool IsActualAddress,
    Guid GeoInfoReference,
    string LocationDescription);
