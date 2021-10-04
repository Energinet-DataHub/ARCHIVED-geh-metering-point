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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice;
using Energinet.DataHub.MeteringPoints.Tests.SubPostOffice.Mocks;
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
        private readonly DispatcherMock _messageDispatcher;
        private readonly ISubPostOfficeClient _subPostOfficeClient;
        private readonly DataAvailableNotificationSenderMock _dataAvailableNotificationSender;
        private readonly DataBundleResponseSenderMock _dataBundleResponseSender;
        private readonly RequestBundleParser _requestBundleParser;
        private readonly DequeueNotificationParser _dequeueNotificationParser;

        private readonly PostOfficeMessageMetadataRepositoryMock _postOfficeMessageMetadataRepository;
        private readonly SubPostOfficeStorageClientMock _subPostOfficeStorageClient;

        public SubPostOfficeTests()
        {
            _messageDispatcher = new DispatcherMock();
            _postOfficeMessageMetadataRepository = new PostOfficeMessageMetadataRepositoryMock();
            _subPostOfficeStorageClient = new SubPostOfficeStorageClientMock();
            PostOfficeStorageClientMock postOfficeStorageClient = new();
            _dataBundleResponseSender = new DataBundleResponseSenderMock();
            var dequeueNotificationParser = new DequeueNotificationParser();
            _requestBundleParser = new RequestBundleParser();
            _dequeueNotificationParser = new DequeueNotificationParser();
            _dataAvailableNotificationSender = new DataAvailableNotificationSenderMock();
            _subPostOfficeClient = new SubPostOfficeClient(
                _subPostOfficeStorageClient,
                postOfficeStorageClient,
                _postOfficeMessageMetadataRepository,
                _messageDispatcher,
                _dataAvailableNotificationSender,
                _dataBundleResponseSender,
                dequeueNotificationParser,
                _requestBundleParser);
        }

        [Fact]
        public async Task Dispatch_Should_Result_In_MessageReady_Notification()
        {
            var message = await DispatchMessage().ConfigureAwait(false);
            _subPostOfficeStorageClient.Exists(message.PostOfficeMessageMetadata.BlobName).Should().BeTrue();
            _dataAvailableNotificationSender.IsSent().Should().BeTrue();
        }

        [Fact]
        public async Task GenerateBundle_Should_Result_In_BundleReady_Notification()
        {
            var message = await DispatchMessage().ConfigureAwait(false);
            await RequestBundle(message.PostOfficeMessageMetadata).ConfigureAwait(false);

            _dataBundleResponseSender.IsSent().Should().BeTrue();
        }

        [Fact]
        public async Task BundleDequeued_Should_Result_In_Dispatched_Commands_For_Each_Message()
        {
            var messages = new List<(PostOfficeMessageMetadata PostOfficeMessageMetadata, string Correlation)>();

            for (var i = 0; i < 100; i++)
            {
                var message = await DispatchMessage().ConfigureAwait(false);
                messages.Add(message);
                await RequestBundle(message!.PostOfficeMessageMetadata).ConfigureAwait(false);
            }

            var bytes = _dequeueNotificationParser.Parse(new DequeueNotificationDto(messages.Select(x => x.PostOfficeMessageMetadata.Id).ToArray(), new GlobalLocationNumberDto("recipient")));

            await _subPostOfficeClient.BundleDequeuedAsync(bytes).ConfigureAwait(false);

            foreach (var message in messages)
            {
                _messageDispatcher.IsDispatched(message.Correlation).Should().BeTrue();
            }
        }

        private async Task RequestBundle(PostOfficeMessageMetadata? message)
        {
            var requestBundleDto = new DataBundleRequestDto("idempotencyId", new[] { message!.Id });

            var bytes = _requestBundleParser.Parse(requestBundleDto);

            await _subPostOfficeClient.CreateBundleAsync(bytes).ConfigureAwait(false);
        }

        private async Task<(PostOfficeMessageMetadata PostOfficeMessageMetadata, string Correlation)> DispatchMessage()
        {
            var correlationId = Guid.NewGuid().ToString();

            await _subPostOfficeClient.DispatchAsync(new PostOfficeMessageEnvelope("recipient", "content", "messagetype", correlationId)).ConfigureAwait(false);

            var message = _postOfficeMessageMetadataRepository.GetMessageByCorrelation(correlationId);
            return (message, correlationId);
        }
    }
}
