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

using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;

namespace Energinet.DataHub.MeteringPoints.Application.Authorization.AuthorizationHandlers
{
    public class ExampleAuthorizationHandler : IAuthorizationHandler<MasterDataDocument, BusinessProcessResult>
    {
        public AuthorizationResult Authorize(MasterDataDocument command)
        {
            // if (!IsValidFormat(command.OccurenceDate))
            // {
            //     return AuthorizationResult.Error(nameof(command.OccurenceDate), GetType());
            // }
            return AuthorizationResult.Ok();
        }
    }
}
