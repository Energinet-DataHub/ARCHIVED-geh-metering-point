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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Sdk;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain
{
    public abstract class TestBase
    {
        protected static void AssertError<TRuleError>(BusinessRulesValidationResult rulesValidationResult, bool errorExpected)
        {
            if (rulesValidationResult == null) throw new ArgumentNullException(nameof(rulesValidationResult));
            var hasError = rulesValidationResult.Errors.Any(error => error is TRuleError);
            Assert.Equal(errorExpected, hasError);
        }

        protected static MeteringPointDetails CreateDetails()
        {
            var address = Address.Create(
                SampleData.StreetName,
                SampleData.StreetCode,
                string.Empty,
                SampleData.CityName,
                string.Empty,
                SampleData.PostCode,
                null,
                string.Empty,
                string.Empty,
                default);

            return new MeteringPointDetails(
                MeteringPointId.New(),
                GsrnNumber.Create(SampleData.GsrnNumber),
                address,
                SampleData.IsOfficialAddress,
                MeteringPointSubType.Physical,
                GridAreaId.New(),
                GsrnNumber.Create(SampleData.PowerPlant),
                LocationDescription.Create(SampleData.LocationDescription),
                string.IsNullOrWhiteSpace(SampleData.MeterNumber) ? null : MeterId.Create(SampleData.MeterNumber),
                ReadingOccurrence.Hourly,
                PowerLimit.Create(SampleData.MaximumPower, SampleData.MaximumCurrent),
                EffectiveDate.Create(SampleData.EffectiveDate),
                SettlementMethod.Flex,
                NetSettlementGroup.Six,
                DisconnectionType.Manual,
                ConnectionType.Installation,
                AssetType.GasTurbine,
                ScheduledMeterReadingDate.Create(SampleData.ScheduledMeterReadingDate));
        }
    }
}
