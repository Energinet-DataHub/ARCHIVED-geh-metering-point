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
using Energinet.DataHub.MessageHub.Client.Dequeue;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub;
using Energinet.DataHub.MeteringPoints.Messaging;
using Energinet.DataHub.MeteringPoints.Tests.LocalMessageHub.Mocks;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.LocalMessageHub
{
    [UnitTest]
    public class LocalMessageHubTests
    {
        private readonly MeteringPointIntegrationEventHandlerMock _integrationEventHandler;
        private readonly ILocalMessageHubClient _localMessageHubClient;
        private readonly ILocalMessageHubDataAvailableClient _localMessageHubDataAvailableClient;
        private readonly DataAvailableNotificationSenderMock _dataAvailableNotificationSender;
        private readonly DataBundleResponseSenderMock _dataBundleResponseSender;
        private readonly RequestBundleParser _requestBundleParser;
        private readonly DequeueNotificationParser _dequeueNotificationParser;

        private readonly MessageHubMessageRepositoryMock _messageHubMessageRepository;

        public LocalMessageHubTests()
        {
            _integrationEventHandler = new MeteringPointIntegrationEventHandlerMock();
            _messageHubMessageRepository = new MessageHubMessageRepositoryMock();
            _dataBundleResponseSender = new DataBundleResponseSenderMock();
            var dequeueNotificationParser = new DequeueNotificationParser();
            _requestBundleParser = new RequestBundleParser();
            _dequeueNotificationParser = new DequeueNotificationParser();
            _dataAvailableNotificationSender = new DataAvailableNotificationSenderMock();
            _localMessageHubClient = new LocalMessageHubClient(
                new StorageHandlerMock(),
                _messageHubMessageRepository,
                _integrationEventHandler,
                _dataBundleResponseSender,
                dequeueNotificationParser,
                _requestBundleParser,
                new BundleCreatorMock(),
                new SystemDateTimeProviderStub());

            _localMessageHubDataAvailableClient = new LocalMessageHubDataAvailableClient(
                _messageHubMessageRepository,
                _dataAvailableNotificationSender,
                new MessageHubMessageFactory(new SystemDateTimeProviderStub()));
        }

        [Fact]
        public async Task Dispatch_Should_Result_In_MessageReady_Notification()
        {
            var message = await DispatchMessage().ConfigureAwait(false);
            _messageHubMessageRepository.GetMessageAsync(message.Message.Id).Should().NotBeNull();
            _dataAvailableNotificationSender.IsSent().Should().BeTrue();
        }

        [Fact]
        public async Task GenerateBundle_Should_Result_In_BundleReady_Notification()
        {
            var message = await DispatchMessage().ConfigureAwait(false);
            await RequestBundle(message.Message).ConfigureAwait(false);

            _dataBundleResponseSender.IsSent().Should().BeTrue();
        }

        [Fact]
        public async Task BundleDequeued_Should_Result_In_Dispatched_Commands_For_Each_Message()
        {
            var messages = new List<(MessageHubMessage Message, string Correlation)>();

            for (var i = 0; i < 100; i++)
            {
                var message = await DispatchMessage().ConfigureAwait(false);
                messages.Add(message);
            }

            await RequestBundle(messages).ConfigureAwait(false);

            var bytes = _dequeueNotificationParser.Parse(new DequeueNotificationDto(messages.Select(x => x.Message.Id).ToArray(), new GlobalLocationNumberDto("recipient")));

            await _localMessageHubClient.BundleDequeuedAsync(bytes).ConfigureAwait(false);

            foreach (var message in messages)
            {
                _integrationEventHandler.IsDispatched(message.Correlation).Should().BeTrue();
            }
        }

        private async Task RequestBundle(List<(MessageHubMessage Message, string Correlation)> messages)
        {
            foreach (var message in messages)
            {
                await RequestBundle(message.Message).ConfigureAwait(false);
            }
        }

        private async Task RequestBundle(MessageHubMessage messageHubMessage)
        {
            var requestBundleDto = new DataBundleRequestDto("idempotencyId", new[] { messageHubMessage!.Id });

            var bytes = _requestBundleParser.Parse(requestBundleDto);

            await _localMessageHubClient.CreateBundleAsync(bytes, "sessionId").ConfigureAwait(false);
        }

        private async Task<(MessageHubMessage Message, string Correlation)> DispatchMessage()
        {
            var correlationId = Guid.NewGuid().ToString();

            await _localMessageHubDataAvailableClient.DataAvailableAsync(new MessageHubEnvelope("recipient", "content", DocumentType.AccountingPointCharacteristicsMessage, correlationId)).ConfigureAwait(false);

            var message = _messageHubMessageRepository.GetMessageByCorrelation(correlationId);
            return (message, correlationId);
        }
    }
}
