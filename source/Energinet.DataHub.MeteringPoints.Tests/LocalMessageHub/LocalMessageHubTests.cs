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
using Energinet.DataHub.MessageHub.Model.Dequeue;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Peek;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
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
        private readonly MeteringPointMessageDequeuedIntegrationEventOutboxDispatcherMock _messageDequeuedIntegrationEventOutboxDispatcher;
        private readonly ILocalMessageHubClient _localMessageHubClient;
        private readonly ILocalMessageHubDataAvailableClient _localMessageHubDataAvailableClient;
        private readonly DataBundleResponseOutboxDispatcherMock _dataBundleResponseOutboxDispatcher;
        private readonly RequestBundleParser _requestBundleParser;
        private readonly DequeueNotificationParser _dequeueNotificationParser;
        private readonly MessageHubMessageRepositoryMock _messageHubMessageRepository;
        private readonly DataAvailableNotificationOutboxDispatcherMock _dataAvailableNotificationOutboxDispatcher;

        public LocalMessageHubTests()
        {
            _messageDequeuedIntegrationEventOutboxDispatcher = new MeteringPointMessageDequeuedIntegrationEventOutboxDispatcherMock();
            _dataBundleResponseOutboxDispatcher = new DataBundleResponseOutboxDispatcherMock();
            _dataAvailableNotificationOutboxDispatcher = new DataAvailableNotificationOutboxDispatcherMock();
            _messageHubMessageRepository = new MessageHubMessageRepositoryMock();
            var dequeueNotificationParser = new DequeueNotificationParser();
            _requestBundleParser = new RequestBundleParser();
            _dequeueNotificationParser = new DequeueNotificationParser();
            _localMessageHubClient = new LocalMessageHubClient(
                new StorageHandlerMock(_messageHubMessageRepository),
                _messageHubMessageRepository,
                _messageDequeuedIntegrationEventOutboxDispatcher,
                _dataBundleResponseOutboxDispatcher,
                dequeueNotificationParser,
                _requestBundleParser,
                new BundleCreatorMock(),
                new SystemDateTimeProviderStub());

            _localMessageHubDataAvailableClient = new LocalMessageHubDataAvailableClient(
                _messageHubMessageRepository,
                _dataAvailableNotificationOutboxDispatcher,
                new MessageHubMessageFactory(new SystemDateTimeProviderStub()),
                new CorrelationContext());
        }

        [Fact]
        public void Dispatch_Should_Result_In_MessageReady_Notification()
        {
            var message = DispatchMessage();
            _messageHubMessageRepository.GetMessageAsync(message.Message.Id).Should().NotBeNull();
            _dataAvailableNotificationOutboxDispatcher.IsDispatched().Should().BeTrue();
        }

        [Fact]
        public async Task GenerateBundle_Should_Result_In_BundleReady_Notification()
        {
            var message = DispatchMessage();
            await RequestBundle(message.Message).ConfigureAwait(false);

            _dataBundleResponseOutboxDispatcher.IsDispatched().Should().BeTrue();
        }

        [Fact]
        public async Task BundleDequeued_Should_Result_In_Dispatched_Commands_For_Each_Message()
        {
            var messages = new List<(MessageHubMessage Message, string Correlation)>();

            for (var i = 0; i < 100; i++)
            {
                var message = DispatchMessage();
                messages.Add(message);
            }

            await RequestBundle(messages).ConfigureAwait(false);

            var bytes = _dequeueNotificationParser.Parse(new DequeueNotificationDto("foo", new GlobalLocationNumberDto("recipient")));

            await _localMessageHubClient.BundleDequeuedAsync(bytes).ConfigureAwait(false);

            foreach (var message in messages)
            {
                _messageDequeuedIntegrationEventOutboxDispatcher.IsDispatched(message.Correlation).Should().BeTrue();
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
            var requestBundleDto =
                new DataBundleRequestDto(
                    messageHubMessage.Id,
                    "foo",
                    "idempotencyId",
                    DocumentType.CreateMeteringPointAccepted.Name);

            var bytes = _requestBundleParser.Parse(requestBundleDto);

            await _localMessageHubClient.CreateBundleAsync(bytes, "sessionId").ConfigureAwait(false);
        }

        private (MessageHubMessage Message, string Correlation) DispatchMessage()
        {
            var correlationId = Guid.NewGuid().ToString();

            _localMessageHubDataAvailableClient.DataAvailable(new MessageHubEnvelope("recipient", "content", DocumentType.AccountingPointCharacteristicsMessage, correlationId, "gsrnNumber"));

            var message = _messageHubMessageRepository.GetMessageByCorrelation(correlationId);
            return (message, correlationId);
        }
    }
}
