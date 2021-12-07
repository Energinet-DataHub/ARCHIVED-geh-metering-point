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
using System.Collections.ObjectModel;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules.Connect;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption
{
    public class ConsumptionMeteringPoint : MarketMeteringPoint
    {
        private MasterData _masterData;

#pragma warning disable
        private ConsumptionMeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            MeteringPointType meteringPointType,
            GridAreaLinkId gridAreaLinkId,
            EffectiveDate effectiveDate,
            NetSettlementGroup netSettlementGroup,
            DisconnectionType disconnectionType,
            ConnectionType? connectionType,
            Capacity? capacity,
            MeteringConfiguration meteringConfiguration,
            MasterData masterData)
            : base(
                id,
                gsrnNumber,
                address,
                meteringPointType,
                gridAreaLinkId,
                effectiveDate,
                capacity,
                connectionType,
                disconnectionType,
                netSettlementGroup,
                meteringConfiguration,
                masterData)
        {
            ConnectionState = ConnectionState.New();
            _masterData = masterData;

            var @event = new ConsumptionMeteringPointCreated(
                id.Value,
                GsrnNumber.Value,
                gridAreaLinkId.Value,
                MeteringConfiguration.Method.Name,
                _masterData.ProductType.Name,
                _masterData.ReadingOccurrence.Name,
                _masterData.UnitType.Name,
                masterData.SettlementMethod.Name,
                netSettlementGroup.Name,
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
                address.IsActual.GetValueOrDefault(),
                address.GeoInfoReference,
                _masterData.PowerPlantGsrnNumber?.Value,
                address.LocationDescription,
                MeteringConfiguration.Meter?.Value,
                _masterData.PowerLimit.Ampere,
                _masterData.PowerLimit.Kwh,
                effectiveDate.DateInUtc,
                DisconnectionType.Name,
                ConnectionType?.Name,
                _masterData.AssetType?.Name,
                ConnectionState.PhysicalState.Name,
                _masterData.ScheduledMeterReadingDate?.MonthAndDay,
                capacity?.Kw);

            AddDomainEvent(@event);
        }

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        private ConsumptionMeteringPoint() { }
#pragma warning restore 8618

        public static new BusinessRulesValidationResult CanCreate(ConsumptionMeteringPointDetails meteringPointDetails)
        {
            if (meteringPointDetails == null) throw new ArgumentNullException(nameof(meteringPointDetails));
            var generalRuleCheckResult = MarketMeteringPoint.CanCreate(meteringPointDetails);
            var rules = new List<IBusinessRule>()
            {
                new PowerPlantIsRequiredForNetSettlementGroupRule(meteringPointDetails.GsrnNumber, meteringPointDetails.NetSettlementGroup, meteringPointDetails.PowerPlantGsrnNumber),
                new ScheduledMeterReadingDateRule(meteringPointDetails.ScheduledMeterReadingDate, meteringPointDetails.NetSettlementGroup),
                new CapacityRequirementRule(meteringPointDetails.Capacity, meteringPointDetails.NetSettlementGroup),
                new AssetTypeRequirementRule(meteringPointDetails.AssetType, meteringPointDetails.NetSettlementGroup),
                new SettlementMethodMustBeFlexOrNonProfiledRule(meteringPointDetails.SettlementMethod),
            };

            return new BusinessRulesValidationResult(generalRuleCheckResult.Errors.Concat(rules.Where(r => r.IsBroken).Select(r => r.ValidationError).ToList()));
        }

        public static ConsumptionMeteringPoint Create(ConsumptionMeteringPointDetails meteringPointDetails, MasterData masterData)
        {
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            if (!CanCreate(meteringPointDetails).Success)
            {
                throw new ConsumptionMeteringPointException($"Cannot create consumption metering point due to violation of one or more business rules.");
            }

            return new ConsumptionMeteringPoint(
                meteringPointDetails.Id,
                meteringPointDetails.GsrnNumber,
                meteringPointDetails.Address,
                MeteringPointType.Consumption,
                meteringPointDetails.GridAreaLinkId,
                meteringPointDetails.EffectiveDate,
                meteringPointDetails.NetSettlementGroup,
                meteringPointDetails.DisconnectionType,
                meteringPointDetails.ConnectionType,
                meteringPointDetails.Capacity,
                meteringPointDetails.MeteringConfiguration,
                masterData);
        }

        public override BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails)
        {
            var rules = new Collection<IBusinessRule>
            {
                new MeteringPointMustHavePhysicalStateNewRule(GsrnNumber, _meteringPointType, ConnectionState.PhysicalState),
                new MustHaveEnergySupplierRule(GsrnNumber, connectionDetails, EnergySupplierDetails),
            };

            return new BusinessRulesValidationResult(rules);
        }

        public override void Connect(ConnectionDetails connectionDetails)
        {
            if (connectionDetails == null) throw new ArgumentNullException(nameof(connectionDetails));
            if (!ConnectAcceptable(connectionDetails).Success)
            {
                throw MeteringPointConnectException.Create(Id, GsrnNumber);
            }

            ConnectionState = ConnectionState.Connected(connectionDetails.EffectiveDate);
            AddDomainEvent(new MeteringPointConnected(Id.Value, GsrnNumber.Value, connectionDetails.EffectiveDate));
        }
    }
}
