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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using MediatR;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class ConsumptionMeteringPointTests
        : TestHost
    {
        private readonly IMediator _mediator;

        public ConsumptionMeteringPointTests()
        {
            _mediator = GetService<IMediator>();
        }

        [Fact]
        public async Task Should_reject_when_powerplant_is_not_specified_and_netsettlementgroup_is_not_0_or_99()
        {
            var request = CreateRequest();

            await _mediator.Send(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D57");
        }

        [Fact]
        public async Task Should_reject_when_street_name_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    StreetName = string.Empty,
                };

            await _mediator.Send(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("E86");
        }

        [Fact]
        public async Task Should_reject_when_post_code_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    PostCode = string.Empty,
                };

            await _mediator.Send(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("E86");
        }

        [Fact]
        public async Task Should_reject_when_city_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    CityName = string.Empty,
                };

            await _mediator.Send(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("E86");
        }

        private static CreateMeteringPoint CreateRequest()
        {
            return new CreateMeteringPoint(
                SampleData.StreetName,
                SampleData.BuildingNumber,
                SampleData.PostCode,
                SampleData.CityName,
                SampleData.CitySubDivisionName,
                SampleData.MunicipalityCode,
                SampleData.CountryCode,
                SampleData.StreetCode,
                SampleData.FloorIdentification,
                SampleData.RoomIdentification,
                SampleData.IsWashable,
                SampleData.GsrnNumber,
                SampleData.TypeOfMeteringPoint,
                MeteringPointSubType.Calculated.Name,
                SampleData.ReadingOccurrence,
                0,
                0,
                SampleData.MeteringGridArea,
                string.Empty,
                string.Empty,
                SampleData.SettlementMethod,
                SampleData.MeasurementUnitType,
                SampleData.DisconnectionType,
                SampleData.EffectiveDate,
                string.Empty,
                Guid.NewGuid().ToString(),
                SampleData.PhysicalState,
                NetSettlementGroup.Six.Name,
                SampleData.ConnectionType,
                SampleData.AssetType,
                "123",
                ToGrid: "456",
                ParentRelatedMeteringPoint: null,
                SampleData.ProductType,
                "0",
                SampleData.GeoInfoReference,
                SampleData.MeasurementUnitType,
                ContractedConnectionCapacity: null,
                RatedCurrent: null);
        }
    }
}
