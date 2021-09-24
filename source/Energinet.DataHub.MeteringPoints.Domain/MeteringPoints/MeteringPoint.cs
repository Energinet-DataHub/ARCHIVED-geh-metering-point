﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public abstract class MeteringPoint : AggregateRootBase
    {
        #pragma warning disable SA1401, CA1051 // Field cannot be private since it is set by derivatives
        protected MeteringPointType _meteringPointType;
        protected ProductType _productType;
        protected MeasurementUnitType _unitType;
#pragma warning restore
        private Address _address;
        private GridAreaId _gridAreaId;
        private MeteringPointSubType _meteringPointSubType;
        private ReadingOccurrence _meterReadingOccurrence;
        private PowerLimit _powerLimit;
        private GsrnNumber? _powerPlantGsrnNumber;
        private string? _locationDescription;
        private EffectiveDate _effectiveDate;
        private string? _meterNumber;

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        protected MeteringPoint() { }
#pragma warning restore 8618

        #pragma warning disable CS8618 //Disable nullable check
        protected MeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            // bool isAddressWashable,
            MeteringPointSubType meteringPointSubType,
            MeteringPointType meteringPointType,
            GridAreaId gridAreaId,
            GsrnNumber? powerPlantGsrnNumber,
            string? locationDescription,
            MeasurementUnitType unitType,
            string? meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            PowerLimit powerLimit,
            EffectiveDate effectiveDate)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            _address = address;
            // _isAddressWashable = isAddressWashable;
            _meteringPointSubType = meteringPointSubType;
            _meteringPointType = meteringPointType;
            _gridAreaId = gridAreaId;
            _powerPlantGsrnNumber = powerPlantGsrnNumber;
            _locationDescription = locationDescription;
            _unitType = unitType;
            _meterNumber = meterNumber;
            _meterReadingOccurrence = meterReadingOccurrence;
            _powerLimit = powerLimit;
            _effectiveDate = effectiveDate;
        }

        public MeteringPointId Id { get; }

        public GsrnNumber GsrnNumber { get; }

        protected ConnectionState ConnectionState { get; set; } = ConnectionState.New();

        public abstract BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails);

        public abstract void Connect(ConnectionDetails connectionDetails);
    }
}
