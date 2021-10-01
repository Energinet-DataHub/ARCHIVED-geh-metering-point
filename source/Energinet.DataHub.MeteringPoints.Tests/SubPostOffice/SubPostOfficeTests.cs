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
using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.PostOffice;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice;
using FluentAssertions;
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using GreenEnergyHub.PostOffice.Communicator.Model;
using GreenEnergyHub.PostOffice.Communicator.Peek;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.SubPostOffice
{
    [UnitTest]
    public class SubPostOfficeTests
    {
        private readonly DummyDispatcher _messageDispatcher;
        private readonly ISubPostOfficeClient _subPostOfficeClient;
        private readonly DummyDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly DummyDataBundleResponseSender _dataBundleResponseSender;
        private readonly DummyDequeueNotificationSender _dequeueNotificationSender;
        private readonly RequestBundleParser _requestBundleParser;
        private readonly DequeueNotificationParser _dequeueNotificationParser;

        private readonly List<PostOfficeMessageMetadata> _ids;

        public SubPostOfficeTests()
        {
            _messageDispatcher = new DummyDispatcher();
            _ids = new List<PostOfficeMessageMetadata> { new("correlationId1") };
            var postOfficeMessageMetadataRepository = new DummyPostOfficeMessageMetadataRepository(_ids);
            var subPostOfficeStorageClient = new DummySubPostOfficeStorageClient();
            var postOfficeStorageClient = new DummyPostOfficeStorageClient();
            _dataBundleResponseSender = new DummyDataBundleResponseSender();
            var dequeueNotificationParser = new DequeueNotificationParser();
            _requestBundleParser = new RequestBundleParser();
            _dequeueNotificationParser = new DequeueNotificationParser();
            _dataAvailableNotificationSender = new DummyDataAvailableNotificationSender();
            _subPostOfficeClient = new SubPostOfficeClient(
                subPostOfficeStorageClient,
                postOfficeStorageClient,
                postOfficeMessageMetadataRepository,
                _messageDispatcher,
                _dataAvailableNotificationSender,
                _dataBundleResponseSender,
                dequeueNotificationParser,
                _requestBundleParser);
            _dequeueNotificationSender = new DummyDequeueNotificationSender();
        }

        [Fact]
        public async Task Dispatch_Should_Result_In_MessageReady_Notification()
        {
            await _subPostOfficeClient.DispatchAsync(new PostOfficeMessageEnvelope("recipient", "content", "messagetype", "correlationId1")).ConfigureAwait(false);

            // should we test that messages are stored correctly?
            _dataAvailableNotificationSender.IsSent().Should().Be(true);
        }

        [Fact]
        public async Task GenerateBundle_Should_Result_In_BundleReady_Notification()
        {
            var requestBundleDto = new DataBundleRequestDto("idempotencyId", new[] { Guid.NewGuid() });

            var bytes = _requestBundleParser.Parse(requestBundleDto);

            await _subPostOfficeClient.CreateBundleAsync(bytes).ConfigureAwait(false);

            // test that bundle is stored correctly
            _dataBundleResponseSender.IsSent().Should().Be(true);
        }

        [Fact]
        public async Task BundleDequeued_Should_Result_In_Dispatched_Commands_For_Each_Message()
        {
            var bytes = _dequeueNotificationParser.Parse(new DequeueNotificationDto(new List<Guid> { Guid.NewGuid() }, new GlobalLocationNumberDto("recipient")));

            await _subPostOfficeClient.BundleDequeuedAsync(bytes).ConfigureAwait(false);

            _messageDispatcher.IsDispatched().Should().Be(true);
        }
    }
}
