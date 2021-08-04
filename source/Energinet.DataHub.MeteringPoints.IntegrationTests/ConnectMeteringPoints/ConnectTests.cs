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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using FluentAssertions;
using MediatR;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.ConnectMeteringPoints
{
    public class ConnectTests
        : TestHost
    {
        private readonly IMediator _mediator;
        private readonly IOutboxManager _outbox;
        private readonly ISystemDateTimeProvider _dateTimeProvider;

        public ConnectTests()
        {
            _mediator = GetService<IMediator>();
            _outbox = GetService<IOutboxManager>();
            _dateTimeProvider = GetService<ISystemDateTimeProvider>();
        }

        [Fact]
        public async Task ConnectMeteringPoint_WithNoValidationErrors_ShouldGenerateConfirmMessageInOutbox()
        {
            var createMeteringPointRequest = CreateMeteringPointRequest();
            var connectMeteringPointRequest = CreateConnectMeteringPointRequest();

            await _mediator.Send(createMeteringPointRequest, CancellationToken.None).ConfigureAwait(false);
            await _mediator.Send(connectMeteringPointRequest, CancellationToken.None).ConfigureAwait(false);

            var meteringPointConnectedOutboxMessage = _outbox.GetNext(OutboxMessageCategory.ActorMessage, typeof(ConnectMeteringPointAccepted).FullName!);
            meteringPointConnectedOutboxMessage.Should().NotBeNull();
            meteringPointConnectedOutboxMessage?.Type.Should().Be(typeof(ConnectMeteringPointAccepted).FullName);
        }

        [Fact]
        public async Task ConnectMeteringPoint_WithValidationErrors_ShouldGenerateRejectMessageInOutbox()
        {
            var createMeteringPointRequest = CreateMeteringPointRequest();

            var connectMeteringPointRequest = CreateConnectMeteringPointRequest() with
            {
                GsrnNumber = "This is not a valid GSRN number",
            };

            await _mediator.Send(createMeteringPointRequest, CancellationToken.None).ConfigureAwait(false);
            await _mediator.Send(connectMeteringPointRequest, CancellationToken.None).ConfigureAwait(false);

            var outboxMessage = _outbox.GetNext(OutboxMessageCategory.ActorMessage, typeof(ConnectMeteringPointRejected).FullName!);
            outboxMessage.Should().NotBeNull();
            outboxMessage?.Type.Should().Be(typeof(ConnectMeteringPointRejected).FullName);
        }

        [Fact(Skip = "Not implemented yet")]
        public void ConnectMeteringPoint_WithAlreadyConnected_ShouldGenerateRejectMessageInOutbox()
        {
        }

        [Fact(Skip = "Not implemented yet")]
        public void ConnectMeteringPoint_WhenEffectiveDateIsOutOfScope_ShouldGenerateRejectMessageInOutbox()
        {
        }

        private static CreateMeteringPoint CreateMeteringPointRequest()
        {
            return new(
                SampleData.StreetName,
                SampleData.PostCode,
                SampleData.CityName,
                SampleData.CountryCode,
                SampleData.StreetCode,
                SampleData.FloorIdentification,
                SampleData.RoomIdentification,
                SampleData.IsWashable,
                SampleData.GsrnNumber,
                SampleData.TypeOfMeteringPoint,
                SampleData.SubTypeOfMeteringPoint,
                SampleData.ReadingOccurrence,
                0,
                0,
                SampleData.MeteringGridArea,
                SampleData.PowerPlantGsrnNumber,
                string.Empty,
                SampleData.SettlementMethod,
                SampleData.MeasurementUnitType,
                SampleData.DisconnectionType,
                SampleData.Occurrence,
                SampleData.MeterNumber,
                string.Empty,
                string.Empty,
                SampleData.NetSettlementGroup,
                SampleData.ConnectionType,
                SampleData.AssetType,
                string.Empty,
                ToGrid: string.Empty,
                ParentRelatedMeteringPoint: string.Empty,
                SampleData.ProductType);
        }

        private ConnectMeteringPoint CreateConnectMeteringPointRequest()
        {
            return new(SampleData.GsrnNumber, _dateTimeProvider.Now().ToString(), string.Empty);
        }
    }
}
