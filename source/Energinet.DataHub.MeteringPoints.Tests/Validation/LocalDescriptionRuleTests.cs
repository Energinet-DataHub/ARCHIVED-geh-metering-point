// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    public class LocalDescriptionRuleTests : CreateMeteringPointRulesTest<LocationDescriptionMustBeValidRule>
    {
        [Theory]
        [InlineData("")]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit.")]
        public void LocalDescriptionShouldValidate(string localDescription)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                LocationDescription = localDescription,
            };

            NoErrors(request);
        }

        [Theory]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec semper neque ac lectus accumsan, vel gravida nulla pretium. Ut vitae ipsum leo. Vestibulum consectetur iaculis est venenatis tincidunt. Fusce eu tincidunt ex.", typeof(LocationDescriptionMaximumLengthValidationError))]
        public void LocalDescriptionShouldResultInError(string localDescription, Type expectedError)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                LocationDescription = localDescription,
            };

            ShouldContainErrors(request, expectedError);
        }
    }
}
