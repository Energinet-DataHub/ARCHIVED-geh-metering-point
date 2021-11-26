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

using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Extensions
{
    public static class WhenProviderExtensions
    {
        public static DoProvider WhenMessageType(this ServiceBusListenerMock provider, string messageType)
        {
            var parser = new MessageHub.Model.DataAvailable.DataAvailableNotificationParser();

            return provider.When(message =>
                parser.Parse(message.Body.ToArray()).MessageType.Value == messageType);
        }
    }
}
