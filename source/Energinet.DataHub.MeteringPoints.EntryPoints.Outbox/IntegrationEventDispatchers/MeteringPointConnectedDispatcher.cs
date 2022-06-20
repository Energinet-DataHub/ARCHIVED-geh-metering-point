﻿// Copyright 2020 Energinet DataHub A/S
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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MeteringPoints.Application.Integrations;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.Connect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.IntegrationEventDispatchers
{
    public class MeteringPointConnectedDispatcher : IntegrationEventDispatcher<MeteringPointConnectedTopic, MeteringPointConnectedIntegrationEvent>
    {
        public MeteringPointConnectedDispatcher(ITopicSender<MeteringPointConnectedTopic> topicSender, ProtobufOutboundMapperFactory protobufOutboundMapperFactory, IIntegrationEventMessageFactory integrationEventMessageFactory, IIntegrationMetadataContext integrationMetadataContext)
            : base(topicSender, protobufOutboundMapperFactory, integrationEventMessageFactory, integrationMetadataContext)
        {
        }

        protected override void EnrichMessage(ServiceBusMessage serviceBusMessage)
        {
            serviceBusMessage.EnrichMetadata(
                nameof(MeteringPointConnected),
                1);
        }
    }
}
