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
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Authorization.GridOperatorPolicies
{
    public class CurrentActorIsGridOperatorPolicy
    {
        private readonly IActorContext _actorContext;

        public CurrentActorIsGridOperatorPolicy(IActorContext actorContext)
        {
            _actorContext = actorContext;
        }

        public Task<AuthorizationResult> AuthorizeAsync(GridArea gridArea)
        {
            if (gridArea == null) throw new ArgumentNullException(nameof(gridArea));
            if (_actorContext.CurrentActor == null) throw new InvalidOperationException("No current user found");

            if (gridArea.ActorId.Value == _actorContext.CurrentActor.ActorId)
            {
                return Task.FromResult(AuthorizationResult.Ok());
            }

            return Task.FromResult(new AuthorizationResult(new List<ValidationError>()
            {
                new CurrentActorIsNotGridOperator(_actorContext.CurrentActor.Identifier, gridArea.Code.Value),
            }));
        }
    }
}
