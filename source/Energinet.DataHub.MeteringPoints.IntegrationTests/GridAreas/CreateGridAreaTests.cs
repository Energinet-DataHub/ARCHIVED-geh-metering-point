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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.GridAreas;
using Energinet.DataHub.MeteringPoints.Application.GridAreas.Create;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas.Rules;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GridAreas;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.GridAreas
{
    [IntegrationTest]
    public class CreateGridAreaTests
        : TestHost
    {
        private readonly IGridAreaRepository _gridAreaRepository;

        public CreateGridAreaTests(
            DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _gridAreaRepository = GetService<IGridAreaRepository>();
        }

        [Fact]
        public async Task Created_grid_area_should_be_retrievable_from_repository()
        {
            var request = CreateRequest();

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var gridAreaCode = GridAreaCode.Create(request.Code);
            var found = await _gridAreaRepository.GetByCodeAsync(gridAreaCode).ConfigureAwait(false);

            // TODO: compare values
            Assert.NotNull(found);
        }

        [Fact]
        public async Task Grid_area_with_invalid_name_should_be_rejected()
        {
            var request = CreateRequest() with
            {
                Name = new string('x', 51),
            };

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var resultHandler = GetService<IBusinessProcessResultHandler<CreateGridArea>>() as CreateGridAreaNullResultHandler;
            resultHandler!.Errors.Should().ContainSingle().Which.Should().BeOfType<GridAreaNameMaxLengthRuleError>();
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("00x")]
        [InlineData("00")]
        [InlineData("0000")]
        public async Task Grid_area_with_invalid_code_should_be_rejected(string code)
        {
            var request = CreateRequest() with
            {
                Code = code,
            };

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var resultHandler = GetService<IBusinessProcessResultHandler<CreateGridArea>>() as CreateGridAreaNullResultHandler;
            resultHandler!.Errors.Should().ContainSingle().Which.Should().BeOfType<GridAreaCodeFormatRuleError>();
        }

        private static CreateGridArea CreateRequest()
        {
            return new CreateGridArea(
                SampleData.GridAreaName,
                SampleData.GridAreaCode,
                SampleData.OperatorName,
                SampleData.OperatorId,
                SampleData.PriceAreaCode,
                SampleData.Transaction);
        }
    }
}
