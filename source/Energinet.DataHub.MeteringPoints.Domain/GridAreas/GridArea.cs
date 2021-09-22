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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.GridAreas
{
    public class GridArea
    {
        private readonly GridAreaName _name;
        private readonly GridAreaCode _code;
        private readonly string _operatorName;
        private readonly string _operatorId;
        private readonly PriceAreaCode _priceAreaCode;

        private GridArea(
            GridAreaName name,
            GridAreaCode code,
            string operatorName,
            string operatorId,
            PriceAreaCode priceAreaCode)
        {
            _name = name;
            _code = code;
            _operatorName = operatorName;
            _operatorId = operatorId;
            _priceAreaCode = priceAreaCode;
        }

        public static BusinessRulesValidationResult CanCreate(GridAreaDetails gridAreaDetails)
        {
            if (gridAreaDetails == null) throw new ArgumentNullException(nameof(gridAreaDetails));

            var rules = new Collection<IBusinessRule>()
            {
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
                GridAreaName.Create(gridAreaDetails.Name),
                gridAreaDetails.Code,
                gridAreaDetails.OperatorName,
                gridAreaDetails.OperatorId,
                EnumerationType.FromName<PriceAreaCode>(gridAreaDetails.PriceAreaCode));
        }
    }
}
