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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.MeteringPoints.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using FluentAssertions;
using MediatR;
using SimpleInjector;
using Xunit.Sdk;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling
{
    public class AssertHelper
    {
        private readonly Container _container;

        public AssertHelper(
            Container container)
        {
            _container = container;
        }

        internal async Task ProcessOverviewAsync(
            string gsrn,
            string expectedProcessName,
            params string[] expectedProcessSteps)
        {
            var processes = await _container.GetInstance<IRequestHandler<MeteringPointProcessesByGsrnQuery, List<Process>>>()
                .Handle(new MeteringPointProcessesByGsrnQuery(gsrn), CancellationToken.None)
                .ConfigureAwait(false);

            processes.Should().ContainSingle(process => process.Name == expectedProcessName, $"a single process with name {expectedProcessName} was expected")
                .Which.Details.Select(detail => detail.Name).Should().ContainInOrder(expectedProcessSteps);
        }

        internal void ValidationError(string expectedErrorCode, bool expectError = true)
        {
            var message = GetOutboxMessages
                    <MessageHubEnvelope>()
                .SingleOrDefault(msg => msg.MessageType.Name.StartsWith("Reject", StringComparison.OrdinalIgnoreCase));

            if (message == null && expectError == false)
            {
                return;
            }

            if (message == null)
            {
                throw new XunitException("No message was found in outbox.");
            }

            var rejectMessage = _container.GetInstance<IJsonSerializer>().Deserialize<RejectMessage>(message!.Content);

            var errorCount = rejectMessage.MarketActivityRecord.Reasons.Count;
            if (errorCount > 1)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Reject message contains more ({errorCount}) than 1 error:");
                foreach (var error in rejectMessage.MarketActivityRecord.Reasons)
                {
                    errorMessage.AppendLine($"Code: {error.Code}. Description: {error.Text}.");
                }

                throw new XunitException(errorMessage.ToString());
            }

            var validationError = rejectMessage.MarketActivityRecord.Reasons
                .Single(error => error.Code == expectedErrorCode);

            if (expectError)
            {
                validationError.Should().NotBeNull();
            }
            else
            {
                validationError.Should().BeNull();
            }
        }

        internal void ValidationError(string expectedErrorCode, DocumentType type)
        {
            var message = GetOutboxMessages
                    <MessageHubEnvelope>()
                .Single(msg => msg.MessageType.Equals(type));

            var rejectMessage = _container.GetInstance<IJsonSerializer>().Deserialize<RejectMessage>(message.Content);

            var errorCount = rejectMessage.MarketActivityRecord.Reasons.Count;
            if (errorCount > 1)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Reject message contains more ({errorCount}) than 1 error:");
                foreach (var error in rejectMessage.MarketActivityRecord.Reasons)
                {
                    errorMessage.AppendLine($"Code: {error.Code}. Description: {error.Text}.");
                }

                throw new XunitException(errorMessage.ToString());
            }

            var validationError = rejectMessage.MarketActivityRecord.Reasons
                .Single(error => error.Code == expectedErrorCode);

            validationError.Should().NotBeNull();
        }

        internal void OutboxMessage<TMessage>(Func<TMessage, bool> funcAssert, int count = 1)
        {
            if (funcAssert == null)
                throw new ArgumentNullException(nameof(funcAssert));

            var messages = GetOutboxMessages<TMessage>()
                .Where(funcAssert.Invoke)
                .ToList();

            messages.Should().NotBeNull();
            messages.Should().AllBeOfType<TMessage>();
            messages.Should().HaveCount(count);
        }

        protected IEnumerable<TMessage> GetOutboxMessages<TMessage>()
        {
            var jsonSerializer = _container.GetInstance<IJsonSerializer>();
            var context = _container.GetInstance<MeteringPointContext>();
            return context.OutboxMessages
                .Where(message => message.Type == typeof(TMessage).FullName)
                .Select(message => jsonSerializer.Deserialize<TMessage>(message.Data));
        }
    }
}
