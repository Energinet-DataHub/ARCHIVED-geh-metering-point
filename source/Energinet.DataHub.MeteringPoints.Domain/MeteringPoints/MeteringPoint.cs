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

using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public abstract class MeteringPoint : AggregateRootBase
    {
        protected MeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            string streetName,
            string postCode,
            string cityName,
            string countryCode,
            bool isAddressWashable,
            PhysicalState physicalState,
            MeteringPointSubType meteringPointSubType,
            MeteringPointType meteringPointType,
            GridAreaId gridAreaId,
            string powerPlant,
            string locationDescription,
            string productType,
            string parentRelatedMeteringPoint,
            string unitType,
            string meterNumber,
            string meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            StreetName = streetName;
            PostCode = postCode;
            CityName = cityName;
            CountryCode = countryCode;
            IsAddressWashable = isAddressWashable;
            PhysicalState = physicalState;
            MeteringPointSubType = meteringPointSubType;
            MeteringPointType = meteringPointType;
            GridAreaId = gridAreaId;
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

            AddDomainEvent(new MeteringPointCreated(id, GsrnNumber));
        }

        public GridAreaId GridAreaId { get; }

        public MeteringPointType MeteringPointType { get; }

        public MeteringPointSubType MeteringPointSubType { get; }

        public PhysicalState PhysicalState { get; }

        public GsrnNumber GsrnNumber { get; }

        public MeteringPointId Id { get; }

        public string StreetName { get; }

        public string PostCode { get; }

        public string CityName { get; }

        public string CountryCode { get; }

        public bool IsAddressWashable { get; }

        public string MeterReadingOccurrence { get; }

        public int MaximumCurrent { get; }

        public int MaximumPower { get; }

        public string PowerPlant { get; }

        public string LocationDescription { get; }

        public string ProductType { get; }

        public string ParentRelatedMeteringPoint { get; }

        public string UnitType { get; }

        public Instant? OccurenceDate { get; }

        public string MeterNumber { get; }
    }
}
