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

using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    #pragma warning disable
    [UnitTest]
    public class MeteringPointTests
    {
        [Fact]
        public void ShouldRaiseEventWhenCreated()
        {
            var meteringPoint = CreateConsumptionMeteringPoint();

            var createdEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is MeteringPointCreated);
            Assert.NotNull(createdEvent);
        }

        [Fact]
        public void ShouldRaiseEventWhenConnected()
        {
            var meteringPoint = CreateConsumptionMeteringPoint();

            meteringPoint.Connect(SystemClock.Instance.GetCurrentInstant());

            var connectedEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is MeteringPointConnected);
            Assert.NotNull(connectedEvent);
        }

        private static MeteringPoint CreateConsumptionMeteringPoint()
        {
            return new ConsumptionMeteringPoint(
                MeteringPointId.New(),
                GsrnNumber.Create(SampleData.GsrnNumber),
                Address.Create(SampleData.StreetName, SampleData.PostCode, SampleData.CityName, SampleData.CountryCode),
                SampleData.IsAddressWashable,
                EnumerationType.FromName<PhysicalState>(SampleData.PhysicalStateName),
                EnumerationType.FromName<MeteringPointSubType>(SampleData.SubTypeName),
                EnumerationType.FromName<MeteringPointType>(SampleData.TypeName),
                GridAreaId.New(),
                GsrnNumber.Create(SampleData.PowerPlant),
                SampleData.LocationDescription,
                MeasurementUnitType.KWh,
                SampleData.MeterNumber,
                ReadingOccurrence.Hourly,
                SampleData.MaximumCurrent,
                SampleData.MaximumPower,
                SampleData.OccurenceDate,
                SettlementMethod.Flex,
                NetSettlementGroup.Zero,
                DisconnectionType.Manual,
                ConnectionType.Direct,
                AssetType.Boiler,
                parentRelatedMeteringPoint: null);

            var createdEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is MeteringPointCreated);
            Assert.NotNull(createdEvent);
        }
    }
}
