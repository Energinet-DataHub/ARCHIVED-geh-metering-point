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

using System.Linq;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class MeteringPointTypeValidRule : AbstractValidator<MasterDataDocument>
    {
        public MeteringPointTypeValidRule()
        {
            TypeOfMeteringPointRequired();
            TypeOfMeteringPointShouldBeKnown();
        }

        private void TypeOfMeteringPointShouldBeKnown()
        {
            var meteringPointTypeNames = EnumerationType.GetAll<MeteringPointType>().Select(typeMe => typeMe.Name.ToUpperInvariant()).ToList();

            RuleFor(createMeteringPoint => createMeteringPoint.TypeOfMeteringPoint)
                .Must(meteringPointType => meteringPointTypeNames.Contains(meteringPointType.ToUpperInvariant()))
                .WithState(createMeteringPoint => new MeteringPointTypeValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.TypeOfMeteringPoint));
        }

        private void TypeOfMeteringPointRequired()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.TypeOfMeteringPoint)
                .NotEmpty()
                .WithState(createMeteringPoint => new MeteringPointTypeRequiredValidationError(createMeteringPoint.GsrnNumber));
        }
    }
}
