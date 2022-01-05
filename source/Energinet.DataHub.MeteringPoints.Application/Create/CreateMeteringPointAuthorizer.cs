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
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.MeteringPoints.Application.Authorization;
using Energinet.DataHub.MeteringPoints.Application.Authorization.GridOperatorPolicies;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;

namespace Energinet.DataHub.MeteringPoints.Application.Create
{
    public class CreateMeteringPointAuthorizer
    {
        private readonly IUserContext _authenticatedUserContext;

        public CreateMeteringPointAuthorizer(IUserContext authenticatedUserContext)
        {
            _authenticatedUserContext = authenticatedUserContext ?? throw new ArgumentNullException(nameof(authenticatedUserContext));
        }

        public Task<AuthorizationResult> AuthorizeAsync(GridArea gridArea)
        {
            if (gridArea == null) throw new ArgumentNullException(nameof(gridArea));
            if (_authenticatedUserContext.CurrentUser is null)
            {
                throw new AuthenticationException("No authenticated user");
            }

            return SummarizeResultFromAsync(GetAuthorizationHandlers(gridArea));
        }

        private static async Task<AuthorizationResult> SummarizeResultFromAsync(IEnumerable<Task<AuthorizationResult>> authorizationTasks)
        {
            var results = await Task.WhenAll(authorizationTasks).ConfigureAwait(false);
            return new AuthorizationResult(results.SelectMany(result => result.Errors).ToList());
        }

        private IReadOnlyList<Task<AuthorizationResult>> GetAuthorizationHandlers(GridArea gridArea)
        {
            return new List<Task<AuthorizationResult>>()
            {
                new CurrentActorIsGridOperatorPolicy(_authenticatedUserContext).AuthorizeAsync(gridArea),
            };
        }
    }
}
