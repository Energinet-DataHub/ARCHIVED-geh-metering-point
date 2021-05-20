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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using MediatR;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    #pragma warning disable
    public class CreateTests
        : TestHost
    {
        private readonly IMediator _mediator;
        private readonly IMeteringPointRepository _meteringPointRepository;

        public CreateTests()
        {
            _mediator = GetService<IMediator>();
            _meteringPointRepository = GetService<IMeteringPointRepository>();
        }

        [Fact]
        public async Task Create_WhenNoValidationErrors_IsSuccessful()
        {
            var request = new CreateMeteringPoint(new Address("", "", "", ""))
            {
                GsrnNumber = SampleData.GsrnNumber,
                MaximumCurrent = 1,
                AssetType = "",
                ConnectionType = "",
                DisconnectionType = "",
                LocationDescription = "",
                MaximumPower = 1,
                MeterNumber = "",
                OccurenceDate = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                PowerPlant = "",
                ProductType = "",
                SettlementMethod = "",
                UnitType = "",
                MeteringGridArea = "",
                MeterReadingOccurrence = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                NetSettlementGroup = "",
                ParentRelatedMeteringPoint = "",
                TypeOfMeteringPoint = "",
                PhysicalStatusOfMeteringPoint = "",
                SubTypeOfMeteringPoint = "",
            };

            await _mediator.Send(request, CancellationToken.None);

            var gsrnNumber = GsrnNumber.Create(request.GsrnNumber);
            var found = await _meteringPointRepository.GetByGsrnNumberAsync(gsrnNumber);
            Assert.NotNull(found);
        }
    }
}
