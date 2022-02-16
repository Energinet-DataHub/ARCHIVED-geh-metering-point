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

using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class ProductionBuilderTests : TestBase
    {
        [Fact]
        public void Production_obligation_is_optional()
        {
            var masterData = Builder()
                .WithProductionObligation(true)
                .Build();

            Assert.True(masterData.ProductionObligation);
        }

        [Fact]
        public void Settlement_method_is_not_allowed()
        {
            var sut = Builder()
                .WithSettlementMethod(SettlementMethod.Flex.Name)
                .Build();

            Assert.Null(sut.SettlementMethod);
        }

        [Fact]
        public void Scheduled_metering_reading_date_is_not_allowed()
        {
            var sut = Builder()
                .WithScheduledMeterReadingDate("0101")
                .Build();

            Assert.Null(sut.ScheduledMeterReadingDate);
        }

        [Fact]
        public void Meter_reading_is_required()
        {
            var sut = Builder()
                .WithReadingPeriodicity(string.Empty)
                .Validate();

            Assert.Contains(sut.Errors, e => e is MeterReadingPeriodicityIsRequired);
        }

        [Fact]
        public void Unit_type_is_required()
        {
            var sut = Builder()
                .WithMeasurementUnitType(null)
                .Validate();

            AssertContainsValidationError<UnitTypeIsRequired>("E73", sut);
        }

        [Fact]
        public void Net_settlement_group_is_required()
        {
            var sut = Builder()
                .Validate();

            AssertContainsValidationError<NetSettlementGroupIsRequired>(sut);
        }

        private static IMasterDataBuilder Builder() =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.Production))
                .WithReadingPeriodicity(ReadingOccurrence.Quarterly.Name);
    }
}
