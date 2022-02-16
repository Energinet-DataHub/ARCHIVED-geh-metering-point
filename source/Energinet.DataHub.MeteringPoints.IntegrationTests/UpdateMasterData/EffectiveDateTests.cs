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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class EffectiveDateTests : TestHost
    {
        public EffectiveDateTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Effective_date_is_stored()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    PowerPlant = SampleData.PowerPlantGsrnNumber,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasEffectiveDate(EffectiveDate.Create(request.EffectiveDate));
        }

        [Fact]
        public async Task Effective_date_is_required()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()
                with
                {
                    EffectiveDate = string.Empty,
                }).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Theory]
        [InlineData("2021-01-01T18:00:00Z", "2021-01-02T23:00:00Z", true)]
        [InlineData("2021-01-01T18:00:00Z", "2020-12-30T23:00:00Z", true)]
        [InlineData("2021-01-01T18:00:00Z", "2021-01-01T23:00:00Z", false)]
        [InlineData("2021-01-01T18:00:00Z", "2020-12-31T23:00:00Z", false)]
        public async Task Effective_date_is_today_or_the_day_before(string today, string effectiveDate, bool expectError)
        {
            var timeProvider = GetService<ISystemDateTimeProvider>() as RunnableDateTimeProviderStub;
            timeProvider!.SetNow(InstantPattern.General.Parse(today).Value);

            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            await SendCommandAsync(CreateUpdateRequest()
                with
                {
                    EffectiveDate = effectiveDate,
                }).ConfigureAwait(false);

            AssertValidationError("E17", expectError);
        }
    }
}
