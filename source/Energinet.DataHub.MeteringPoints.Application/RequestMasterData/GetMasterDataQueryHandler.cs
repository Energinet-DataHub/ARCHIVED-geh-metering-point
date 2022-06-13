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
                              $"mp.GsrnNumber AS {nameof(DataModel.GsrnNumber)}, " +
                              $"mp.StreetName AS {nameof(DataModel.StreetName)}, " +
                              $"mp.StreetCode AS {nameof(DataModel.StreetCode)}, " +
                              $"mp.PostCode AS {nameof(DataModel.PostCode)}, " +
                              $"mp.CityName AS {nameof(DataModel.City)}, " +
                              $"mp.CountryCode AS {nameof(DataModel.CountryCode)}, " +
                              $"mp.CitySubDivision AS {nameof(DataModel.CitySubDivision)}, " +
                              $"mp.Floor AS {nameof(DataModel.Floor)}, " +
                              $"mp.Room AS {nameof(DataModel.Room)}, " +
                              $"mp.BuildingNumber AS {nameof(DataModel.BuildingNumber)}, " +
                              $"mp.MunicipalityCode AS {nameof(DataModel.MunicipalityCode)}, " +
                              $"mp.IsActualAddress AS {nameof(DataModel.IsActualAddress)}, " +
                              $"mp.GeoInfoReference AS {nameof(DataModel.GeoInfoReference)}, " +
                              $"mp.ConnectionState_PhysicalState AS {nameof(DataModel.ConnectionState)}, " +
                              $"mp.MeteringPointSubType AS {nameof(DataModel.MeteringMethod)}, " +
                              $"mp.MeterReadingOccurrence AS {nameof(DataModel.ReadingPeriodicity)}, " +
                              $"mp.TypeOfMeteringPoint AS {nameof(DataModel.Type)}, " +
                              $"mp.MaximumCurrent AS {nameof(DataModel.MaximumCurrent)}, " +
                              $"mp.MaximumPower AS {nameof(DataModel.MaximumPower)}, " +
                              $"ga.Code AS {nameof(DataModel.GridAreaCode)}, " +
                              $"mp.PowerPlant AS {nameof(DataModel.PowerPlantGsrnNumber)}, " +
                              $"mp.LocationDescription AS {nameof(DataModel.LocationDescription)}, " +
                              $"mp.ProductType AS {nameof(DataModel.Product)}, " +
                              $"mp.UnitType AS {nameof(DataModel.UnitType)}, " +
                              $"mp.EffectiveDate AS {nameof(DataModel.EffectiveDate)}, " +
                              $"mp.MeterNumber AS {nameof(DataModel.MeterNumber)}, " +
                              $"mp.Capacity AS {nameof(DataModel.Capacity)}, " +
                              $"mp.AssetType AS {nameof(DataModel.AssetType)}, " +
                              $"mp.SettlementMethod AS {nameof(DataModel.SettlementMethod)}, " +
                              $"mp.ScheduledMeterReadingDate AS {nameof(DataModel.ScheduledMeterReadingDate)}, " +
                              $"mp.ProductionObligation AS {nameof(DataModel.ProductionObligation)}, " +
                              $"mp.NetSettlementGroup AS {nameof(DataModel.NetSettlementGroup)}, " +
                              $"mp.DisconnectionType AS {nameof(DataModel.DisconnectionType)}, " +
                              $"mp.ConnectionType AS {nameof(DataModel.ConnectionType)}, " +
                              $"mp.ToGrid AS {nameof(DataModel.ToGridAreaCode)}, " +
                              $"mp.FromGrid AS {nameof(DataModel.FromGridAreaCode)} " +
                              $"FROM [dbo].[MeteringPoints] mp " +
                              $"JOIN [dbo].[GridAreaLinks] gl ON gl.Id = mp.MeteringGridArea " +
                              $"JOIN [dbo].[GridAreas] ga ON ga.Id = gl.GridAreaId " +
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
            Series: new Series(dataModel.Product, dataModel.UnitType),
            GridAreaDetails: new GridAreaDetails(dataModel.GridAreaCode, string.Empty, string.Empty),
            ConnectionState: dataModel.ConnectionState,
            MeteringMethod: dataModel.MeteringMethod,
            ReadingPeriodicity: dataModel.ReadingPeriodicity,
            Type: dataModel.Type,
            MaximumCurrent: dataModel.MaximumCurrent,
            MaximumPower: dataModel.MaximumPower,
            PowerPlantGsrnNumber: dataModel.PowerPlantGsrnNumber,
            EffectiveDate: dataModel.EffectiveDate,
            MeterNumber: dataModel.MeterNumber,
            Capacity: dataModel.Capacity,
            AssetType: dataModel.AssetType,
            SettlementMethod: dataModel.SettlementMethod,
            ScheduledMeterReadingDate: dataModel.ScheduledMeterReadingDate,
            ProductionObligation: dataModel.ProductionObligation,
            NetSettlementGroup: dataModel.NetSettlementGroup,
            DisconnectionType: dataModel.DisconnectionType,
            ConnectionType: dataModel.ConnectionType);
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
    string GridAreaCode,
    string PowerPlantGsrnNumber,
    string LocationDescription,
    string Product,
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
    Series Series,
    GridAreaDetails GridAreaDetails,
    string ConnectionState,
    string MeteringMethod,
    string ReadingPeriodicity,
    string Type,
    int MaximumCurrent,
    int MaximumPower,
    string PowerPlantGsrnNumber,
    DateTime EffectiveDate,
    string MeterNumber,
    double Capacity,
    string AssetType,
    string SettlementMethod,
    string ScheduledMeterReadingDate,
    bool ProductionObligation,
    string NetSettlementGroup,
    string DisconnectionType,
    string ConnectionType);

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

public record Series(string Product, string UnitType);

public record GridAreaDetails(string Code, string ToCode, string FromCode);
