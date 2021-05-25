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

using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class OccurenceDateValidRule : AbstractValidator<CreateMeteringPoint>
    {
        private const string OccurenceDateFormatRexEx = @"^(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})Z$";

        public OccurenceDateValidRule()
        {
            RuleFor_OccurenceDateRequired();
            RuleFor_OccurenceDateFormat();
        }

        private void RuleFor_OccurenceDateRequired()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.OccurenceDate)
                .NotEmpty()
                .WithState((createMeteringPoint) => new OccurenceRequiredValidationError());
        }

        private void RuleFor_OccurenceDateFormat()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.OccurenceDate)
                .Matches(OccurenceDateFormatRexEx)
                .WithState((createMeteringPoint) => new OccurenceDateWrongFormatValidationError(createMeteringPoint.OccurenceDate));
        }
    }
}
