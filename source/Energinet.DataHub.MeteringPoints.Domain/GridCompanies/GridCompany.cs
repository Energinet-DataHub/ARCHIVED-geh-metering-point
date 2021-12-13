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

namespace Energinet.DataHub.MeteringPoints.Domain.GridCompanies
{
    public class GridCompany
    {
        public GridCompany(GridCompanyId gridCompanyId, ActorType actorType, string actorId)
        {
            Id = gridCompanyId;
            ActorType = actorType;
            ActorId = actorId;
        }

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind complex types in constructor
        private GridCompany()
        {
        }
#pragma warning restore 8618

        public GridCompanyId Id { get; }

        public string ActorId { get; }

        public ActorType ActorType { get; }
    }
}
