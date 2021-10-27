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
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments.Validation;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    [UnitTest]
    public class DisconnectionRuleTests : CreateMeteringPointRulesTest<DisconnectionTypeRule>
    {
        [Theory]
        [InlineData(nameof(DisconnectionType.Manual), nameof(MeteringPointType.Production))]
        [InlineData(nameof(DisconnectionType.Manual), nameof(MeteringPointType.Consumption))]
        [InlineData(nameof(DisconnectionType.Remote), nameof(MeteringPointType.Production))]
        [InlineData(nameof(DisconnectionType.Remote), nameof(MeteringPointType.Consumption))]
        public void DisconnectionShouldValidate(string disconnectionType, string meteringPointType)
        {
            var request = CreateRequest() with
            {
                DisconnectionType = disconnectionType,
                TypeOfMeteringPoint = meteringPointType,
            };

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData(nameof(DisconnectionType.Manual), nameof(MeteringPointType.VEProduction), typeof(DisconnectionTypeMandatoryValidationError))]
        [InlineData("", nameof(MeteringPointType.Production), typeof(DisconnectionTypeMandatoryValidationError))]
        [InlineData("Disconnection Test Type", nameof(MeteringPointType.Production), typeof(DisconnectionTypeWrongValueValidationError))]
        public void DisconnectShouldResultInError(string disconnectionType, string meteringPointType, Type expectedError)
        {
            var request = CreateRequest() with
            {
                DisconnectionType = disconnectionType,
                TypeOfMeteringPoint = meteringPointType,
            };

            ShouldValidateWithSingleError(request, expectedError);
        }
    }
}
