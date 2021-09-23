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

using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.GridAreas
{
    [UnitTest]
    public class CreationTests
    {
        [Fact]
        public void Should_succeed()
        {
            var details = CreateDetails();

            var gridArea = GridArea.Create(details);

            gridArea.Should().NotBeNull();
        }

        [Fact]
        public void Grid_area_with_invalid_name_should_be_rejected()
        {
            var details = CreateDetails() with { Name = new string('x', 51) };

            var result = GridArea.CanCreate(details);

            AssertContainsValidationError<GridAreaNameMaxLengthRuleError>(result);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("00x")]
        [InlineData("00")]
        [InlineData("0000")]
        public void Grid_area_with_invalid_code_should_be_rejected(string code)
        {
            var details = CreateDetails() with { Code = code };

            var result = GridArea.CanCreate(details);

            AssertContainsValidationError<GridAreaCodeFormatRuleError>(result);
        }

        private static GridAreaDetails CreateDetails()
        {
            return new GridAreaDetails(
                SampleData.GridAreaName,
                SampleData.GridAreaCode,
                SampleData.PriceAreaCode);
        }

        private static void AssertContainsValidationError<TValidationError>(BusinessRulesValidationResult result)
        {
            Assert.Contains(result.Errors, error => error is TValidationError);
        }
    }
}
