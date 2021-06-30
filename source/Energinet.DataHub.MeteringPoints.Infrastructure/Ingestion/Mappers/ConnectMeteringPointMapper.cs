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
using System.Diagnostics.Contracts;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Google.Protobuf;
using ConnectMeteringPoint = Energinet.DataHub.MeteringPoints.Application.ConnectMeteringPoint;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Mappers
{
    public class ConnectMeteringPointMapper : ProtobufOutboundMapper<ConnectMeteringPoint>
    {
        protected override IMessage Convert(ConnectMeteringPoint obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return new MeteringPointEnvelope()
            {
                ConnectMeteringPoint = new Contracts.ConnectMeteringPoint
                {
                    GsrnNumber = obj.GsrnNumber,
                    EffectiveDate = obj.EffectiveDate,
                },
            };
        }
    }
}
