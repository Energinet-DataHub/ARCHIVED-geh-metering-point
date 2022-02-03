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
using Energinet.DataHub.MeteringPoints.Application.CloseDown;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CloseDownMeteringPoint
{
    [IntegrationTest]
    public class RequestCloseDownTests : TestHost
    {
        public RequestCloseDownTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Request_is_accepted()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateRequest();

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.AcceptCloseDownRequest);
        }

        [Fact]
        public async Task Reject_if_gsrn_number_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    GsrnNumber = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D57");
        }

        [Fact]
        public async Task Reject_if_transaction_id_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    TransactionId = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Reject_if_effective_date_is_invalid()
        {
            var request = CreateRequest()
                with
                {
                    EffectiveDate = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Fact]
        public async Task Reject_if_metering_point_does_not_exist()
        {
            var request = CreateRequest();

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        private static RequestCloseDown CreateRequest()
        {
            return new RequestCloseDown(
                SampleData.Transaction,
                SampleData.GsrnNumber,
                SampleData.EffectiveDate);
        }
    }
}
