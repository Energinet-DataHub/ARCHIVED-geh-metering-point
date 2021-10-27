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
using Energinet.DataHub.MeteringPoints.Application.Backend.GridAreas.Create;
using Energinet.DataHub.MeteringPoints.Application.GridAreas;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
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
            var request = CreateRequest() with { Code = "123" };

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var found = await _gridAreaRepository.GetByCodeAsync(request.Code!).ConfigureAwait(false);

            found.Should().NotBeNull();
            found!.Code.Value.Should().Be(request.Code);
        }

        [Fact]
        public async Task Existing_grid_area_with_fullflex_null_should_be_retrievable_from_repository()
        {
            const string code = "870";
            var found = await _gridAreaRepository.GetByCodeAsync(code).ConfigureAwait(false);

            found.Should().NotBeNull();
            found!.Code.Value.Should().Be(code);
        }

        [Fact]
        public async Task Creating_grid_area_with_existing_code_should_be_rejected()
        {
            var request = CreateRequest();

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var resultHandler = GetService<IBusinessProcessResultHandler<CreateGridArea>>() as CreateGridAreaNullResultHandler;
            resultHandler!.Errors.Should().ContainSingle().Which.Should().BeOfType<GridAreaCodeUniqueRuleError>();
        }

        [Fact]
        public async Task Grid_area_with_invalid_name_should_be_rejected()
        {
            var request = CreateRequest() with { Name = new string('x', 51) };

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
            var request = CreateRequest() with { Code = code };

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var resultHandler = GetService<IBusinessProcessResultHandler<CreateGridArea>>() as CreateGridAreaNullResultHandler;
            resultHandler!.Errors.Should().ContainSingle().Which.Should().BeOfType<GridAreaCodeFormatRuleError>();
        }

        private static CreateGridArea CreateRequest()
        {
            return new CreateGridArea(
                SampleData.GridAreaName,
                SampleData.GridAreaCode,
                SampleData.PriceAreaCode,
                SampleData.Transaction);
        }
    }
}
