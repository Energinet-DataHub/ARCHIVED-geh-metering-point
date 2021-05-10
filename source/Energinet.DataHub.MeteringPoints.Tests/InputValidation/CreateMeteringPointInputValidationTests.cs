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
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.InputValidation;
using Energinet.DataHub.MeteringPoints.Application.InputValidation.Validators;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using FluentAssertions;
using SimpleInjector;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.InputValidation
{
    [UnitTest]
    public class CreateMeteringPointInputValidationTests
    {
        [Fact]
        public void InputValidations()
        {
            var container = new Container();

            container.AddInputValidation();

            var validators = container.GetAllInstances<IValidator<CreateMeteringPoint, CreateMeteringPointResult>>().Select(x => x.GetType());

            var type = typeof(IValidator<CreateMeteringPoint, CreateMeteringPointResult>);
            var types = type.Assembly
                .GetTypes()
                .Where(p => type.IsAssignableFrom(p));

            types.Should().BeEquivalentTo(validators);
        }

        [Theory]
        [InlineData("571313131313131313", true)]
        [InlineData("5713131313131313134", false)]
        [InlineData("CreateMeteringPoint32423", false)]
        public void GsrnInputValidation(string gsrn, bool expectToSucceed)
        {
            var createMeteringPoint = new CreateMeteringPoint
            {
                GsrnNumber = gsrn,
            };

            var validator = new GsrnInputValidator();
            var validationResult = validator.Validate(createMeteringPoint);

            validationResult.Success.Should().Be(expectToSucceed);
        }
    }
}
