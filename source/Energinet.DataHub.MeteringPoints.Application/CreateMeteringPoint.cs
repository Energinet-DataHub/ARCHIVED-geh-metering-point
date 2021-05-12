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

using Energinet.DataHub.MeteringPoints.Application.Transport;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application
{
    public class CreateMeteringPoint : IRequest<CreateMeteringPointResult>, IOutboundMessage, IInboundMessage
    {
        public string GsrnNumber { get; set; }

        public string TypeOfMeteringPoint { get; set; }

        public string SubTypeOfMeteringPoint { get; set; }

        public string MeterReadingOccurrence { get; set; }

        public int MaximumCurrent { get; set; }

        public int MaximumPower { get; set; }

        public string MeteringGridArea { get; set; }

        public string PowerPlant { get; set; }

        public string LocationDescription { get; set; }

        public string ProductType { get; set; }

        public Address InstallationLocationAddress { get; set; }

        public string ParentRelatedMeteringPoint { get; set; }

        public string SettlementMethod { get; set; }

        public string UnitType { get; set; }

        public string DisconnectionType { get; set; }

        public string OccurenceDate { get; set; }

        public string MeterNumber { get; set; }
    }
}
