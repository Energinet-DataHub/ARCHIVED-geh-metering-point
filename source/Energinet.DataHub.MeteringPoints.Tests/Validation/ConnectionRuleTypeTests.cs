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
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    public class ConnectionRuleTypeTests : CreateMeteringPointRulesTest<ConnectionTypeRule>
    {
        [Theory]
        [InlineData(nameof(ConnectionType.Direct), nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(ConnectionType.Direct), nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(ConnectionType.Installation), nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(ConnectionType.Installation), nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(ConnectionType.Direct), nameof(MeteringPointType.VEProduction), nameof(NetSettlementGroup.Zero))]
        [InlineData(nameof(ConnectionType.Installation), nameof(MeteringPointType.Analysis), nameof(NetSettlementGroup.Ninetynine))]
        [InlineData(nameof(ConnectionType.Installation), nameof(MeteringPointType.Analysis), nameof(NetSettlementGroup.Three))]
        [InlineData("", nameof(MeteringPointType.VEProduction), nameof(NetSettlementGroup.Zero))]
        public void ConnectionShouldValidate(string connectionType, string meteringPointType, string netSettlementGroup)
        {
            var request = CreateRequest() with
            {
                ConnectionType = connectionType,
                TypeOfMeteringPoint = meteringPointType,
                NetSettlementGroup = netSettlementGroup,
            };

            var errors = Validate(request);

            errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(nameof(ConnectionType.Direct), nameof(MeteringPointType.Production), nameof(NetSettlementGroup.Zero), typeof(ConnectionTypeMandatoryValidationError))]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One), typeof(ConnectionTypeMandatoryValidationError))]
        [InlineData("Connection Test Type", nameof(MeteringPointType.VEProduction), nameof(NetSettlementGroup.One), typeof(ConnectionTypeWrongValueValidationError))]
        [InlineData(nameof(ConnectionType.Direct), nameof(MeteringPointType.VEProduction), nameof(NetSettlementGroup.Six), typeof(ConnectionTypeNetSettlementGroupValidationError))]
        public void ConnectShouldResultInError(string connectionType, string meteringPointType, string netSettlementGroup, Type expectedError)
        {
            var request = CreateRequest() with
            {
                ConnectionType = connectionType,
                TypeOfMeteringPoint = meteringPointType,
                NetSettlementGroup = netSettlementGroup,
            };

            var errors = Validate(request);

            errors.Should().ContainSingle(error => error.GetType() == expectedError);
        }
    }
}
