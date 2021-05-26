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
        #pragma warning disable CS8618 // Disable non-nullable check
        protected MeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            bool isAddressWashable,
            PhysicalState physicalState,
            MeteringPointSubType meteringPointSubType,
            MeteringPointType meteringPointType,
            GridAreaId gridAreaId,
            GsrnNumber powerPlant,
            string locationDescription,
            string parentRelatedMeteringPoint,
            MeasurementUnitType unitType,
            string meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            IsAddressWashable = isAddressWashable;
            PhysicalState = physicalState;
            MeteringPointSubType = meteringPointSubType;
            MeteringPointType = meteringPointType;
            GridAreaId = gridAreaId;
            PowerPlant = powerPlant;
            LocationDescription = locationDescription;
            ParentRelatedMeteringPoint = parentRelatedMeteringPoint;
            UnitType = unitType;
            MeterNumber = meterNumber;
            MeterReadingOccurrence = meterReadingOccurrence;
            MaximumCurrent = maximumCurrent;
            MaximumPower = maximumPower;
            OccurenceDate = occurenceDate;
        }

        protected MeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            bool isAddressWashable,
            PhysicalState physicalState,
            MeteringPointSubType meteringPointSubType,
            MeteringPointType meteringPointType,
            GridAreaId gridAreaId,
            GsrnNumber powerPlant,
            string locationDescription,
            string parentRelatedMeteringPoint,
            MeasurementUnitType unitType,
            string meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            Address = address;
            IsAddressWashable = isAddressWashable;
            PhysicalState = physicalState;
            MeteringPointSubType = meteringPointSubType;
            MeteringPointType = meteringPointType;
            GridAreaId = gridAreaId;
            PowerPlant = powerPlant;
            LocationDescription = locationDescription;
            ParentRelatedMeteringPoint = parentRelatedMeteringPoint;
            UnitType = unitType;
            MeterNumber = meterNumber;
            MeterReadingOccurrence = meterReadingOccurrence;
            MaximumCurrent = maximumCurrent;
            MaximumPower = maximumPower;
            OccurenceDate = occurenceDate;

            AddDomainEvent(new MeteringPointCreated(id, GsrnNumber, meteringPointType, gridAreaId));
        }

        public GridAreaId GridAreaId { get; }

        public MeteringPointType MeteringPointType { get; }

        public MeteringPointSubType MeteringPointSubType { get; }

        public PhysicalState PhysicalState { get; }

        public GsrnNumber GsrnNumber { get; }

        public MeteringPointId Id { get; }

        public bool IsAddressWashable { get; }

        public ReadingOccurrence MeterReadingOccurrence { get; }

        public int MaximumCurrent { get; }

        public int MaximumPower { get; }

        public GsrnNumber PowerPlant { get; }

        public string LocationDescription { get; }

        public ProductType ProductType { get; protected set; }

        public string ParentRelatedMeteringPoint { get; }

        public MeasurementUnitType UnitType { get; }

        public Instant? OccurenceDate { get; }

        public string MeterNumber { get; }

        public Address Address { get; }
    }
}
