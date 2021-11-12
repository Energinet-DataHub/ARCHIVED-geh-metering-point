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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Special.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Special
{
    public class SpecialMeteringPoint : MeteringPoint
    {
        public SpecialMeteringPoint(
            [NotNull]MeteringPointId id,
            GsrnNumber gsrnNumber,
            [NotNull]Address address,
            [NotNull]MeteringMethod meteringMethod,
            MeteringPointType meteringPointType,
            [NotNull]GridAreaLinkId gridAreaLinkId,
            LocationDescription? locationDescription,
            MeterId? meterNumber,
            [NotNull]ReadingOccurrence meterReadingOccurrence,
            [NotNull]PowerLimit powerLimit,
            [NotNull]EffectiveDate effectiveDate,
            GsrnNumber? powerPlantGsrnNumber,
            Capacity? capacity,
            AssetType? assetType)
            : base(
                id,
                gsrnNumber,
                address,
                meteringMethod,
                meteringPointType,
                gridAreaLinkId,
                powerPlantGsrnNumber,
                locationDescription,
                GetUnitType(meteringPointType),
                meterNumber,
                meterReadingOccurrence,
                powerLimit,
                effectiveDate,
                capacity,
                assetType)
        {
            _productType = ProductType.EnergyActive;

            var @event = new SpecialMeteringPointCreated(
                id.Value,
                GsrnNumber.Value,
                gridAreaLinkId.Value,
                meteringMethod.Name,
                _productType.Name,
                meterReadingOccurrence.Name,
                address.City,
                address.Floor,
                address.Room,
                address.BuildingNumber,
                address.CountryCode?.Name,
                address.MunicipalityCode,
                address.PostCode,
                address.StreetCode,
                address.StreetName,
                address.CitySubDivision,
                locationDescription?.Value,
                meterNumber?.Value,
                powerLimit.Ampere,
                powerLimit.Kwh,
                effectiveDate.DateInUtc,
                ConnectionState.PhysicalState.Name,
                powerPlantGsrnNumber?.Value,
                capacity?.Kw,
                assetType?.Name,
                _unitType.Name);

            AddDomainEvent(@event);
        }

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        private SpecialMeteringPoint() { }
#pragma warning restore 8618

        public static BusinessRulesValidationResult CanCreate(SpecialMeteringPointDetails meteringPointDetails)
        {
            if (meteringPointDetails == null) throw new ArgumentNullException(nameof(meteringPointDetails));
            var generalRuleCheckResult = MeteringPoint.CanCreate(meteringPointDetails);
            var rules = new List<IBusinessRule>
            {
                new StreetNameIsRequiredRule(meteringPointDetails.GsrnNumber, meteringPointDetails.Address),
                new MeterReadingOccurrenceConditionalRule(meteringPointDetails.ReadingOccurrence, meteringPointDetails.MeteringPointType),
            };

            return new BusinessRulesValidationResult(generalRuleCheckResult.Errors.Concat(rules.Where(r => r.IsBroken).Select(r => r.ValidationError).ToList()));
        }

        public static SpecialMeteringPoint Create(SpecialMeteringPointDetails meteringPointDetails)
        {
            if (!CanCreate(meteringPointDetails).Success)
            {
                throw new ConsumptionMeteringPointException($"Cannot create consumption metering point due to violation of one or more business rules.");
            }

            return new SpecialMeteringPoint(
                meteringPointDetails.Id,
                meteringPointDetails.GsrnNumber,
                meteringPointDetails.Address,
                meteringPointDetails.MeteringMethod,
                MeteringPointType.Exchange,
                meteringPointDetails.GridAreaLinkId,
                meteringPointDetails.LocationDescription,
                meteringPointDetails.MeterNumber,
                meteringPointDetails.ReadingOccurrence,
                meteringPointDetails.PowerLimit,
                meteringPointDetails.EffectiveDate,
                meteringPointDetails.PowerPlantGsrnNumber,
                meteringPointDetails.Capacity,
                meteringPointDetails.AssetType);
        }

        public override BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails)
        {
            throw new NotImplementedException();
        }

        public override void Connect(ConnectionDetails connectionDetails)
        {
            throw new NotImplementedException();
        }

        private static MeasurementUnitType GetUnitType(EnumerationType meteringPointType)
        {
            return meteringPointType == MeteringPointType.ExchangeReactiveEnergy ? MeasurementUnitType.KVArh : MeasurementUnitType.KWh;
        }
    }
}