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

using System;
using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentAssertions;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    public abstract class RuleSetTest<TRequest, TRuleSet>
        where TRuleSet : AbstractValidator<TRequest>, new()
    {
        protected IEnumerable<ValidationError> Validate(TRequest request)
        {
            var validationResult = new RuleSet().Validate(request);

            return validationResult.Errors
                .Select(error => (ValidationError)error.CustomState);
        }

        protected void ShouldValidateWithSingleError(TRequest request, Type expectedError)
        {
            var errors = Validate(request);

            errors.Should().ContainSingle(error => error.GetType() == expectedError);
        }

        protected void ShouldValidateWithNoErrors(TRequest request)
        {
            var errors = Validate(request);

            errors.Should().BeEmpty();
        }

        private class RuleSet : AbstractValidator<TRequest>
        {
            public RuleSet()
            {
                RuleFor(request => request).SetValidator(new TRuleSet());
            }
        }
    }
}
