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

using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules.Disconnect;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    public class DisconnectTests : TestBase
    {
        private readonly SystemDateTimeProviderStub _systemDateTimeProvider;

        public DisconnectTests()
        {
            _systemDateTimeProvider = new SystemDateTimeProviderStub();
        }

        [Fact]
        public void Metering_point_must_be_connected()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption);
            AssertError<PhysicalStateMustBeConnectedorDisconnectedError>(meteringPoint.DisconnectAcceptable(ConnectNow()));
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption))]
        [InlineData(nameof(MeteringPointType.Production))]
        public void Not_possible_when_no_energy_supplier_E17_E18(string meteringPointType)
        {
            var meteringPoint = CreateMeteringPoint(EnumerationType.FromName<MeteringPointType>(meteringPointType));
            var connectionDetails = ConnectNow();

            var checkResult = meteringPoint.DisconnectAcceptable(connectionDetails);

            Assert.Contains(checkResult.Errors, error => error is MustHaveEnergySupplierRuleError);
        }

        [Fact]
        public void Possible_when_no_energy_supplier_none_accounting_points()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.SurplusProductionGroup);
            var connectionDetails = ConnectNow();

            var checkResult = meteringPoint.DisconnectAcceptable(connectionDetails);

            Assert.DoesNotContain(checkResult.Errors, error => error is MustHaveEnergySupplierRuleError);
        }

        [Fact]
        public void Should_succeed()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption);
            var connectionDetails = ConnectNow();
            SetStartOfSupplyPriorToEffectiveDate(meteringPoint, connectionDetails.EffectiveDate);

            meteringPoint.Connect(connectionDetails);
            meteringPoint.Disconnect(connectionDetails);

            Assert.Equal(meteringPoint.ConnectionState.PhysicalState, PhysicalState.Disconnected);
            Assert.Contains(meteringPoint.DomainEvents, evt => evt is MeteringPointDisconnected);
        }

        private static MeteringPoint CreateMeteringPoint(MeteringPointType meteringPointType)
        {
            return TestBase.CreateMeteringPoint(meteringPointType);
        }

        private static void SetStartOfSupplyPriorToEffectiveDate(MeteringPoint meteringPoint, Instant effectiveDate)
        {
            var startOfSupply = effectiveDate.Minus(Duration.FromDays(1));
            var energySupplierDetails = EnergySupplierDetails.Create(startOfSupply);
            meteringPoint.SetEnergySupplierDetails(energySupplierDetails);
        }

        private ConnectionDetails ConnectNow()
        {
            var effectiveDate = _systemDateTimeProvider.Now();
            return ConnectionDetails.Create(effectiveDate);
        }
    }
}
