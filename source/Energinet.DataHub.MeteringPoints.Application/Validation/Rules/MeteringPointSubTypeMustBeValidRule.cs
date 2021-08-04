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

using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class MeteringPointSubTypeMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        public MeteringPointSubTypeMustBeValidRule()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.SubTypeOfMeteringPoint)
                .NotEmpty()
                .WithState(createMeteringPoint => new MeteringPointSubTypeMandatoryValidationError(createMeteringPoint.GsrnNumber));

            RuleFor(createMeteringPoint => createMeteringPoint.SubTypeOfMeteringPoint)
                .Must(SubTypeIsPhysicalOrVirtualOrCalculated)
                .WithState(createMeteringPoint => new MeteringPointSubTypeValueMustBeValidValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.SubTypeOfMeteringPoint));
        }

        private static bool SubTypeIsPhysicalOrVirtualOrCalculated(string subTypeOfMeteringPoint)
        {
            return Physical(subTypeOfMeteringPoint) ||
                   Virtual(subTypeOfMeteringPoint) ||
                   Calculated(subTypeOfMeteringPoint);
        }

        private static bool Physical(string subTypeOfMeteringPoint)
        {
            return subTypeOfMeteringPoint.Equals(MeteringPointSubType.Physical.Name);
        }

        private static bool Virtual(string subTypeOfMeteringPoint)
        {
            return subTypeOfMeteringPoint.Equals(MeteringPointSubType.Virtual.Name);
        }

        private static bool Calculated(string subTypeOfMeteringPoint)
        {
            return subTypeOfMeteringPoint.Equals(MeteringPointSubType.Calculated.Name);
        }
    }
}
