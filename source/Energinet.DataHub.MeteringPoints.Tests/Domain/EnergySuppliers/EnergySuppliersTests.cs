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
using Energinet.DataHub.MeteringPoints.Domain.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using FluentAssertions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.EnergySuppliers
{
    [UnitTest]
    public class EnergySuppliersTests
    {
        private static readonly MeteringPointId _meteringPointId = MeteringPointId.New();

        [Fact]
        public void Add_to_empty_list_should_be_OK()
        {
            var sut = DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.EnergySuppliers.Create(Array.Empty<EnergySupplierDetails>());

            var newEnergySupplier = CreateDetails(SampleData.Today, "1");
            sut.Add(newEnergySupplier);

            sut.GetCurrent(SampleData.Today).Should().Be(newEnergySupplier);
        }

        [Fact]
        public void Add_with_existing_on_same_date_should_not_be_ignored()
        {
            var sut = DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.EnergySuppliers.Create(Array.Empty<EnergySupplierDetails>());
            sut.Add(CreateDetails(SampleData.Today, "1"));

            var isAbleToAdd = sut.CanAdd(CreateDetails(SampleData.Today, "2"));

            isAbleToAdd.Should().BeFalse();
        }

        [Fact]
        public void Add_between_existing_registrations_should_be_OK()
        {
            var sut = DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.EnergySuppliers.Create(Array.Empty<EnergySupplierDetails>());
            sut.Add(CreateDetails(SampleData.Today, "1"));
            sut.Add(CreateDetails(SampleData.TodayPlusTwo, "3"));
            var existingNumberOfFutureSuppliers = sut.GetFutureEnergySuppliers(SampleData.Today).Count();

            sut.Add(CreateDetails(SampleData.TodayPlusTwo, "2"));

            var newNumberOfFutureSuppliers = sut.GetFutureEnergySuppliers(SampleData.Today).Count();
            newNumberOfFutureSuppliers.Should().Be(existingNumberOfFutureSuppliers + 1);
        }

        [Fact]
        public void Add_between_existing_registrations_with_same_gln_should_be_OK()
        {
            var sut = DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.EnergySuppliers.Create(Array.Empty<EnergySupplierDetails>());
            sut.Add(CreateDetails(SampleData.Today, "1"));
            sut.Add(CreateDetails(SampleData.TodayPlusTwo, "1"));
            var existingNumberOfFutureSuppliers = sut.GetFutureEnergySuppliers(SampleData.Today).Count();

            sut.Add(CreateDetails(SampleData.TodayPlusTwo, "1"));

            var newNumberOfFutureSuppliers = sut.GetFutureEnergySuppliers(SampleData.Today).Count();
            newNumberOfFutureSuppliers.Should().Be(existingNumberOfFutureSuppliers + 1);
        }

        private static EnergySupplierDetails CreateDetails(Instant when, string gln)
        {
            return EnergySupplierDetails.Create(_meteringPointId, when, GlnNumber.Create(gln));
        }
    }
}
