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
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Client.Core;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints.Queries
{
    public class MeteringPointByGsrnQueryHandler : IRequestHandler<MeteringPointByGsrnQuery, MeteringPointDTO?>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MeteringPointByGsrnQueryHandler(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<MeteringPointDTO?> Handle(MeteringPointByGsrnQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var sql = @"SELECT  MP.[Id] AS MeteringPointId
                                ,MP.[GsrnNumber]
                                ,MP.[StreetName]
                                ,MP.[PostCode]
                                ,MP.[CityName]
                                ,MP.[CountryCode]
                                ,MP.[ConnectionState_PhysicalState] AS ConnectionState
                                ,MP.[MeteringPointSubType]
                                ,MP.[MeterReadingOccurrence]
                                ,MP.[TypeOfMeteringPoint] AS MeteringPointType
                                ,MP.[MaximumCurrent]
                                ,MP.[MaximumPower]
					            ,(SELECT TOP(1) G.[Name] FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = MP.[MeteringGridArea]) AS GridAreaName
                                ,(SELECT TOP(1) G.[Code] FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = MP.[MeteringGridArea]) AS GridAreaCode
                                ,MP.[PowerPlant]
                                ,MP.[LocationDescription]
                                ,MP.[ProductType]
                                ,MP.[ParentRelatedMeteringPoint]
                                ,MP.[UnitType]
                                ,MP.[EffectiveDate]
                                ,MP.[MeterNumber]
                                ,MP.[ConnectionState_EffectiveDate] AS EffectiveDate
                                ,MP.[StreetCode]
                                ,MP.[CitySubDivision] AS CitySubDivisionName
                                ,MP.[Floor]
                                ,MP.[Room] AS Suite
                                ,MP.[BuildingNumber]
                                ,MP.[MunicipalityCode]
                                ,MP.[IsActualAddress]
                                ,MP.[GeoInfoReference]
                                ,MP.[Capacity]
	                            ,CMP.[RecordId]
                                ,CMP.[SettlementMethod]
                                ,CMP.[NetSettlementGroup]
                                ,CMP.[AssetType]
                                ,CMP.[ScheduledMeterReadingDate]
	                            ,(SELECT TOP(1) G.Code FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = EMP.[ToGrid]) AS ToGridAreaCode
                                ,(SELECT TOP(1) G.Code FROM [GridAreas] G INNER JOIN [GridAreaLinks] GL ON G.Id = GL.GridAreaId WHERE GL.Id = EMP.[FromGrid]) AS FromGridAreaCode
	                            ,MMP.[StartOfSupplyDate]
                                ,MMP.[ConnectionType]
                                ,MMP.[DisconnectionType]
                                ,PMP.[ProductionObligation]
                                ,PMP.[NetSettlementGroup]
                                ,PMP.[DisconnectionType]
	                            ,PMP.[ConnectionType]
                          FROM  [dbo].[MeteringPoints] MP LEFT OUTER JOIN
	                            [dbo].[ExchangeMeteringPoints] EMP ON EMP.Id = MP.Id LEFT OUTER JOIN
	                            [dbo].[MarketMeteringPoints] MMP ON MMP.Id = MP.Id LEFT OUTER JOIN
	                            [dbo].[ConsumptionMeteringPoints] CMP ON CMP.Id = MP.Id LEFT OUTER JOIN
	                            [dbo].[ProductionMeteringPoints] PMP ON PMP.Id = MP.Id
                         WHERE GsrnNumber = @GsrnNumber";

            var result = await _connectionFactory
                .GetOpenConnection()
                .QuerySingleOrDefaultAsync<MeteringPointDTO>(sql, new { request.GsrnNumber })
                .ConfigureAwait(false);

            return result;
        }
    }
}
