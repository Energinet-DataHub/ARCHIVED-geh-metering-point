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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Special.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Special
{
    public class SpecialMeteringPoint : MeteringPoint
    {
        #pragma warning disable
        private readonly MasterData _masterData;
        #pragma warning restore

        public SpecialMeteringPoint(
            [NotNull]MeteringPointId id,
            GsrnNumber gsrnNumber,
            MeteringPointType meteringPointType,
            [NotNull]GridAreaLinkId gridAreaLinkId,
            [NotNull]EffectiveDate effectiveDate,
            MasterData masterData)
            : base(
                id,
                gsrnNumber,
                meteringPointType,
                gridAreaLinkId,
                effectiveDate,
                masterData)
        {
            _masterData = masterData;
            var @event = new SpecialMeteringPointCreated(
                id.Value,
                GsrnNumber.Value,
                gridAreaLinkId.Value,
                MeteringConfiguration.Method.Name,
                _masterData.ProductType.Name,
                _masterData.ReadingOccurrence.Name,
                _masterData.Address.City,
                _masterData.Address.Floor,
                _masterData.Address.Room,
                _masterData.Address.BuildingNumber,
                _masterData.Address.CountryCode?.Name,
                _masterData.Address.MunicipalityCode,
                _masterData.Address.PostCode,
                _masterData.Address.StreetCode,
                _masterData.Address.StreetName,
                _masterData.Address.CitySubDivision,
                MeteringConfiguration.Meter?.Value,
                _masterData.PowerLimit.Ampere,
                _masterData.PowerLimit.Kwh,
                effectiveDate.DateInUtc,
                ConnectionState.PhysicalState.Name,
                _masterData.PowerPlantGsrnNumber?.Value,
                _masterData.Capacity?.Kw,
                _masterData.AssetType?.Name,
                _masterData.UnitType.Name);

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

        public static SpecialMeteringPoint Create(SpecialMeteringPointDetails meteringPointDetails, MasterData masterData)
        {
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            if (!CanCreate(meteringPointDetails).Success)
            {
                throw new SpecialMeteringPointException($"Cannot create {meteringPointDetails.MeteringPointType.Name} metering point due to violation of one or more business rules.");
            }

            return new SpecialMeteringPoint(
                meteringPointDetails.Id,
                meteringPointDetails.GsrnNumber,
                meteringPointDetails.MeteringPointType,
                meteringPointDetails.GridAreaLinkId,
                meteringPointDetails.EffectiveDate,
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
