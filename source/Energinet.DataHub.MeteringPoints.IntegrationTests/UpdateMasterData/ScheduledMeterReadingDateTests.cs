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
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class ScheduledMeterReadingDateTests : TestHost
    {
        public ScheduledMeterReadingDateTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Scheduled_meter_reading_date_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand() with
            {
                ScheduledMeterReadingDate = "0101",
            }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ScheduledMeterReadingDate = "0201",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasScheduledMeterReadingDate("0201");
        }

        [Fact]
        public async Task Scheduled_meter_reading_date_is_rejected_if_format_is_incorrect()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ScheduledMeterReadingDate = "020122",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Cannot_be_removed_if_required()
        {
            await CreateConsumptionMeteringPointInNetSettlementGroup6Async().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ScheduledMeterReadingDate = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E0H");
        }
    }
}
