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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exchange
{
    public class ExchangeMeteringPoint : MeteringPoint
    {
        private GridAreaLinkId _fromGrid;
        #pragma warning disable
        private readonly MasterData _masterData;
        private GridAreaLinkId _toGrid;

        public ExchangeMeteringPoint(
            [NotNull]MeteringPointId id,
            GsrnNumber gsrnNumber,
            [NotNull]Address address,
            MeteringPointType meteringPointType,
            [NotNull]GridAreaLinkId gridAreaLinkId,
            [NotNull]ReadingOccurrence meterReadingOccurrence,
            [NotNull]PowerLimit powerLimit,
            [NotNull]EffectiveDate effectiveDate,
            [NotNull]GridAreaLinkId toGrid,
            [NotNull]GridAreaLinkId fromGrid,
            MeteringConfiguration meteringConfiguration,
            MasterData masterData)
            : base(
                id,
                gsrnNumber,
                address,
                meteringPointType,
                gridAreaLinkId,
                powerPlantGsrnNumber: null,
                MeasurementUnitType.KWh,
                meterReadingOccurrence,
                powerLimit,
                effectiveDate,
                capacity: null,
                assetType: null,
                meteringConfiguration,
                masterData)
        {
            _toGrid = toGrid;
            _fromGrid = fromGrid;
            _masterData = masterData;

            var @event = new ExchangeMeteringPointCreated(
                id.Value,
                GsrnNumber.Value,
                gridAreaLinkId.Value,
                MeteringConfiguration.Method.Name,
                _masterData.ProductType.Name,
                meterReadingOccurrence.Name,
                _unitType.Name,
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
                address.LocationDescription,
                MeteringConfiguration.Meter?.Value,
                powerLimit.Ampere,
                powerLimit.Kwh,
                effectiveDate.DateInUtc,
                ConnectionState.PhysicalState.Name,
                fromGrid.Value,
                toGrid.Value);

            AddDomainEvent(@event);
        }

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        private ExchangeMeteringPoint() { }
#pragma warning restore 8618

        public static BusinessRulesValidationResult CanCreate(ExchangeMeteringPointDetails meteringPointDetails)
        {
            if (meteringPointDetails == null) throw new ArgumentNullException(nameof(meteringPointDetails));
            var generalRuleCheckResult = MeteringPoint.CanCreate(meteringPointDetails);
            var rules = new List<IBusinessRule>()
            {
                new StreetNameIsRequiredRule(meteringPointDetails.GsrnNumber, meteringPointDetails.Address),
                new MeterReadingOccurrenceRule(meteringPointDetails.ReadingOccurrence),
                new GeoInfoReferenceRequirementRule(meteringPointDetails.Address),
            };

            return new BusinessRulesValidationResult(generalRuleCheckResult.Errors.Concat(rules.Where(r => r.IsBroken).Select(r => r.ValidationError).ToList()));
        }

        public static ExchangeMeteringPoint Create(ExchangeMeteringPointDetails meteringPointDetails, MasterData masterData)
        {
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            if (!CanCreate(meteringPointDetails).Success)
            {
                throw new ConsumptionMeteringPointException($"Cannot create exchange metering point due to violation of one or more business rules.");
            }

            return new ExchangeMeteringPoint(
                meteringPointDetails.Id,
                meteringPointDetails.GsrnNumber,
                meteringPointDetails.Address,
                MeteringPointType.Exchange,
                meteringPointDetails.GridAreaLinkId,
                meteringPointDetails.ReadingOccurrence,
                meteringPointDetails.PowerLimit,
                meteringPointDetails.EffectiveDate,
                meteringPointDetails.ToGridLinkId,
                meteringPointDetails.FromGridLinkId,
                meteringPointDetails.MeteringConfiguration,
                masterData);
        }

        public override BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails)
        {
            throw new NotImplementedException();
        }

        public override void Connect(ConnectionDetails connectionDetails)
        {
            throw new NotImplementedException();
        }
    }
}
