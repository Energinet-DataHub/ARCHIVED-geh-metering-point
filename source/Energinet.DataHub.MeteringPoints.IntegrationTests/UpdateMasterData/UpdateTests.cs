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
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class UpdateTests : TestHost
    {
        public UpdateTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Confirm_should_contain_correct_business_reason_code()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);
            var request = CreateUpdateRequest();
            await SendCommandAsync(request).ConfigureAwait(false);

            await AssertMeteringPointExistsAsync(request.GsrnNumber).ConfigureAwait(false);
            AssertConfirmMessage(DocumentType.ConfirmChangeMasterData, "E32");
        }

        [Fact]
        public async Task Reject_should_contain_correct_business_reason_code()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);
            var request = CreateUpdateRequest()
                with
                {
                    StreetCode = "invalid value",
                };
            await SendCommandAsync(request).ConfigureAwait(false);
            AssertRejectMessage(DocumentType.RejectChangeMasterData, "E32");
        }
    }
}
