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
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.IntegrationEvents.Contracts;
using Google.Protobuf;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint
{
    public class MeteringPointCreatedEventMessageMapper : ProtobufOutboundMapper<MeteringPointCreatedEventMessage>
    {
        protected override IMessage Convert(MeteringPointCreatedEventMessage obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return new MeteringPointCreated
            {
                MeteringPointId = obj.MeteringPointId,
                GsrnNumber = obj.GsrnNumber,
                MeteringPointType = obj.GetMeteringPointType(),
                GridAreaCode = obj.GridAreaId,
                ToGridAreaCode = obj.ToGrid,
                FromGridAreaCode = obj.FromGrid,
                SettlementMethod = obj.GetSettlementMethod(),
                NetSettlementGroup = obj.GetNetSettlementGroup(),
                MeteringMethod = obj.GetMeteringMethod(),
                ConnectionState = obj.GetConnectionState(),
                MeterReadingPeriodicity = obj.GetMeterReadingPeriodicity(),
                Product = obj.GetProductType(),
                UnitType = obj.GetUnitType(),
                EffectiveDate = obj.EffectiveDate.ToTimestamp(),
                ParentGsrnNumber = obj.ParentGsrn,
            };
        }
    }
}
