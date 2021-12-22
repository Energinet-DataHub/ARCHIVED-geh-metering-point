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

using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.MeteringPoints.Domain.Actors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Actors
{
    public class ActorProvider
    {
        private readonly IUserContext _userContext;

        public ActorProvider(IUserContext userContext)
        {
            _userContext = userContext;
        }

#pragma warning disable CA1822 // Could be static
        public Actor DataHub => new("5790001330552", IdentificationType.GLN.ToString(), Role.MeteringPointAdministrator.ToString());
#pragma warning restore CA1822

        // TODO: Fix with new user context
        public Actor CurrentActor => new(_userContext.CurrentUser!.ActorId.ToString(), IdentificationType.GLN.ToString(), Role.GridAccessProvider.ToString());
    }
}
