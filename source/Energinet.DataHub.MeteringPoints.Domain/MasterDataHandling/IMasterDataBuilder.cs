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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    /// <summary>
    /// Builder for metering point master data
    /// </summary>
    public interface IMasterDataBuilder
    {
        /// <summary>
        /// Validates configured values of builder
        /// </summary>
        /// <returns><see cref="BusinessRulesValidationResult"/></returns>
        BusinessRulesValidationResult Validate();

        /// <summary>
        /// Configures net settlement group
        /// </summary>
        /// <param name="netSettlementGroup"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithNetSettlementGroup(string netSettlementGroup);

        /// <summary>
        /// Build master data
        /// </summary>
        /// <returns><see cref="MasterData"/></returns>
        MasterData Build();

        /// <summary>
        /// Configures meter method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="meterNumber"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithMeteringConfiguration(string method, string? meterNumber);

        /// <summary>
        /// Configures address
        /// </summary>
        /// <param name="streetName"></param>
        /// <param name="streetCode"></param>
        /// <param name="buildingNumber"></param>
        /// <param name="city"></param>
        /// <param name="citySubDivision"></param>
        /// <param name="postCode"></param>
        /// <param name="countryCode"></param>
        /// <param name="floor"></param>
        /// <param name="room"></param>
        /// <param name="municipalityCode"></param>
        /// <param name="isActual"></param>
        /// <param name="geoInfoReference"></param>
        /// <param name="locationDescription"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithAddress(string? streetName = null, string? streetCode = null, string? buildingNumber = null, string? city = null, string? citySubDivision = null, string? postCode = null, CountryCode? countryCode = null, string? floor = null, string? room = null, int? municipalityCode = null, bool? isActual = null, Guid? geoInfoReference = null, string? locationDescription = null);

        /// <summary>
        /// Configures measurement unit type
        /// </summary>
        /// <param name="measurementUnitType"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithMeasurementUnitType(string? measurementUnitType);

        /// <summary>
        /// Configures GSRN-number of power plant
        /// </summary>
        /// <param name="gsrnNumber"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithPowerPlant(string? gsrnNumber);

        /// <summary>
        /// Configures meter reading periodicity
        /// </summary>
        /// <param name="readingPeriodicity"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithReadingPeriodicity(string? readingPeriodicity);

        /// <summary>
        /// Configures power limit
        /// </summary>
        /// <param name="kwh"></param>
        /// <param name="ampere"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithPowerLimit(int kwh, int ampere);

        /// <summary>
        /// Configures settlement method
        /// </summary>
        /// <param name="settlementMethod"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithSettlementMethod(string? settlementMethod);

        /// <summary>
        /// Configures disconnection type
        /// </summary>
        /// <param name="disconnectionType"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithDisconnectionType(string? disconnectionType);

        /// <summary>
        /// Configures asset type
        /// </summary>
        /// <param name="assetType"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithAssetType(string? assetType);

        /// <summary>
        /// Configures scheduled meter reading date
        /// </summary>
        /// <param name="scheduledMeterReadingDate"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithScheduledMeterReadingDate(string? scheduledMeterReadingDate);

        /// <summary>
        /// Configures capacity
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithCapacity(string? capacity);

        /// <summary>
        /// Configures effective date
        /// </summary>
        /// <param name="effectiveDate"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder EffectiveOn(string? effectiveDate);

        /// <summary>
        /// Configures product type
        /// </summary>
        /// <param name="productType"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithProductType(string productType);

        /// <summary>
        /// Configures connection type
        /// </summary>
        /// <param name="connectionType"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithConnectionType(string? connectionType);

        /// <summary>
        /// Configures production obligation of a production metering point
        /// </summary>
        /// <param name="productionObligation"></param>
        /// <returns><see cref="IMasterDataBuilder"/></returns>
        IMasterDataBuilder WithProductionObligation(bool? productionObligation);
    }
}
