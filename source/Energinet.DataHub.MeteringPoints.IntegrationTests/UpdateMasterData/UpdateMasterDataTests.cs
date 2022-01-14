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
        public async Task Metering_method_is_changed_from_physical_to_virtual()
        {
            await CreatePhysicalConsumptionMeteringPoint().ConfigureAwait(false);
            var message = new MasterDataDocument()
                with
                {
                    EffectiveDate = _timeProvider.Now().ToString(),
                    TransactionId = SampleData.Transaction,
                    GsrnNumber = SampleData.GsrnNumber,
                    ProcessType = BusinessProcessType.ChangeMasterData.Name,
                    MeteringMethod = MeteringMethod.Virtual.Name,
                };

            await SendCommandAsync(message).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.ConfirmChangeMasterData);
            var integrationEvent = FindIntegrationEvent<MeteringConfigurationChangedIntegrationEvent>();
            Assert.NotNull(integrationEvent);
            Assert.NotEmpty(integrationEvent?.MeteringPointId);
            Assert.Equal(SampleData.GsrnNumber, integrationEvent?.GsrnNumber);
            Assert.Equal(MeteringMethod.Virtual.Name, integrationEvent?.Method);
            Assert.Empty(integrationEvent!.Meter);
        }

        [Fact]
        public async Task Metering_method_is_changed_from_virtual_to_physical()
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
                    MeterNumber = "000001",
                };

            await SendCommandAsync(message).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.ConfirmChangeMasterData);
            var integrationEvent = FindIntegrationEvent<MeteringConfigurationChangedIntegrationEvent>();
            Assert.NotNull(integrationEvent);
            Assert.NotEmpty(integrationEvent?.MeteringPointId);
            Assert.Equal(SampleData.GsrnNumber, integrationEvent?.GsrnNumber);
            Assert.Equal(MeteringMethod.Physical.Name, integrationEvent?.Method);
            Assert.Equal(message.MeterNumber, integrationEvent?.Meter);
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

        [Fact]
        public async Task Meter_is_changed()
        {
            await CreatePhysicalConsumptionMeteringPoint().ConfigureAwait(false);
            var message = new MasterDataDocument()
                with
                {
                    EffectiveDate = _timeProvider.Now().ToString(),
                    TransactionId = SampleData.Transaction,
                    GsrnNumber = SampleData.GsrnNumber,
                    ProcessType = BusinessProcessType.ChangeMasterData.Name,
                    MeterNumber = "000002",
                };

            await SendCommandAsync(message).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.ConfirmChangeMasterData);
            var integrationEvent = FindIntegrationEvent<MeteringConfigurationChangedIntegrationEvent>();
            Assert.NotNull(integrationEvent);
            Assert.NotEmpty(integrationEvent?.MeteringPointId);
            Assert.Equal(SampleData.GsrnNumber, integrationEvent?.GsrnNumber);
            Assert.Equal(MeteringMethod.Physical.Name, integrationEvent?.Method);
            Assert.Equal(message.MeterNumber, integrationEvent?.Meter);
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
