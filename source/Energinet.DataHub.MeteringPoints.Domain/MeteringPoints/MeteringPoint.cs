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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
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
        private ReadingOccurrence _meterReadingOccurrence;
        private PowerLimit _powerLimit;
        private GsrnNumber? _powerPlantGsrnNumber;
        private EffectiveDate _effectiveDate;
        private Capacity? _capacity;

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        protected MeteringPoint() { }
#pragma warning restore 8618

        #pragma warning disable CS8618 //Disable nullable check
        protected MeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            MeteringPointType meteringPointType,
            GridAreaLinkId gridAreaLinkId,
            GsrnNumber? powerPlantGsrnNumber,
            MeasurementUnitType unitType,
            ReadingOccurrence meterReadingOccurrence,
            PowerLimit powerLimit,
            EffectiveDate effectiveDate,
            Capacity? capacity,
            MeteringConfiguration meteringConfiguration)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            Address = address;
            _meteringPointType = meteringPointType;
            _gridAreaLinkId = gridAreaLinkId;
            _powerPlantGsrnNumber = powerPlantGsrnNumber;
            _unitType = unitType;
            _meterReadingOccurrence = meterReadingOccurrence;
            _powerLimit = powerLimit;
            _effectiveDate = effectiveDate;
            _capacity = capacity;

            MeteringConfiguration = meteringConfiguration;
        }

        public MeteringPointId Id { get; }

        public GsrnNumber GsrnNumber { get; }

        public Address Address { get; private set; }

        internal MeteringConfiguration MeteringConfiguration { get; private set; }

        protected ConnectionState ConnectionState { get; set; } = ConnectionState.New();

        public static BusinessRulesValidationResult CanCreate(MeteringPointDetails meteringPointDetails)
        {
            if (meteringPointDetails == null) throw new ArgumentNullException(nameof(meteringPointDetails));
            var rules = new List<IBusinessRule>()
            {
                //new MeterIdRequirementRule(meteringPointDetails.MeterNumber, meteringPointDetails.MeteringMethod),
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
            var checkResult = CanChangeAddress(newAddress);
            if (checkResult.Success == false)
            {
                throw new MasterDataChangeException(checkResult.Errors.ToList());
            }

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
                    Address.IsActual.GetValueOrDefault(),
                    Address.GeoInfoReference.GetValueOrDefault()));
            }
        }

        public void ChangeMeteringConfiguration(MeteringConfiguration configuration, EffectiveDate effectiveDate)
        {
            if (effectiveDate == null) throw new ArgumentNullException(nameof(effectiveDate));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (MeteringConfiguration.Equals(configuration))
            {
                return;
            }

            MeteringConfiguration = configuration;

            AddDomainEvent(new MeteringConfigurationChanged(
                Id.Value.ToString(),
                GsrnNumber.Value,
                MeteringConfiguration.Meter.Value,
                MeteringConfiguration.Method.Name,
                effectiveDate.ToString()));
        }

        #pragma warning disable CA1024
        public MeteringConfiguration GetMeteringConfiguration()
        {
            return MeteringConfiguration;
        }
        #pragma warning restore
    }
}
