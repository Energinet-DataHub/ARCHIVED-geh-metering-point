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

using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules.Connect;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Consumption
{
    [UnitTest]
    public class ConnectTests
    {
        private readonly SystemDateTimeProviderStub _systemDateTimeProvider;

        public ConnectTests()
        {
            _systemDateTimeProvider = new SystemDateTimeProviderStub();
        }

        [Fact]
        public void Not_possible_when_no_energy_supplier_has_been_registered()
        {
            var meteringPoint = CreateConsumptionMeteringPoint();
            var connectionDetails = ConnectNow();

            var checkResult = meteringPoint.ConnectAcceptable(connectionDetails);

            Assert.Contains(checkResult.Errors, error => error is MustHaveEnergySupplierRuleError);
        }

        [Fact]
        public void Not_possible_when_effective_date_is_ahead_of_start_of_supply()
        {
            var meteringPoint = CreateConsumptionMeteringPoint();
            var connectionDetails = ConnectNow();
            SetStartOfSupplyAheadOfEffectiveDate(meteringPoint, connectionDetails.EffectiveDate);

            var checkResult = meteringPoint.ConnectAcceptable(connectionDetails);

            Assert.Contains(checkResult.Errors, error => error is MustHaveEnergySupplierRuleError);
        }

        [Fact]
        public void Should_succeed()
        {
            var meteringPoint = CreateConsumptionMeteringPoint();
            var connectionDetails = ConnectNow();
            SetStartOfSupplyPriorToEffectiveDate(meteringPoint, connectionDetails.EffectiveDate);

            meteringPoint.Connect(connectionDetails);

            Assert.Contains(meteringPoint.DomainEvents, evt => evt is MeteringPointConnected);
        }

        [Fact]
        public void Should_throw_when_business_rules_are_violated()
        {
            var meteringPoint = CreateConsumptionMeteringPoint();
            var connectionDetails = ConnectNow();

            Assert.Throws<MeteringPointConnectException>(() => meteringPoint.Connect(connectionDetails));
        }

        private static void SetStartOfSupplyAheadOfEffectiveDate(ConsumptionMeteringPoint meteringPoint, Instant effectiveDate)
        {
            var startOfSupply = effectiveDate.Plus(Duration.FromDays(1));
            var energySupplierDetails = EnergySupplierDetails.Create(startOfSupply);
            meteringPoint.SetEnergySupplierDetails(energySupplierDetails);
        }

        private static void SetStartOfSupplyPriorToEffectiveDate(ConsumptionMeteringPoint meteringPoint, Instant effectiveDate)
        {
            var startOfSupply = effectiveDate.Minus(Duration.FromDays(1));
            var energySupplierDetails = EnergySupplierDetails.Create(startOfSupply);
            meteringPoint.SetEnergySupplierDetails(energySupplierDetails);
        }

        private static ConsumptionMeteringPoint CreateConsumptionMeteringPoint()
        {
            var address = Address.Create(
                SampleData.StreetName,
                SampleData.StreetCode,
                SampleData.BuildingNumber,
                SampleData.CityName,
                SampleData.CitySubdivision,
                SampleData.PostCode,
                EnumerationType.FromName<CountryCode>(SampleData.CountryCode),
                SampleData.Floor,
                SampleData.Room,
                SampleData.MunicipalityCode);
            var meteringPointDetails = new MeteringPointDetails(
                MeteringPointId.New(),
                GsrnNumber.Create(SampleData.GsrnNumber),
                Address.Create(SampleData.StreetName, SampleData.StreetCode, SampleData.BuildingNumber, SampleData.CityName, SampleData.CitySubdivision, SampleData.PostCode, CountryCode.DK, SampleData.Floor, SampleData.Room, SampleData.MunicipalityCode),
                SampleData.IsOfficialAddress,
                EnumerationType.FromName<MeteringPointSubType>(SampleData.SubTypeName),
                GridAreaId.New(),
                GsrnNumber.Create(SampleData.PowerPlant),
                SampleData.LocationDescription,
                SampleData.MeterNumber,
                ReadingOccurrence.Hourly,
                PowerLimit.Create(SampleData.MaximumPower, SampleData.MaximumCurrent),
                EffectiveDate.Create(SampleData.EffectiveDate),
                EnumerationType.FromName<SettlementMethod>(SampleData.SettlementMethod!),
                EnumerationType.FromName<NetSettlementGroup>(SampleData.NetSettlementGroup!),
                DisconnectionType.Manual,
                ConnectionType.Installation,
                AssetType.Boiler,
                ScheduledMeterReadingDate.Create(SampleData.ScheduledMeterReadingDate));

            return ConsumptionMeteringPoint.Create(meteringPointDetails);
        }

        private ConnectionDetails ConnectNow()
        {
            var effectiveDate = _systemDateTimeProvider.Now();
            return ConnectionDetails.Create(effectiveDate);
        }
    }
}
