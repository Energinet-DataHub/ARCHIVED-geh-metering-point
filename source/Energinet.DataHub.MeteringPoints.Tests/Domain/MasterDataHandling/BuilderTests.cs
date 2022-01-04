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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class BuilderTests : TestBase
    {
        [Theory]
        [InlineData(nameof(MeteringPointType.Analysis))]
        [InlineData(nameof(MeteringPointType.Exchange))]
        [InlineData(nameof(MeteringPointType.Production))]
        [InlineData(nameof(MeteringPointType.ElectricalHeating))]
        [InlineData(nameof(MeteringPointType.InternalUse))]
        [InlineData(nameof(MeteringPointType.NetConsumption))]
        [InlineData(nameof(MeteringPointType.NetProduction))]
        [InlineData(nameof(MeteringPointType.OtherConsumption))]
        [InlineData(nameof(MeteringPointType.OtherProduction))]
        [InlineData(nameof(MeteringPointType.OwnProduction))]
        [InlineData(nameof(MeteringPointType.TotalConsumption))]
        [InlineData(nameof(MeteringPointType.WholesaleServices))]
        [InlineData(nameof(MeteringPointType.ConsumptionFromGrid))]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy))]
        [InlineData(nameof(MeteringPointType.GridLossCorrection))]
        [InlineData(nameof(MeteringPointType.NetFromGrid))]
        [InlineData(nameof(MeteringPointType.NetToGrid))]
        [InlineData(nameof(MeteringPointType.SupplyToGrid))]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup))]
        [InlineData(nameof(MeteringPointType.VEProduction))]
        public void Settlement_method_is_not_allowed(string meteringPointType)
        {
            var sut = new MasterDataBuilder(
                    new MasterDataFieldSelector().GetMasterDataFieldsFor(EnumerationType.FromName<MeteringPointType>(meteringPointType)))
                .WithSettlementMethod(SettlementMethod.Flex.Name)
                .Build();

            Assert.Null(sut.SettlementMethod);
        }
    }
}
