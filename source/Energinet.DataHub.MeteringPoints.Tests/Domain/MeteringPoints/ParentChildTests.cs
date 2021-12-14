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
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
#pragma warning disable
    [UnitTest]
    public class ParentChildTests : TestBase
    {
        [Fact]
        public async Task Parent_and_child_metering_points_must_reside_in_the_same_grid_area()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Consumption);
            var childMeteringPoint = new ChildMeteringPoint(CreateMeteringPoint(MeteringPointType.ElectricalHeating));

            AssertContainsValidationError<ParentAndChildGridAreasMustBeTheSame>(await childMeteringPoint.CanCoupleToAsync(parent));
            Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent));
        }

        [Fact]
        public async Task Only_metering_points_in_group_1_or_2_can_act_as_parent()
        {
            var parent = CreateMeteringPoint(MeteringPointType.InternalUse);
            var childMeteringPoint = new ChildMeteringPoint(CreateMeteringPoint(MeteringPointType.ElectricalHeating));

            AssertContainsValidationError<CannotActAsParent>(await childMeteringPoint.CanCoupleToAsync(parent));
            Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent));
        }

        [Fact]
        public async Task Only_metering_points_in_group_5_can_act_as_child_of_a_metering_point_in_group_2()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Exchange);
            var childMeteringPoint = new ChildMeteringPoint(CreateMeteringPoint(MeteringPointType.ElectricalHeating));

            AssertContainsValidationError<CannotActAsParent>(await childMeteringPoint.CanCoupleToAsync(parent));
            Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent));
        }

        [Fact]
        public async Task Only_metering_points_in_group_3_and_4_can_act_as_child_of_a_metering_point_in_group_1()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Production);
            var childMeteringPoint = new ChildMeteringPoint(CreateMeteringPoint(MeteringPointType.InternalUse));

            AssertContainsValidationError<CannotActAsParent>(await childMeteringPoint.CanCoupleToAsync(parent));
            Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent));
        }

        [Fact]
        public async Task Coupling_is_successful()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Consumption);
            var child = CreateMeteringPoint(MeteringPointType.NetConsumption);
            var childMeteringPoint = new ChildMeteringPoint(child);

            childMeteringPoint.CoupleToAsync(parent);

            Assert.Contains(child.DomainEvents, e => e is CoupledToParent);
        }
    }

    public class CoupledToParent : DomainEventBase
    {
    }

    public class ParentCouplingException : BusinessOperationException
    {
        public ParentCouplingException()
        {
        }

        public ParentCouplingException(string? message)
            : base(message)
        {
        }

        public ParentCouplingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ParentCouplingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    public class ChildMeteringPoint
    {
        public ChildMeteringPoint(MeteringPoint meteringPoint)
        {
        }

        public Task<BusinessRulesValidationResult> CanCoupleToAsync(MeteringPoint parent)
        {
            var errors = new List<ValidationError>();
            if (GridAreaDoesNotMatch(parent))
            {
                errors.Add(new ParentAndChildGridAreasMustBeTheSame());
            }

            errors.Add(new CannotActAsParent());

            return Task.FromResult(new BusinessRulesValidationResult(errors));
        }

        private bool GridAreaDoesNotMatch(MeteringPoint parent)
        {
            return true;
        }

        public async Task CoupleToAsync(MeteringPoint parent)
        {
            if ((await CanCoupleToAsync(parent)).Success == false)
            {
                throw new ParentCouplingException();
            }
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
