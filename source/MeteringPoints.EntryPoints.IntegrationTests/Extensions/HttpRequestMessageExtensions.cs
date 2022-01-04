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
using System.Net.Http;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static void AddFakeJwtToken(this HttpRequestMessage request, UserIdentity userIdentity)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var token = FakeJwtGenerator.GenerateToken(userIdentity);

            request.Headers.Add("Authorization", $"Bearer {token}");
        }

        public static void AddDefaultJwtToken(this HttpRequestMessage requestMessage)
        {
            AddFakeJwtToken(requestMessage, new UserIdentity(Guid.Parse("158725db-35b5-4740-8ba4-80c616ec9f92"), "SomeRole", "GLN", "8200000001409"));
        }
    }
}
