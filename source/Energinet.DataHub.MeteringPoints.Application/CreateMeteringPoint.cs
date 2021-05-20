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

using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Transport;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application
{
    #nullable disable // TODO: Once we re-visit the model, consider this disable again.
    public class CreateMeteringPoint : IRequest<CreateMeteringPointResult>, IBusinessRequest, IOutboundMessage, IInboundMessage
    {
        public CreateMeteringPoint() { }

        public CreateMeteringPoint(
            string gsrnNumber,
            string typeOfMeteringPoint,
            string subTypeOfMeteringPoint,
            string meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            string meteringGridArea,
            string powerPlant,
            string locationDescription,
            string productType,
            Address installationLocationAddress,
            string parentRelatedMeteringPoint,
            string settlementMethod,
            string unitType,
            string disconnectionType,
            string occurenceDate,
            string meterNumber)
        {
            GsrnNumber = gsrnNumber;
            TypeOfMeteringPoint = typeOfMeteringPoint;
            SubTypeOfMeteringPoint = subTypeOfMeteringPoint;
            MeterReadingOccurrence = meterReadingOccurrence;
            MaximumCurrent = maximumCurrent;
            MaximumPower = maximumPower;
            MeteringGridArea = meteringGridArea;
            PowerPlant = powerPlant;
            LocationDescription = locationDescription;
            ProductType = productType;
            InstallationLocationAddress = installationLocationAddress;
            ParentRelatedMeteringPoint = parentRelatedMeteringPoint;
            SettlementMethod = settlementMethod;
            UnitType = unitType;
            DisconnectionType = disconnectionType;
            OccurenceDate = occurenceDate;
            MeterNumber = meterNumber;
        }

        public string GsrnNumber { get; set; }

        public string TypeOfMeteringPoint { get; }

        public string SubTypeOfMeteringPoint { get; }

        public string MeterReadingOccurrence { get; }

        public int MaximumCurrent { get; }

        public int MaximumPower { get; }

        public string MeteringGridArea { get; }

        public string PowerPlant { get; }

        public string LocationDescription { get; }

        public string ProductType { get; set; }

        public Address InstallationLocationAddress { get; }

        public string ParentRelatedMeteringPoint { get; }

        public string SettlementMethod { get; }

        public string UnitType { get; }

        public string DisconnectionType { get; }

        public string OccurenceDate { get; }

        public string MeterNumber { get; }

        public string TransactionId { get; }
    }
    #nullable restore
}
