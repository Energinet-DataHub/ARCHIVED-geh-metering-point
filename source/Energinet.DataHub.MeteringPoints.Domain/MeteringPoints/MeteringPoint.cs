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
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public abstract class MeteringPoint
    {
        public MeteringPoint(
            GsrnNumber gsrnNumber,
            string streetName,
            string postCode,
            string cityName,
            string countryCode,
            bool isAddressWashable,
            string physicalStatusOfMeteringPoint,
            string meteringPointSubType,
            string typeOfMeteringPoint,
            string meteringGridArea,
            string powerPlant,
            string locationDescription,
            string productType,
            string parentRelatedMeteringPoint,
            string unitType,
            string meterNumber,
            Instant? meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate)
        {
            GsrnNumber = gsrnNumber;
            Id = Guid.NewGuid();
            StreetName = streetName;
            PostCode = postCode;
            CityName = cityName;
            CountryCode = countryCode;
            IsAddressWashable = isAddressWashable;
            PhysicalStatusOfMeteringPoint = physicalStatusOfMeteringPoint;
            MeteringPointSubType = meteringPointSubType;
            TypeOfMeteringPoint = typeOfMeteringPoint;
            MeteringGridArea = meteringGridArea;
            PowerPlant = powerPlant;
            LocationDescription = locationDescription;
            ProductType = productType;
            ParentRelatedMeteringPoint = parentRelatedMeteringPoint;
            UnitType = unitType;
            MeterNumber = meterNumber;
            MeterReadingOccurrence = meterReadingOccurrence;
            MaximumCurrent = maximumCurrent;
            MaximumPower = maximumPower;
            OccurenceDate = occurenceDate;
        }

        public MeteringPoint(
            GsrnNumber gsrnNumber,
            Guid id,
            string streetName,
            string postCode,
            string cityName,
            string countryCode,
            bool isAddressWashable,
            string physicalStatusOfMeteringPoint,
            string meteringPointSubType,
            string typeOfMeteringPoint,
            string meteringGridArea,
            string powerPlant,
            string locationDescription,
            string productType,
            string parentRelatedMeteringPoint,
            string unitType,
            string meterNumber,
            Instant? meterReadingOccurence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate)
            : this(
                gsrnNumber,
                streetName,
                postCode,
                cityName,
                countryCode,
                isAddressWashable,
                physicalStatusOfMeteringPoint,
                meteringPointSubType,
                typeOfMeteringPoint,
                meteringGridArea,
                powerPlant,
                locationDescription,
                productType,
                parentRelatedMeteringPoint,
                unitType,
                meterNumber,
                meterReadingOccurence,
                maximumCurrent,
                maximumPower,
                occurenceDate)
        {
            Id = id;
        }

        public GsrnNumber GsrnNumber { get; }

        public Guid Id { get; }

        public string StreetName { get; }

        public string PostCode { get; }

        public string CityName { get; }

        public string CountryCode { get; }

        public bool IsAddressWashable { get; }

        public string PhysicalStatusOfMeteringPoint { get; }

        public string MeteringPointSubType { get; }

        public Instant? MeterReadingOccurrence { get; }

        public string TypeOfMeteringPoint { get; }

        public int MaximumCurrent { get; }

        public int MaximumPower { get; }

        public string MeteringGridArea { get; }

        public string PowerPlant { get; }

        public string LocationDescription { get; }

        public string ProductType { get; }

        public string ParentRelatedMeteringPoint { get; }

        public string UnitType { get; }

        public Instant? OccurenceDate { get; }

        public string MeterNumber { get; }
    }
}
