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
        private readonly IGridAreaRepository _gridAreaRepository;
        private readonly GridArea _defaultGridArea;

        public ParentChildTests()
        {
            _gridAreaRepository = new GridAreaRepositoryStub();
            _defaultGridArea = CreateGridArea();
        }

        [Fact]
        public async Task Parent_and_child_metering_points_must_reside_in_the_same_grid_area()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Consumption, CreateGridArea());
            var childMeteringPoint = CreateChildMeteringPoint(MeteringPointType.ElectricalHeating);

            AssertContainsValidationError<ParentAndChildGridAreasMustBeTheSame>(await childMeteringPoint.CanCoupleToAsync(parent));
            Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent));
        }

        [Fact]
        public async Task Only_metering_points_in_group_1_or_2_can_act_as_parent()
        {
            var parent = CreateMeteringPoint(MeteringPointType.InternalUse);
            var childMeteringPoint = CreateChildMeteringPoint(MeteringPointType.ElectricalHeating);

            AssertContainsValidationError<CannotActAsParent>(await childMeteringPoint.CanCoupleToAsync(parent));
            Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent));
        }

        [Fact]
        public async Task Only_metering_points_in_group_5_can_act_as_child_of_a_metering_point_in_group_2()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Exchange);
            var childMeteringPoint = CreateChildMeteringPoint(MeteringPointType.ElectricalHeating);

            AssertContainsValidationError<CannotActAsParent>(await childMeteringPoint.CanCoupleToAsync(parent));
            Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent));
        }

        [Fact]
        public async Task Only_metering_points_in_group_3_and_4_can_act_as_child_of_a_metering_point_in_group_1()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Production);
            var childMeteringPoint = CreateChildMeteringPoint(MeteringPointType.Consumption);

            AssertContainsValidationError<CannotActAsChild>(await childMeteringPoint.CanCoupleToAsync(parent));
            Assert.ThrowsAsync<ParentCouplingException>(() => childMeteringPoint.CoupleToAsync(parent));
        }

        [Fact]
        public async Task Coupling_is_successful()
        {
            var parent = CreateMeteringPoint(MeteringPointType.Consumption);
            var child = CreateMeteringPoint(MeteringPointType.NetConsumption);
            var childMeteringPoint = new ChildMeteringPoint(child, _gridAreaRepository);

            await childMeteringPoint.CoupleToAsync(parent);

            Assert.Contains(child.DomainEvents, e => e is CoupledToParent);
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
                EffectiveDate.Create(SampleData.EffectiveDate),
                MasterDataBuilder(type).Build());
        }

        private GridArea CreateGridArea()
        {
            var gridArea = GridArea.Create(new GridAreaDetails(GridAreaName.Create("870"), GridAreaCode.Create("870"), PriceAreaCode.DK1, null));
            _gridAreaRepository.Add(gridArea);
            return gridArea;
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
        private readonly MeteringPoint _meteringPoint;
        private readonly IGridAreaRepository _gridAreaRepository;

        public ChildMeteringPoint(MeteringPoint meteringPoint, IGridAreaRepository gridAreaRepository)
        {
            _meteringPoint = meteringPoint ?? throw new ArgumentNullException(nameof(meteringPoint));
            _gridAreaRepository = gridAreaRepository ?? throw new ArgumentNullException(nameof(gridAreaRepository));
        }

        public async Task<BusinessRulesValidationResult> CanCoupleToAsync(MeteringPoint parent)
        {
            var errors = new List<ValidationError>();
            if (await GridAreasMatchAsync(parent) == false)
            {
                errors.Add(new ParentAndChildGridAreasMustBeTheSame());
            }

            var rules = new List<IBusinessRule>()
            {
                new OnlySpecificGroupsCanActAsParentRule(parent),
                new OnlySpecificGroupsCanActAsChildOfGroup1(parent, _meteringPoint),
            };

            errors.AddRange(rules.Where(rule => rule.IsBroken).Select(rule => rule.ValidationError).ToList());

            return new BusinessRulesValidationResult(errors);
        }

        private async Task<bool> GridAreasMatchAsync(MeteringPoint parent)
        {
            var parentGridArea = await _gridAreaRepository.GetByLinkIdAsync(parent.GridAreaLinkId);
            var childGridArea = await _gridAreaRepository.GetByLinkIdAsync(_meteringPoint.GridAreaLinkId);
            return parentGridArea.Equals(childGridArea);
        }

        public async Task CoupleToAsync(MeteringPoint parent)
        {
            if ((await CanCoupleToAsync(parent)).Success == false)
            {
                throw new ParentCouplingException();
            }
        }
    }

    public class OnlySpecificGroupsCanActAsChildOfGroup1 : IBusinessRule
    {
        public OnlySpecificGroupsCanActAsChildOfGroup1(MeteringPoint parent, MeteringPoint meteringPoint)
        {
            if (parent.MeteringPointType.MeteringPointGroup == 1)
            {
                IsBroken = meteringPoint.MeteringPointType.MeteringPointGroup is not (3 or 4);
            }
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError => new CannotActAsChild();
    }

    public class CannotActAsChild : ValidationError
    {
    }

    public class OnlySpecificGroupsCanActAsParentRule : IBusinessRule
    {
        public OnlySpecificGroupsCanActAsParentRule(MeteringPoint parent)
        {
            IsBroken = parent.MeteringPointType.MeteringPointGroup is not (1 or 2);
        }

        public bool IsBroken { get; }
        public ValidationError ValidationError => new CannotActAsParent();
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

        public GridAreaRepositoryStub()
        {
        }
        public void Add(GridArea gridArea)
        {
            _gridAreas.Add(gridArea);
        }

        public Task<GridArea?> GetByCodeAsync(string code)
        {
            return Task.FromResult(_gridAreas.FirstOrDefault(gridArea => gridArea.Code.Value.Equals(code)));
        }

        public Task<GridArea?> GetByLinkIdAsync(GridAreaLinkId linkId)
        {
            return Task.FromResult(_gridAreas.FirstOrDefault(gridArea => gridArea.DefaultLink.Id.Equals(linkId)));
        }
    }
}
