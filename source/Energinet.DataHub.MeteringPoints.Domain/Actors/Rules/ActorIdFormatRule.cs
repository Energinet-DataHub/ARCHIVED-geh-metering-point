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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.Actors.Rules
{
    public class ActorIdFormatRule : IBusinessRule
    {
        private readonly string? _actorIdValue;

        public ActorIdFormatRule(string? actorIdValue)
        {
            _actorIdValue = actorIdValue;
        }

        public bool IsBroken => !IsValid;

        public ValidationError ValidationError => new ActorIdFormatRuleError();

        private static bool IsValid => IsValidEic() || IsValidGln();

        private static bool IsValidEic()
        {
            // TODO: Should this be a standalone validation or should we create an EIC object instead?
            throw new NotImplementedException();
        }

        private static bool IsValidGln()
        {
            // TODO: Should this be a standalone validation or use the GLN number object instead?
            throw new NotImplementedException();
        }
    }
}
