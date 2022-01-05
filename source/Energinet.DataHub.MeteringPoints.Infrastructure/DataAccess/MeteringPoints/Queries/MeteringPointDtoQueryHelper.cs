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
        public const string Sql = @"SELECT [Id] AS MeteringPointId
                                ,[GsrnNumber]
                                ,[StreetName]
                                ,[PostCode]
                                ,[CityName]
                                ,[CountryCode]
                                ,[ConnectionState_PhysicalState] AS PhysicalState
                                ,[MeteringPointSubType]
                                ,[MeterReadingOccurrence] AS ReadingOccurrence
                                ,[TypeOfMeteringPoint] AS MeteringPointType
                                ,[MaximumCurrent]
                                ,[MaximumPower]
					            ,(SELECT TOP(1) G.[Name] FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = [MeteringGridArea]) AS GridAreaName
                                ,(SELECT TOP(1) G.[Code] FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = [MeteringGridArea]) AS GridAreaCode
                                ,[PowerPlant] AS PowerPlantGsrnNumber
                                ,[LocationDescription]
                                ,[ProductType] AS Product
                                ,[UnitType]
                                ,[EffectiveDate]
                                ,[MeterNumber]
                                ,[StreetCode]
                                ,[CitySubDivision] AS CitySubDivisionName
                                ,[Floor]
                                ,[Room] AS Suite
                                ,[BuildingNumber]
                                ,[MunicipalityCode]
                                ,[IsActualAddress]
                                ,[GeoInfoReference]
                                ,[Capacity]
                                ,[AssetType]
                                ,[SettlementMethod]
                                ,(SELECT TOP(1) G.Code FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = [ToGrid]) AS ToGridAreaCode
                                ,(SELECT TOP(1) G.Code FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = [FromGrid]) AS FromGridAreaCode
                                ,[NetSettlementGroup]
	                            ,[StartOfSupplyDate] AS SupplyStart
                                ,[ConnectionType]
                                ,[DisconnectionType]
                                ,[ProductionObligation]
                                ,[ParentRelatedMeteringPoint] AS ParentMeteringPointId
                          FROM  [dbo].[MeteringPoints]";
    }
}
