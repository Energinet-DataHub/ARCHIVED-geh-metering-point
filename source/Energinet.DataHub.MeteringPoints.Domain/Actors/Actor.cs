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

namespace Energinet.DataHub.MeteringPoints.Domain.Actors
{
    public class Actor
    {
        public Actor(ActorId actorId, IdentificationType identificationType, string identificationNumber, Collection<Role> roles)
        {
            Id = actorId;
            IdentificationType = identificationType;
            IdentificationNumber = identificationNumber;
            Roles = roles;
            CurrentRole = Role.None;
        }

        public Actor(ActorId actorId, IdentificationType identificationType, string identificationNumber, Role role)
        {
            Id = actorId;
            IdentificationType = identificationType;
            IdentificationNumber = identificationNumber;
            Roles = new Collection<Role> { role };
            CurrentRole = role;
        }

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind complex types in constructor
        private Actor()
        {
        }
#pragma warning restore 8618

        public ActorId Id { get; }

        public string IdentificationNumber { get; }

        public IdentificationType IdentificationType { get; }

        public Collection<Role> Roles { get; }

        public Role CurrentRole { get; private set; }

        public void SetRole(Role role)
        {
            if (role == null!) throw new ArgumentNullException(nameof(role));

            if (!Roles.Contains(role))
            {
                throw new InvalidOperationException($"Actor doesn't have the role: {role.Name}");
            }

            CurrentRole = role;
        }
    }
}
