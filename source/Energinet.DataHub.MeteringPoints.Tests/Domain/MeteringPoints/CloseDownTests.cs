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

using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exceptions;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules.CloseDown;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    [UnitTest]
    public class CloseDownTests : TestBase
    {
        [Fact]
        public void Metering_point_is_closed_down()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption);

            meteringPoint.CloseDown();

            var domainEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is MeteringPointWasClosedDown) as MeteringPointWasClosedDown;
            Assert.NotNull(domainEvent);
            Assert.Equal(meteringPoint.Id.Value, domainEvent?.MeteringPointId);
        }

        [Fact]
        public void A_metering_point_can_be_closed_down_once_only()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption);
            meteringPoint.CloseDown();

            var checkResult = meteringPoint.CanCloseDown();

            AssertContainsValidationError<MeteringPointIsAlreadyClosedDown>("D16", checkResult);
            Assert.Throws<CloseDownException>(() => meteringPoint.CloseDown());
        }
    }
}
