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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Execeptions;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.MarketMeteringPoints;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    [UnitTest]
    public class SetEnergySupplierTests : TestBase
    {
        private readonly SystemDateTimeProviderStub _systemDateTimeProvider;

        public SetEnergySupplierTests()
        {
            _systemDateTimeProvider = new SystemDateTimeProviderStub();
        }

        [Fact]
        public void Energy_supplier_cannot_be_assigned_to_non_accounting_point()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Exchange);

            Assert.Throws<CannotAssignEnergySupplierExeception>(() => meteringPoint.SetEnergySupplierDetails(EnergySupplierDetails.Create(_systemDateTimeProvider.Now())));
        }

        [Fact]
        public void Should_set_energy_supplier_details()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption);
            meteringPoint.SetEnergySupplierDetails(EnergySupplierDetails.Create(_systemDateTimeProvider.Now()));

            Assert.Contains(meteringPoint.DomainEvents, e => e is EnergySupplierDetailsChanged);
        }

        [Fact]
        public void Does_not_change_energy_supplier_details_when_details_are_the_same()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption);
            meteringPoint.SetEnergySupplierDetails(EnergySupplierDetails.Create(_systemDateTimeProvider.Now()));

            Assert.Equal(1, meteringPoint.DomainEvents.Count(e => e is EnergySupplierDetailsChanged));
        }

        private static MeteringPoint CreateMeteringPoint(MeteringPointType type)
        {
            return MeteringPoint.Create(
                MeteringPointId.New(),
                GsrnNumber.Create(SampleData.GsrnNumber),
                type,
                new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId)),
                EffectiveDate.Create(SampleData.EffectiveDate),
                MasterDataBuilder(type).Build());
        }
    }
}
