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
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild.Exceptions;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild.Rules;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    [UnitTest]
    public class ParentChildTests : TestBase
    {
        private readonly IGridAreaRepository _gridAreaRepository;
        private readonly GridArea _defaultGridArea;

        public ParentChildTests()
        {
            _gridAreaRepository = new GridAreaRepositoryStub();
            _defaultGridArea = CreateGridArea();
        }

        [Fact]
        public async Task Can_not_couple_to_a_parent_that_is_closed_down()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Consumption, CreateGridArea());
            var childMeteringPoint = CreateChildMeteringPoint(MeteringPointType.ElectricalHeating);
            parent.CloseDown();

            AssertContainsValidationError<CannotCoupleToClosedDownParent>("D16", await childMeteringPoint.CanCoupleToAsync(parent).ConfigureAwait(false));
            await Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Parent_and_child_metering_points_must_reside_in_the_same_grid_area()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Consumption, CreateGridArea());
            var childMeteringPoint = CreateChildMeteringPoint(MeteringPointType.ElectricalHeating);

            AssertContainsValidationError<ParentAndChildGridAreasMustBeTheSame>("D46", await childMeteringPoint.CanCoupleToAsync(parent).ConfigureAwait(false));
            await Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Only_metering_points_in_group_1_or_2_can_act_as_parent()
        {
            var parent = CreateMeteringPoint(MeteringPointType.InternalUse);
            var childMeteringPoint = CreateChildMeteringPoint(MeteringPointType.ElectricalHeating);

            AssertContainsValidationError<CannotActAsParent>("D18", await childMeteringPoint.CanCoupleToAsync(parent).ConfigureAwait(false));
            await Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Only_metering_points_in_group_5_can_act_as_child_of_a_metering_point_in_group_2()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Exchange);
            var childMeteringPoint = CreateChildMeteringPoint(MeteringPointType.Consumption);

            AssertContainsValidationError<CannotActAsChildOfGroup2MeteringPoints>("D18", await childMeteringPoint.CanCoupleToAsync(parent).ConfigureAwait(false));
            await Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Only_metering_points_in_group_3_and_4_can_act_as_child_of_a_metering_point_in_group_1()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Production);
            var childMeteringPoint = CreateChildMeteringPoint(MeteringPointType.Consumption);

            AssertContainsValidationError<CannotActAsChildOfGroup1MeteringPoints>("D18", await childMeteringPoint.CanCoupleToAsync(parent).ConfigureAwait(false));
            await Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Coupling_is_successful()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Consumption);
            var child = CreateMeteringPoint(MeteringPointType.NetConsumption);
            var childMeteringPoint = new ChildMeteringPoint(child, _gridAreaRepository);

            await childMeteringPoint.CoupleToAsync(parent).ConfigureAwait(false);

            Assert.Contains(child.DomainEvents, e => e is CoupledToParent);
        }

        [Fact]
        public async Task Decoupling_is_successful()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Consumption);
            var child = CreateMeteringPoint(MeteringPointType.NetConsumption);
            var childMeteringPoint = new ChildMeteringPoint(child, _gridAreaRepository);
            await childMeteringPoint.CoupleToAsync(parent).ConfigureAwait(false);

            childMeteringPoint.Decouple();

            Assert.Contains(child.DomainEvents, e => e is DecoupledFromParent);
        }

        private ChildMeteringPoint CreateChildMeteringPoint(MeteringPointType type)
        {
            return new ChildMeteringPoint(CreateMeteringPoint(type), _gridAreaRepository);
        }

        private MeteringPoint CreateMeteringPoint(MeteringPointType type, GridArea? gridArea = null)
        {
            return MeteringPoint.Create(
                MeteringPointId.New(),
                GsrnNumber.Create(SampleData.GsrnNumber),
                type,
                gridArea is not null ? gridArea.DefaultLink.Id : _defaultGridArea.DefaultLink.Id,
                MasterDataBuilder(type).Build());
        }

        private GridArea CreateGridArea()
        {
            var gridArea = GridArea.Create(new GridAreaDetails(GridAreaName.Create("870"), GridAreaCode.Create("870"), PriceAreaCode.DK1, null, ActorId.Create()));
            _gridAreaRepository.Add(gridArea);
            return gridArea;
        }
    }
}
