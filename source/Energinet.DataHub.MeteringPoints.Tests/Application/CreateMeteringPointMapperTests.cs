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

using AutoFixture;
using Energinet.DataHub.MeteringPoints.Application;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Application
{
    public class CreateMeteringPointMapperTests
    {
        [Fact]
        [UnitTest]
        public void CreateMeteringPointCommandShouldMapToContractAndBack()
        {
            var fixture = new Fixture();
            var outboundMapper = new Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Mappers.CreateMeteringPointMapper();
            var inboundMapper = new Energinet.DataHub.MeteringPoints.Infrastructure.Processing.Mappers.CreateMeteringPointMapper();
            var command = fixture.Create<CreateMeteringPoint>();

            var contract = (Contracts.MeteringPointEnvelope)outboundMapper.Convert(command);
            var doubleMappedCommand = inboundMapper.Convert(contract.CreateMeteringPoint) as CreateMeteringPoint;

            doubleMappedCommand.Should().BeEquivalentTo(command);
        }
    }
}
