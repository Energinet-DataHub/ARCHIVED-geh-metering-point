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
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess
{
    public class ActorAccessor
    {
#pragma warning disable // TODO: Re-visit when real actor model exists
        public Actor GetDataHub()
        {
            return new Actor("5790001330552", "GS1", "DDZ");
        }

        public Actor GetByIdentifier(string glnNumber, string type)
        {
            return new Actor(glnNumber, type, "Foo");
        }

        // TODO: Remove when real actor model exists
        public class Actor
        {
            public Actor(string id, string idType, string role)
            {
                Id = id;
                IdType = idType;
                Role = role;
            }

            public string Id { get; set; }

            public string IdType { get; set; }

            public string Role { get; set; }
        }
    }
}
