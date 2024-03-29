﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MeteringPoints.Application.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Authorization.GridOperatorPolicies
{
    public class GridOperatorIsOwnerPolicy
    {
        private readonly IMeteringPointOwnershipProvider _ownershipProvider;
        private readonly IActorContext _actorContext;

        public GridOperatorIsOwnerPolicy(IMeteringPointOwnershipProvider ownershipProvider, IActorContext actorContext)
        {
            _ownershipProvider = ownershipProvider ?? throw new ArgumentNullException(nameof(ownershipProvider));
            _actorContext = actorContext ?? throw new ArgumentNullException(nameof(actorContext));
        }

        public async Task<AuthorizationResult> AuthorizeAsync(MeteringPoint meteringPoint)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));

            var ownerOfMeteringPoint = await _ownershipProvider.GetOwnerAsync(meteringPoint).ConfigureAwait(false);
            if (ownerOfMeteringPoint.ActorId == _actorContext.CurrentActor?.ActorId)
            {
                return AuthorizationResult.Ok();
            }

            return new AuthorizationResult(new List<ValidationError>
            {
                new GridOperatorIsNotOwnerOfMeteringPoint(),
            });
        }
    }
}
