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
using Energinet.DataHub.MeteringPoints.Domain.Actors.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.Actors
{
    public class ActorId : ValueObject
    {
        public ActorId(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static ActorId Create(string? actorId)
        {
            if (string.IsNullOrWhiteSpace(actorId))
            {
                throw new ArgumentException($"'{nameof(actorId)}' cannot be empty", nameof(actorId));
            }

            var result = CheckRules(actorId);
            if (!result.Success)
            {
                throw new InvalidOperationException("Invalid Actor Id");
            }

            return new ActorId(actorId);
        }

        public static BusinessRulesValidationResult CheckRules(string? actorId)
        {
            return new BusinessRulesValidationResult(new List<IBusinessRule>()
            {
                // TODO: Should this use its own format rule or should we use a GLN / EIC rule based on what format the string has?
                new ActorIdFormatRule(actorId),
            });
        }
    }
}
