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

using System.Runtime.CompilerServices;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain
{
    public class MeteringDetailsTests : TestBase
    {
        [Fact]
        public void Meter_is_required_when_physical()
        {
            var result = MeteringConfiguration.CheckRules(MeteringMethod.Physical, null);

            AssertError<MeterIdIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Meter_is_not_allowed_when_virtual()
        {
            var result = MeteringConfiguration.CheckRules(MeteringMethod.Virtual, MeterId.Create("fakeId"));

            AssertError<MeterIdIsNotAllowedRuleError>(result, true);
        }

        [Fact]
        public void Meter_is_not_allowed_when_calculated()
        {
            var result = MeteringConfiguration.CheckRules(MeteringMethod.Calculated, MeterId.Create("fakeId"));

            AssertError<MeterIdIsNotAllowedRuleError>(result, true);
        }

        [Fact]
        public void Can_create()
        {
            var method = MeteringMethod.Physical;
            var meter = MeterId.Create("FakeId");

            var sut = MeteringConfiguration.Create(method, meter);

            Assert.Equal(method, sut.Method);
            Assert.Equal(meter, sut.Meter);
        }

        [Fact]
        public void Cannot_create()
        {
            Assert.Throws<InvalidMeteringConfigurationException>(() =>
                MeteringConfiguration.Create(MeteringMethod.Physical, null));
        }
    }
}
