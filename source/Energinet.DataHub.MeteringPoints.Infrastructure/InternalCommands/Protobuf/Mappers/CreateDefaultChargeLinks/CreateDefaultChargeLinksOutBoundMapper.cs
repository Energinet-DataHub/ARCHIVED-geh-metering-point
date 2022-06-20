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
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.RequestResponse.Contract;
using Google.Protobuf;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands.Protobuf.Mappers.CreateDefaultChargeLinks
{
    public class CreateDefaultChargeLinksOutBoundMapper : ProtobufOutboundMapper<Application.Integrations.ChargeLinks.Create.CreateDefaultChargeLinks>
    {
        protected override IMessage Convert(Application.Integrations.ChargeLinks.Create.CreateDefaultChargeLinks obj)
        {
            if (obj == null) throw new ArgumentException(null, nameof(obj));

            return new MeteringPointEnvelope
            {
                CreateDefaultChargeLinks = new RequestResponse.Contract.CreateDefaultChargeLinks()
                {
                    CorrelationId = obj.CorrelationId,
                    GsrnNumber = obj.GsrnNumber,
                },
            };
        }
    }
}
