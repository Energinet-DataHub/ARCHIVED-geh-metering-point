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

using System;
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
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

        private static GridAreaDetails CreateDetails()
        {
            return new GridAreaDetails(
                GridAreaName.Create(SampleData.GridAreaName),
                GridAreaCode.Create(SampleData.GridAreaCode),
                EnumerationType.FromName<PriceAreaCode>(SampleData.PriceAreaCode),
                FullFlexFromDate.Create(DateTime.Now),
                ActorId.Create("90f6f4e5-8073-4842-b33e-c59e9f4a8c3f"));
        }

        private static void AssertContainsValidationError<TValidationError>(BusinessRulesValidationResult result)
        {
            Assert.Contains(result.Errors, error => error is TValidationError);
        }
    }
}
