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
using System.Collections.ObjectModel;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.GridAreas
{
    public class GridArea
    {
        private readonly GridAreaName _name;
        private readonly PriceAreaCode _priceAreaCode;

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind complex types in constructor
        private GridArea() { }
#pragma warning restore 8618

        private GridArea(
            GridAreaId gridAreaId,
            GridAreaName name,
            GridAreaCode code,
            PriceAreaCode priceAreaCode)
        {
            Id = gridAreaId;
            Code = code;
            _name = name;
            _priceAreaCode = priceAreaCode;
        }

        public GridAreaCode Code { get; }

        public GridAreaId Id { get; }

        public static BusinessRulesValidationResult CanCreate(GridAreaDetails gridAreaDetails)
        {
            if (gridAreaDetails == null) throw new ArgumentNullException(nameof(gridAreaDetails));

            var rules = new Collection<IBusinessRule>
            {
                new GridAreaCodeFormatRule(gridAreaDetails.Code),
                new GridAreaNameMaxLengthRule(gridAreaDetails.Name),
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
                GridAreaName.Create(gridAreaDetails.Name),
                GridAreaCode.Create(gridAreaDetails.Code),
                EnumerationType.FromName<PriceAreaCode>(gridAreaDetails.PriceAreaCode));
        }
    }
}
