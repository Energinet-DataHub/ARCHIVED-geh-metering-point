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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints.Queries
{
    public static class MeteringPointDtoQueryHelper
    {
        public const string Sql = @"SELECT mp.[Id] AS MeteringPointId
                                ,mp.[GsrnNumber]
                                ,mp.[StreetName]
                                ,mp.[PostCode]
                                ,mp.[CityName]
                                ,mp.[CountryCode]
                                ,mp.[ConnectionState_PhysicalState] AS PhysicalState
                                ,mp.[MeteringPointSubType]
                                ,mp.[MeterReadingOccurrence] AS ReadingOccurrence
                                ,mp.[TypeOfMeteringPoint] AS MeteringPointType
                                ,mp.[MaximumCurrent]
                                ,mp.[MaximumPower]
					            ,(SELECT TOP(1) G.[Name] FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = [MeteringGridArea]) AS GridAreaName
                                ,(SELECT TOP(1) G.[Code] FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = [MeteringGridArea]) AS GridAreaCode
                                ,mp.[PowerPlant] AS PowerPlantGsrnNumber
                                ,mp.[LocationDescription]
                                ,mp.[ProductType] AS Product
                                ,mp.[UnitType]
                                ,mp.[EffectiveDate]
                                ,mp.[MeterNumber]
                                ,mp.[StreetCode]
                                ,mp.[CitySubDivision] AS CitySubDivisionName
                                ,mp.[Floor]
                                ,mp.[Room] AS Suite
                                ,mp.[BuildingNumber]
                                ,mp.[MunicipalityCode]
                                ,mp.[IsActualAddress]
                                ,mp.[GeoInfoReference]
                                ,mp.[Capacity]
                                ,mp.[AssetType]
                                ,mp.[SettlementMethod]
                                ,(SELECT TOP(1) G.Code FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = [ToGrid]) AS ToGridAreaCode
                                ,(SELECT TOP(1) G.Code FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = [FromGrid]) AS FromGridAreaCode
                                ,mp.[NetSettlementGroup]
	                            ,mp.[StartOfSupplyDate] AS SupplyStart
                                ,mp.[ConnectionType]
                                ,mp.[DisconnectionType]
                                ,mp.[ProductionObligation]
                                ,mp.[ParentRelatedMeteringPoint] AS ParentMeteringPointId
                          FROM  [dbo].[MeteringPoints] mp";
    }
}
