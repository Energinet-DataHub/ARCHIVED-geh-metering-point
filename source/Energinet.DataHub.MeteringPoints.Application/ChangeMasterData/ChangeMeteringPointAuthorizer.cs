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
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.MeteringPoints.Application.Authorization;
using Energinet.DataHub.MeteringPoints.Application.Authorization.GridOperatorPolicies;
using Energinet.DataHub.MeteringPoints.Application.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData
{
    public class ChangeMeteringPointAuthorizer
    {
        private readonly IActorContext _authenticatedUserContext;
        private readonly IMeteringPointOwnershipProvider _ownershipProvider;

        public ChangeMeteringPointAuthorizer(IActorContext authenticatedUserContext, IMeteringPointOwnershipProvider ownershipProvider)
        {
            _authenticatedUserContext = authenticatedUserContext ?? throw new ArgumentNullException(nameof(authenticatedUserContext));
            _ownershipProvider = ownershipProvider ?? throw new ArgumentNullException(nameof(ownershipProvider));
        }

        public Task<AuthorizationResult> AuthorizeAsync(MeteringPoint meteringPoint)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));
            if (_authenticatedUserContext.CurrentActor is null)
            {
                throw new AuthenticationException("No authenticated user");
            }

            return SummarizeResultFromAsync(GetAuthorizationHandlers(meteringPoint));
        }

        private static async Task<AuthorizationResult> SummarizeResultFromAsync(IEnumerable<Task<AuthorizationResult>> authorizationTasks)
        {
            var results = await Task.WhenAll(authorizationTasks).ConfigureAwait(false);
            return new AuthorizationResult(results.SelectMany(result => result.Errors).ToList());
        }

        private IEnumerable<Task<AuthorizationResult>> GetAuthorizationHandlers(MeteringPoint request)
        {
            return new List<Task<AuthorizationResult>>
            {
                new GridOperatorIsOwnerPolicy(_ownershipProvider, _authenticatedUserContext).AuthorizeAsync(request),
            };
        }
    }
}
