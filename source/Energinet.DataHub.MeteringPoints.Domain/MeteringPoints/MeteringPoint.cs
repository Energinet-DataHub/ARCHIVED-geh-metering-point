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
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    //TODO:REMEMBER REMOVE THIS WHEN VALUE OBJECTS ARE IMPLEMENTED
    #pragma warning disable
    public class MeteringPoint : AggregateRootBase
    {
        //TODO: Implement relevant value objects
        private readonly GridAreaId _gridAreaId;
        // private MeteringPointType _typeOfMeteringPoint;
        private readonly MeteringPointSubType _subTypeOfMeteringPoint;
        private PhysicalState _physicalState;

        private string _meterReadingOccurrence;
        private int _maximumCurrent;
        private int _maximumPower;
        private string _meteringGridArea;
        private string _powerPlant;
        private string _locationDescription;
        private string _productType;
        private string _parentRelatedMeteringPoint;
        private string _settlementMethod;
        private string _unitType;
        private string _disconnectionType;
        private string _occurenceDate;
        private string _meterNumber;

        public MeteringPoint(MeteringPointId meteringPointId, GsrnNumber gsrnNumber, GridAreaId gridAreaId, MeteringPointType typeOfMeteringPoint, MeteringPointSubType subTypeOfMeteringPoint)
        {
            Id = meteringPointId ?? throw new ArgumentNullException(nameof(meteringPointId));
            GsrnNumber = gsrnNumber ?? throw new ArgumentNullException(nameof(gsrnNumber));
            _physicalState = PhysicalState.New;
            _gridAreaId = gridAreaId ?? throw new ArgumentNullException(nameof(gridAreaId));
            TypeOfMeteringPoint = typeOfMeteringPoint ?? throw new ArgumentNullException(nameof(typeOfMeteringPoint));
            _subTypeOfMeteringPoint = subTypeOfMeteringPoint ?? throw new ArgumentNullException(nameof(subTypeOfMeteringPoint));
            AddDomainEvent(new MeteringPointCreated(meteringPointId, GsrnNumber));
        }

        public MeteringPointId Id { get; }

        public GsrnNumber GsrnNumber { get; }

        public MeteringPointType TypeOfMeteringPoint { get; }
    }
}
