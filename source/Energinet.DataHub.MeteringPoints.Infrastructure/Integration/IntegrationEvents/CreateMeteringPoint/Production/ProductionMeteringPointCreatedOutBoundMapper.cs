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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Google.Protobuf;
using NodaTime;
using NodaTime.Text;
using ProductionMeteringPointCreated = Energinet.DataHub.MeteringPoints.IntegrationEventContracts.ProductionMeteringPointCreated;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Production
{
    public class ProductionMeteringPointCreatedOutBoundMapper : ProtobufOutboundMapper<ProductionMeteringPointCreatedIntegrationEvent>
    {
        protected override IMessage Convert(ProductionMeteringPointCreatedIntegrationEvent obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return new ProductionMeteringPointCreated
            {
                MeteringPointId = obj.MeteringPointId,
                GsrnNumber = obj.GsrnNumber,
                GridAreaCode = obj.GridAreaCode,
                MeteringMethod = obj.GetMeteringMethod(),
                MeterReadingPeriodicity = obj.GetMeterReadingPeriodicity(),
                NetSettlementGroup = obj.GetNetSettlementGroup(),
                Product = obj.GetProductType(),
                ConnectionState = obj.GetConnectionState(),
                UnitType = obj.GetUnitType(),
                EffectiveDate = obj.EffectiveDate.ToTimestamp(),
            };
        }
    }
}
