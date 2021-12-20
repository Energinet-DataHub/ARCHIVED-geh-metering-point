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
using Dapper;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public class AssertMeteringPoint
    {
        private readonly dynamic _meteringPoint;

        private AssertMeteringPoint(dynamic meteringPoint)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));
            Assert.NotNull(meteringPoint);
            _meteringPoint = meteringPoint;
        }

        public static AssertMeteringPoint Initialize(string gsrnNumber, IDbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            var meteringPoint = connectionFactory.GetOpenConnection().QuerySingle(
                $"SELECT Id, RecordId, GsrnNumber, StreetName, PostCode, CityName, CountryCode," +
                $"MeteringPointSubType, MeterReadingOccurrence, TypeOfMeteringPoint, MaximumCurrent, MaximumPower," +
                $"MeteringGridArea, PowerPlant, LocationDescription, ProductType, ParentRelatedMeteringPoint," +
                $"UnitType, EffectiveDate, MeterNumber, ConnectionState_PhysicalState, ConnectionState_EffectiveDate," +
                $"StreetCode, CitySubDivision, Floor, Room, BuildingNumber, MunicipalityCode, IsActualAddress, GeoInfoReference," +
                $"Capacity, AssetType, SettlementMethod, ScheduledMeterReadingDate, ProductionObligation," +
                $"NetSettlementGroup, DisconnectionType, ConnectionType, StartOfSupplyDate, ToGrid, FromGrid" +
                $" FROM [dbo].[MeteringPoints]" +
                $" WHERE GsrnNumber = {gsrnNumber}");

            Assert.NotNull(meteringPoint);
            return new AssertMeteringPoint(meteringPoint);
        }

        public AssertMeteringPoint HasConnectionState(PhysicalState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));
            Assert.Equal(state.Name, _meteringPoint.ConnectionState_PhysicalState);
            return this;
        }
    }
}
