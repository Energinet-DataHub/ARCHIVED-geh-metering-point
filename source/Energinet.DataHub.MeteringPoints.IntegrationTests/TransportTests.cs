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
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Send;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Send_and_receive_must_result_in_same_transmitted_values()
        {
            var expectedGsrnNumber = "123";

            // Send
            var sendingServiceCollection = new ServiceCollection();
            sendingServiceCollection.AddSingleton<InProcessChannel>();
            sendingServiceCollection.AddScoped<Dispatcher>();
            sendingServiceCollection.SendProtobuf<MeteringPointEnvelope>();
            var sendingServiceProvider = sendingServiceCollection.BuildServiceProvider();

            var messageDispatcher = sendingServiceProvider.GetRequiredService<Dispatcher>();
            var outboundMessage = new Application.CreateMeteringPoint
            {
                GsrnNumber = expectedGsrnNumber,
            };
            await messageDispatcher.DispatchAsync(outboundMessage).ConfigureAwait(false);
            var channel = sendingServiceProvider.GetRequiredService<InProcessChannel>();

            // The wire
            var bytes = channel.GetWrittenBytes();

            // Receive
            var receivingServiceCollection = new ServiceCollection();
            receivingServiceCollection.ReceiveProtobuf<MeteringPointEnvelope>(
                config => config
                    .FromOneOf(envelope => envelope.MeteringPointMessagesCase)
                    .WithParser(() => MeteringPointEnvelope.Parser));

            var receivingServiceProvider = receivingServiceCollection.BuildServiceProvider();
            var messageExtractor = receivingServiceProvider.GetRequiredService<MessageExtractor>();

            var message = await messageExtractor.ExtractAsync(bytes).ConfigureAwait(false);

            message.Should().BeOfType<Application.CreateMeteringPoint>();
        }
    }
}
