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
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    [UnitTest]
    public class DisconnectReconnectRuleSetTests
    {
        [Fact]
        public void Validate_E79_ProcessType()
        {
            var businessRequest = CreateRequest() with
            {
                ProcessType = BusinessProcessType.DisconnectReconnectMeteringPoint.Name,
                GsrnNumber = SampleData.GsrnNumber,
                PhysicalStatusOfMeteringPoint = string.Empty,
            };

            Validate(businessRequest, typeof(PhysicalStateMandatoryValidationError), true);
        }

        private static MasterDataDocument CreateRequest()
        {
            return new();
        }

        private static List<ValidationError> GetValidationErrors(MasterDataDocument request)
        {
            var ruleSet = new ValidationRuleSet();
            var validationResult = ruleSet.Validate(request);

            return validationResult.Errors
                .Select(error => (ValidationError)error.CustomState)
                .ToList();
        }

        private static void Validate(MasterDataDocument businessRequest, System.Type validationError, bool expectedError)
        {
            var errors = GetValidationErrors(businessRequest);
            var errorType = errors.Find(error => error.GetType() == validationError);

            if (expectedError)
            {
                errorType.Should().BeOfType(validationError);
            }
            else
            {
                errorType.Should().BeNull();
            }
        }
    }
}
