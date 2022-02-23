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
using System.Security.Claims;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Mocks
{
    public class JwtTokenValidatorMock : IJwtTokenValidator
    {
        public Task<(bool IsValid, ClaimsPrincipal? ClaimsPrincipal)> ValidateTokenAsync(string? token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token), "Set desired user id as token in the test.");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, token),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            return Task.FromResult((true, (ClaimsPrincipal?)claimsPrincipal));
        }
    }
}
