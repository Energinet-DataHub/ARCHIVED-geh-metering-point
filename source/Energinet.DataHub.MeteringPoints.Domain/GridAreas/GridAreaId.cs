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

// NOTE: We currently don't know the requirements for a grid area, except that it is a 3 digit code for the Id
// Currently this class does not reflect a grid area, and is not used.
using System;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.GridAreas
{
    public class GridAreaId : ValueObject
    {
        public GridAreaId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static GridAreaId New()
        {
            return new GridAreaId(Guid.NewGuid());
        }
    }
}
