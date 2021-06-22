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
    public class MeteringPointCreated : DomainEventBase
    {
        public MeteringPointCreated(MeteringPointId meteringPointId, GsrnNumber gsrnNumber, MeteringPointType meteringPointType, GridAreaId gridAreaId, MeteringPointSubType meteringPointSubType, PhysicalState physicalState, ReadingOccurrence readingOccurrence, ProductType productType, MeasurementUnitType measurementUnitType)
        {
            MeteringPointId = meteringPointId;
            GsrnNumber = gsrnNumber;
            MeteringPointType = meteringPointType;
            GridAreaId = gridAreaId;
            MeteringPointSubType = meteringPointSubType;
            PhysicalState = physicalState;
            ReadingOccurrence = readingOccurrence;
            ProductType = productType;
            MeasurementUnitType = measurementUnitType;
        }

        public MeteringPointId MeteringPointId { get; }

        public PhysicalState PhysicalState { get; }

        public ReadingOccurrence ReadingOccurrence { get; }

        public ProductType ProductType { get; }

        public MeasurementUnitType MeasurementUnitType { get; }

        public MeteringPointSubType MeteringPointSubType { get; }

        public GsrnNumber GsrnNumber { get; }

        public MeteringPointType MeteringPointType { get; }

        public GridAreaId GridAreaId { get; }
    }
}
