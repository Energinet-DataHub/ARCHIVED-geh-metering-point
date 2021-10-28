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
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
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
        private GridAreaLinkId _gridAreaLinkId;
        private MeteringMethod _meteringMethod;
        private ReadingOccurrence _meterReadingOccurrence;
        private PowerLimit _powerLimit;
        private GsrnNumber? _powerPlantGsrnNumber;
        private LocationDescription? _locationDescription;
        private EffectiveDate _effectiveDate;
        private MeterId? _meterNumber;
        private Capacity? _capacity;

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        protected MeteringPoint() { }
#pragma warning restore 8618

        #pragma warning disable CS8618 //Disable nullable check
        protected MeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            MeteringMethod meteringMethod,
            MeteringPointType meteringPointType,
            GridAreaLinkId gridAreaLinkId,
            GsrnNumber? powerPlantGsrnNumber,
            LocationDescription? locationDescription,
            MeasurementUnitType unitType,
            MeterId? meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            PowerLimit powerLimit,
            EffectiveDate effectiveDate,
            Capacity? capacity)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            Address = address;
            _meteringMethod = meteringMethod;
            _meteringPointType = meteringPointType;
            _gridAreaLinkId = gridAreaLinkId;
            _powerPlantGsrnNumber = powerPlantGsrnNumber;
            _locationDescription = locationDescription;
            _unitType = unitType;
            _meterNumber = meterNumber;
            _meterReadingOccurrence = meterReadingOccurrence;
            _powerLimit = powerLimit;
            _effectiveDate = effectiveDate;
            _capacity = capacity;
        }

        public MeteringPointId Id { get; }

        public GsrnNumber GsrnNumber { get; }

        protected ConnectionState ConnectionState { get; set; } = ConnectionState.New();

        protected Address Address { get; set; }

        public static BusinessRulesValidationResult CanCreate(MeteringPointDetails meteringPointDetails)
        {
            if (meteringPointDetails == null) throw new ArgumentNullException(nameof(meteringPointDetails));
            var rules = new List<IBusinessRule>()
            {
                new MeterIdRequirementRule(meteringPointDetails.MeterNumber, meteringPointDetails.MeteringMethod),
            };

            return new BusinessRulesValidationResult(rules);
        }

        public BusinessRulesValidationResult CanChangeAddress(Address address)
        {
            var rules = new List<IBusinessRule>()
            {
                new StreetNameIsRequiredRule(GsrnNumber, address),
                new PostCodeIsRequiredRule(address),
                new CityIsRequiredRule(address),
            };
            return new BusinessRulesValidationResult(rules);
        }

        public abstract BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails);

        public abstract void Connect(ConnectionDetails connectionDetails);

        public void ChangeAddress(Address newAddress)
        {
            if (newAddress == null) throw new ArgumentNullException(nameof(newAddress));
            if (newAddress.Equals(Address) == false)
            {
                Address = newAddress;
                AddDomainEvent(new AddressChanged(
                    Address.StreetName,
                    Address.PostCode,
                    Address.City,
                    Address.StreetCode,
                    Address.BuildingNumber,
                    Address.CitySubDivision,
                    Address.CountryCode?.Name,
                    Address.Floor,
                    Address.Room,
                    Address.MunicipalityCode.GetValueOrDefault(),
                    Address.IsActual,
                    Address.GeoInfoReference.GetValueOrDefault()));
            }
        }
    }
}
