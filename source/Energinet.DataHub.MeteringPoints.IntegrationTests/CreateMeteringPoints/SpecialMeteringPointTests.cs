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
using Energinet.DataHub.MeteringPoints.Application.Create.Consumption;
using Energinet.DataHub.MeteringPoints.Application.Create.Special;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class SpecialMeteringPointTests : TestHost
    {
        public SpecialMeteringPointTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Should_reject_if_meter_reading_occurrence_is_not_quarterly_or_hourly()
        {
            var invalidReadingOccurrence = ReadingOccurrence.Yearly.Name;
            var request = CreateCommand()
                with
                {
                    MeterReadingOccurrence = invalidReadingOccurrence,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_type_is_ve_production_and_meter_reading_occurrence_is_not_quarterly_or_hourly_or_monthly()
        {
            var invalidReadingOccurrence = ReadingOccurrence.Yearly.Name;
            var request = Scenarios.CreateCommand(MeteringPointType.VEProduction)
                with
                {
                    MeterReadingOccurrence = invalidReadingOccurrence,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
        }

        private static CreateMeteringPoint CreateCommand()
        {
            return Scenarios.CreateCommand(MeteringPointType.ElectricalHeating);
        }
    }
}
