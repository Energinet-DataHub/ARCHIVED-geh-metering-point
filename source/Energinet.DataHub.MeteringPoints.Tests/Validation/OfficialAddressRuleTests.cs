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
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    [UnitTest]
    public class OfficialAddressRuleTests : CreateMeteringPointRulesTest<OfficialAddressRule>
    {
        [Theory]
        [InlineData("7511e205-cd43-44e3-9a17-7664f66a5d07", true, nameof(MeteringPointType.Production))]
        [InlineData("7511e205-cd43-44e3-9a17-7664f66a5d07", true, nameof(MeteringPointType.ElectricalHeating))]
        [InlineData("", false, nameof(MeteringPointType.ElectricalHeating))]
        public void OfficialAddressShouldValidate(string reference, bool isOfficialAddress, string meteringPointType)
        {
            var request = CreateRequest() with
            {
                GeoInfoReference = reference,
                IsOfficialAddress = isOfficialAddress,
                TypeOfMeteringPoint = meteringPointType,
            };

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData("7511e205-cd43-44e3-9a17-7664f66a5d071", true, nameof(MeteringPointType.Production), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        [InlineData("7511e205-cd43-44e3-9a17-7664f66a5d071", false, nameof(MeteringPointType.Production), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        [InlineData("7511e205-cd43-44e3-9a17-7664f66a5d071", true, nameof(MeteringPointType.Consumption), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        [InlineData("7511e205cd4344e39a177664f66a5d07", true, nameof(MeteringPointType.Production), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        [InlineData("7511e205cd4344e39a177664f66a5d075d07", true, nameof(MeteringPointType.Production), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        [InlineData("", true, nameof(MeteringPointType.ElectricalHeating), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        [InlineData("x", true, nameof(MeteringPointType.ElectricalHeating), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        [InlineData("x", null, nameof(MeteringPointType.ElectricalHeating), typeof(OfficialAddressIsMandatoryWhenGeoInfoReferenceIsPresentValidationError))]
        [InlineData("x", false, nameof(MeteringPointType.ElectricalHeating), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        [InlineData("x", true, nameof(MeteringPointType.Production), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        [InlineData("x", null, nameof(MeteringPointType.Production), typeof(OfficialAddressIsMandatoryWhenGeoInfoReferenceIsPresentValidationError))]
        [InlineData("x", false, nameof(MeteringPointType.Production), typeof(GeoInfoReferenceIsMandatoryValidationError))]
        public void OfficialAddressShouldResultInError(string reference, bool? isOfficialAddress, string meteringPointType, Type expectedError)
        {
            var request = CreateRequest() with
            {
                GeoInfoReference = reference,
                IsOfficialAddress = isOfficialAddress,
                TypeOfMeteringPoint = meteringPointType,
            };

            ShouldValidateWithSingleError(request, expectedError);
        }
    }
}
