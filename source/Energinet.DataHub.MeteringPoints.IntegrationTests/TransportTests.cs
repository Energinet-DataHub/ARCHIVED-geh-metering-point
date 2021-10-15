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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Send;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using MasterDataDocument = Energinet.DataHub.MeteringPoints.Application.MarketDocuments.MasterDataDocument;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public class TransportTests
    {
        [Fact]
        public async Task Send_and_receive_must_result_in_same_transmitted_values()
        {
            const string? expectedGsrnNumber = "123";
            byte[]? bytes;

            // Send setup
            await using var sendingContainer = new Container();
            sendingContainer.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            sendingContainer.Register<InProcessChannel>(Lifestyle.Singleton);
            sendingContainer.Register<Dispatcher>(Lifestyle.Transient);
            sendingContainer.SendProtobuf<MeteringPointEnvelope>();
            sendingContainer.Verify();

            // Send scope
            await using (AsyncScopedLifestyle.BeginScope(sendingContainer))
            {
                var messageDispatcher = sendingContainer.GetRequiredService<Dispatcher>();
                var outboundMessage = new MasterDataDocument
                {
                    GsrnNumber = expectedGsrnNumber,
                };
                await messageDispatcher.DispatchAsync(outboundMessage).ConfigureAwait(false);
                var channel = sendingContainer.GetRequiredService<InProcessChannel>();

                // The wire
                bytes = channel.GetWrittenBytes();
            }

            // Receive setup
            await using var receivingContainer = new Container();
            receivingContainer.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            receivingContainer.ReceiveProtobuf<MeteringPointEnvelope>(
                config => config
                    .FromOneOf(envelope => envelope.MeteringPointMessagesCase)
                    .WithParser(() => MeteringPointEnvelope.Parser));
            receivingContainer.Verify();

            // Receive scope
            await using var scope = AsyncScopedLifestyle.BeginScope(receivingContainer);
            var messageExtractor = receivingContainer.GetRequiredService<MessageExtractor>();
            var message = await messageExtractor.ExtractAsync(bytes).ConfigureAwait(false);
            message.Should().BeOfType<MasterDataDocument>();
        }
    }
}
