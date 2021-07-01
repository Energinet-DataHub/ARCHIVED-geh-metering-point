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
using System.Net;
using System.Runtime.Remoting;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf;
using MeteringPointCreatedEventMessage = Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Messages.MeteringPointCreatedEventMessage;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Mappers
{
    public class CreateMeteringPointEventMessageMapper : ProtobufOutboundMapper<Messages.MeteringPointCreatedEventMessage>
    {
        protected override IMessage Convert(MeteringPointCreatedEventMessage obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return new IntegrationEventEnvelope()
            {
                MeteringPointCreatedMessage = new IntegrationEventContracts.MeteringPointCreated
                {
                    MeteringPointId = obj.Gsrn,
                    MeteringPointType = obj.MeteringPointType,
                    MeteringGridArea = obj.GridAreaId,
                    ToGrid = obj.ToGrid,
                    FromGrid = obj.FromGrid,
                    SettlementMethod = obj.SettlementMethod,
                    NetSettlementGroup = obj.NetSettlementGroup,
                    MeteringMethod = obj.MeteringMethod,
                    ConnectionState = obj.ConnectionState,
                    MeterReadingPeriodicity = obj.MeterReadingPeriodicity,
                    Product = obj.Product,
                    QuantityUnit = obj.QuantityUnit,
                    ParentMeteringPointId = obj.ParentGsrn,
                    EffectiveDate = obj.EffectiveDate,
                },
            };
        }
    }
}
