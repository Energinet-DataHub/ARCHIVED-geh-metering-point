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

using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters;
using FluentAssertions;
using SimpleInjector;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests
{
    [UnitTest]
    public class ErrorValidationRegistrationTests
    {
        [Fact]
        public void ErrorMessageFactoryShouldMapValidationErrorToErrorDto()
        {
            var container = new Container();
            container.AddValidationErrorConversion(
                validateRegistrations: true,
                typeof(GsrnNumberMustBeValidErrorConverter).Assembly,
                typeof(GsrnNumberMustBeValidValidationError).Assembly);
            container.Verify();
            var sut = container.GetInstance<ErrorMessageFactory>();

            var validationErrors = new List<ValidationError>
            {
                new GsrnNumberMustBeValidValidationError("123"),
            };

            var validationError = validationErrors.First();
            var errorMessage = sut.GetErrorMessage(validationError);
            errorMessage.Code.Should().Be("TODO");
            errorMessage.Description.Should().Be("This might not be a gsrn number: 123");
        }

        [Fact]
        public void EnsureAllValidationErrorsAreCoveredByConverters()
        {
            var container = new Container();
            container.AddValidationErrorConversion(
                validateRegistrations: true,
                typeof(CreateMeteringPoint).Assembly, // Application
                typeof(GsrnNumberMustBeValidValidationError).Assembly, // Domain
                typeof(ErrorMessageFactory).Assembly); // Infrastructure
        }
    }
}
