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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeMasterData;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class UpdateMasterDataTests
        : TestHost
    {
        private readonly SystemDateTimeProviderStub _timeProvider;

        public UpdateMasterDataTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _timeProvider = (SystemDateTimeProviderStub)GetService<ISystemDateTimeProvider>();
            _timeProvider.SetNow(InstantPattern.General.Parse(SampleData.EffectiveDate).Value);
        }

        [Fact]
        public async Task Meter_is_required_when_changing_method_to_physical()
        {
            await CreateVirtualConsumptionMeteringPoint().ConfigureAwait(false);
            var message = new MasterDataDocument()
                with
                {
                    EffectiveDate = _timeProvider.Now().ToString(),
                    TransactionId = SampleData.Transaction,
                    GsrnNumber = SampleData.GsrnNumber,
                    ProcessType = BusinessProcessType.ChangeMasterData.Name,
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterNumber = null,
                };

            await SendCommandAsync(message).ConfigureAwait(false);

            AssertValidationError("D31");
        }

        private async Task CreatePhysicalConsumptionMeteringPoint()
        {
            var request = Scenarios.CreateConsumptionMeteringPointCommand()
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterNumber = "1",
                    NetSettlementGroup = NetSettlementGroup.Zero.Name,
                    ConnectionType = null,
                    ScheduledMeterReadingDate = null,
                };
            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);
        }

        private async Task CreateVirtualConsumptionMeteringPoint()
        {
            var request = Scenarios.CreateConsumptionMeteringPointCommand()
                with
                {
                    MeteringMethod = MeteringMethod.Virtual.Name,
                    NetSettlementGroup = NetSettlementGroup.Zero.Name,
                    ConnectionType = null,
                    ScheduledMeterReadingDate = null,
                };
            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);
        }
    }
}
