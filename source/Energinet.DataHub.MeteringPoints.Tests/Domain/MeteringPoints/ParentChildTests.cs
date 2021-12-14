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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    [UnitTest]
    public class ParentChildTests : TestBase
    {
        [Fact]
        public void Parent_and_child_metering_points_must_reside_in_the_same_grid_area()
        {
            var childMeteringPoint = new ChildMeteringPoint(CreateMeteringPoint(MeteringPointType.ElectricalHeating));

            var result = childMeteringPoint.CanCoupleTo(CreateMeteringPoint(MeteringPointType.Consumption));

            AssertContainsValidationError<ParentAndChildGridAreasMustBeTheSame>(result);
        }

        [Fact]
        public void Only_metering_points_in_group_1_or_2_can_act_as_parent()
        {
            var childMeteringPoint = new ChildMeteringPoint(CreateMeteringPoint(MeteringPointType.ElectricalHeating));

            var result = childMeteringPoint.CanCoupleTo(CreateMeteringPoint(MeteringPointType.InternalUse));

            AssertContainsValidationError<CannotActAsParent>(result);
        }

        [Fact]
        public void Only_metering_points_in_group_5_can_act_as_child_of_a_metering_point_in_group_2()
        {
            var childMeteringPoint = new ChildMeteringPoint(CreateMeteringPoint(MeteringPointType.ElectricalHeating));

            var result = childMeteringPoint.CanCoupleTo(CreateMeteringPoint(MeteringPointType.Exchange));

            AssertContainsValidationError<CannotActAsParent>(result);
        }

        [Fact]
        public void Only_metering_points_in_group_3_and_4_can_act_as_child_of_a_metering_point_in_group_1()
        {
            var childMeteringPoint = new ChildMeteringPoint(CreateMeteringPoint(MeteringPointType.InternalUse));

            var result = childMeteringPoint.CanCoupleTo(CreateMeteringPoint(MeteringPointType.Production));

            AssertContainsValidationError<CannotActAsParent>(result);
        }
    }

#pragma warning disable
    public class ChildMeteringPoint
    {
        public ChildMeteringPoint(MeteringPoint meteringPoint)
        {
        }

        public BusinessRulesValidationResult CanCoupleTo(MeteringPoint parent)
        {
            return new BusinessRulesValidationResult(new List<ValidationError>()
            {
                new ParentAndChildGridAreasMustBeTheSame(),
                new CannotActAsParent(),
            });
        }
    }

    public class ParentAndChildGridAreasMustBeTheSame : ValidationError
    {
    }

    public class CannotActAsParent : ValidationError
    {
    }

    public class GridAreaRepositoryStub : IGridAreaRepository
    {
        private readonly List<GridArea> _gridAreas = new();

        public void Add(GridArea gridArea)
        {
            _gridAreas.Add(gridArea);
        }

        public Task<GridArea?> GetByCodeAsync(string code)
        {
            return Task.FromResult(_gridAreas.FirstOrDefault(gridArea => gridArea.Code.Value.Equals(code)));
        }
    }
}
