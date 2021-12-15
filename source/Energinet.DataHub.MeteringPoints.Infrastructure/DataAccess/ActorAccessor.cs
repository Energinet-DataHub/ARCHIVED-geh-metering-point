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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess
{
    public class ActorAccessor
    {
        private readonly MeteringPointContext _context;

        public ActorAccessor(MeteringPointContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

#pragma warning disable // TODO: Re-visit when real actor model exists
        public Actor GetDataHub()
        {
            // TODO: Hardcoded
            return new Actor(new ActorId(Guid.NewGuid()), IdentificationType.GLN, "5790001330552", new Collection<string> { "DDZ" });
        }

        public Actor? GetByIdentifier(string number, string type)
        {
            var actors = _context.Actors
                .Where(actor =>
                    actor.IdentificationNumber == number &&
                    actor.IdentificationType == EnumerationType.FromName<IdentificationType>(type));

            return actors.FirstOrDefault();
        }
    }
}
