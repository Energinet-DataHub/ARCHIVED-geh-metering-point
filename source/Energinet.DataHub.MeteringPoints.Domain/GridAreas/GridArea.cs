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
using System.Collections.ObjectModel;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.GridAreas
{
    public class GridArea
    {
        private readonly GridAreaName _name;
        private readonly List<GridAreaLink> _gridAreaLinks = new();

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind complex types in constructor
        private GridArea() { }
#pragma warning restore 8618

        private GridArea(
            GridAreaId gridAreaId,
            GridAreaName name,
            GridAreaCode code,
            ActorId actorId)
        {
            Id = gridAreaId;
            Code = code;
            _name = name;
            ActorId = actorId;

            AddDefaultLink();
        }

        public GridAreaCode Code { get; }

        public GridAreaId Id { get; }

        public ActorId ActorId { get; }

        public GridAreaLink DefaultLink => _gridAreaLinks.First(); // TODO: Add metering points via Grid Area instead

        public static BusinessRulesValidationResult CanCreate(GridAreaDetails gridAreaDetails)
        {
            if (gridAreaDetails == null) throw new ArgumentNullException(nameof(gridAreaDetails));

            var rules = new Collection<IBusinessRule>
            {
                // Note: For now, all rules are enforced by value objects.
            };

            return new BusinessRulesValidationResult(rules);
        }

        public static GridArea Create(GridAreaDetails gridAreaDetails)
        {
            if (gridAreaDetails == null) throw new ArgumentNullException(nameof(gridAreaDetails));

            if (!CanCreate(gridAreaDetails).Success)
            {
                throw new InvalidOperationException("Cannot create grid area due to violation of one or more business rules.");
            }

            return new GridArea(
                GridAreaId.New(),
                gridAreaDetails.Name,
                gridAreaDetails.Code,
                gridAreaDetails.ActorId);
        }

        private void AddDefaultLink()
        {
            _gridAreaLinks.Add(new GridAreaLink(GridAreaLinkId.New(), Id));
        }
    }
}
