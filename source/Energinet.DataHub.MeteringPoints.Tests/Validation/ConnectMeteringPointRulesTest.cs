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
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    [UnitTest]
    public class ConnectMeteringPointRulesTest : RuleSetTest<ConnectMeteringPoint, ConnectMeteringPointRuleSet>
    {
        [Theory]
        [InlineData("571234567891234568", "2021-05-05T10:10:10Z", "a9932d11-3884-4257-8c69-09d4d88302fa")]
        [InlineData("571234567891234568", "2021-05-05T10:10:10.000Z", "a9932d11-3884-4257-8c69-09d4d88302fa")]
        public void ShouldValidate(string gsrn, string effectiveDate, string transaction)
        {
            var request = CreateRequest(gsrn, effectiveDate, transaction);

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData("x71234567891234568", "2021-05-05T10:10:10Z", "a9932d11-3884-4257-8c69-09d4d88302fa", typeof(GsrnNumberMustBeValidValidationError))]
        [InlineData("", "2021-05-05T10:10:10Z", "a9932d11-3884-4257-8c69-09d4d88302fa", typeof(GsrnNumberMustBeValidValidationError))]
        [InlineData("571234567891234568", "2021205-05T10:10:10Z", "a9932d11-3884-4257-8c69-09d4d88302fa", typeof(EffectiveDateWrongFormatValidationError))]
        [InlineData("571234567891234568", "2021-05-05T10:10:10.0000Z", "a9932d11-3884-4257-8c69-09d4d88302fa", typeof(EffectiveDateWrongFormatValidationError))]
        [InlineData("571234567891234568", "", "a9932d11-3884-4257-8c69-09d4d88302fa", typeof(EffectiveDateRequiredValidationError))]
        [InlineData("571234567891234568", "2021-05-05T10:10:10Z", "", typeof(TransactionIdentificationValidationError))]
        public void ShouldResultInError(string gsrn, string effectiveDate, string transaction, Type expectedError)
        {
            var request = CreateRequest(gsrn, effectiveDate, transaction);

            ShouldValidateWithSingleError(request, expectedError);
        }

        private static ConnectMeteringPoint CreateRequest(string gsrn, string effectiveDate, string transaction)
        {
            return new ConnectMeteringPoint
            {
                GsrnNumber = gsrn,
                EffectiveDate = effectiveDate,
                TransactionId = transaction,
            };
        }
    }
}
