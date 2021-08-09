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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    public class PhysicalStateTests : CreateMeteringPointRulesTest<PhysicalStateRule>
    {
        [Theory]
        [InlineData(nameof(PhysicalState.New), nameof(MeteringPointType.Consumption))]
        public void ShouldValidate(string physicalState, string meteringPointType)
        {
            var request = CreateRequest() with
            {
                PhysicalStatusOfMeteringPoint = physicalState,
                TypeOfMeteringPoint = meteringPointType,
            };

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData("", nameof(MeteringPointType.Consumption), typeof(PhysicalStateMandatoryValidationError))]
        [InlineData(null, nameof(MeteringPointType.Consumption), typeof(PhysicalStateMandatoryValidationError))]
        public void ShouldResultInError(string physicalState, string meteringPointType, Type expectedError)
        {
            var request = CreateRequest() with
            {
                PhysicalStatusOfMeteringPoint = physicalState,
                TypeOfMeteringPoint = meteringPointType,
            };

            ShouldValidateWithSingleError(request, expectedError);
        }
    }
}
