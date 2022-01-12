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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Exceptions;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    [UnitTest]
    public class UpdateMasterDataTests : TestBase
    {
        [Fact]
        public void Master_data_is_updated()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption, MasterDataBuilderForConsumption());

            var updatedMasterData = UpdatedMasterData(meteringPoint.MasterData);
            meteringPoint.UpdateMasterData(updatedMasterData, new MasterDataValidator());

            var masterDataWasUpdated = meteringPoint.DomainEvents.FirstOrDefault(e => e is MasterDataWasUpdated) as MasterDataWasUpdated;
            Assert.Equal(updatedMasterData.Address.StreetName, masterDataWasUpdated?.StreetName);
            Assert.Equal(updatedMasterData.Address.StreetCode, masterDataWasUpdated?.StreetCode);
            Assert.Equal(updatedMasterData.Address.City, masterDataWasUpdated?.City);
            Assert.Equal(updatedMasterData.Address.Floor, masterDataWasUpdated?.Floor);
            Assert.Equal(updatedMasterData.Address.Room, masterDataWasUpdated?.Room);
            Assert.Equal(updatedMasterData.Address.BuildingNumber, masterDataWasUpdated?.BuildingNumber);
            Assert.Equal(updatedMasterData.Address.CountryCode?.Name, masterDataWasUpdated?.CountryCode);
            Assert.Equal(updatedMasterData.Address.IsActual, masterDataWasUpdated?.IsActual);
            Assert.Equal(updatedMasterData.Address.LocationDescription, masterDataWasUpdated?.LocationDescription);
            Assert.Equal(updatedMasterData.Address.MunicipalityCode, masterDataWasUpdated?.MunicipalityCode);
            Assert.Equal(updatedMasterData.Address.PostCode, masterDataWasUpdated?.PostCode);
            Assert.Equal(updatedMasterData.Address.CitySubDivision, masterDataWasUpdated?.CitySubDivision);
            Assert.Equal(updatedMasterData.Address.GeoInfoReference, masterDataWasUpdated?.GeoInfoReference);
            Assert.Equal(updatedMasterData.Capacity?.Kw, masterDataWasUpdated?.Capacity);
            Assert.Equal(updatedMasterData.AssetType?.Name, masterDataWasUpdated?.AssetType);
            Assert.Equal(updatedMasterData.ConnectionType?.Name, masterDataWasUpdated?.ConnectionType);
            Assert.Equal(updatedMasterData.DisconnectionType?.Name, masterDataWasUpdated?.DisconnectionType);
            Assert.Equal(updatedMasterData.EffectiveDate?.DateInUtc.ToString(), masterDataWasUpdated?.EffectiveDate);
            Assert.Equal(updatedMasterData.MeteringConfiguration.Meter.Value, masterDataWasUpdated?.MeterNumber);
            Assert.Equal(updatedMasterData.MeteringConfiguration.Method.Name, masterDataWasUpdated?.MeteringMethod);
            Assert.Equal(updatedMasterData.PowerLimit.Ampere, masterDataWasUpdated?.PowerLimitInAmpere);
            Assert.Equal(updatedMasterData.PowerLimit.Kwh, masterDataWasUpdated?.PowerLimitInKwh);
            Assert.Equal(updatedMasterData.ProductionObligation, masterDataWasUpdated?.ProductionObligation);
            Assert.Equal(updatedMasterData.ProductType.Name, masterDataWasUpdated?.ProductType);
            Assert.Equal(updatedMasterData.ReadingOccurrence.Name, masterDataWasUpdated?.ReadingOccurrence);
            Assert.Equal(updatedMasterData.SettlementMethod?.Name, masterDataWasUpdated?.SettlementMethod);
            Assert.Equal(updatedMasterData.UnitType.Name, masterDataWasUpdated?.UnitType);
            Assert.Equal(updatedMasterData.NetSettlementGroup?.Name, masterDataWasUpdated?.NetSettlementGroup);
            Assert.Equal(updatedMasterData.PowerPlantGsrnNumber?.Value, masterDataWasUpdated?.PowerPlantGsrnNumber);
            Assert.Equal(updatedMasterData.ScheduledMeterReadingDate?.MonthAndDay, masterDataWasUpdated?.ScheduledMeterReadingDate);
        }

        [Fact]
        public void Master_data_cannot_be_changed_if_not_valid_for_the_type_of_metering_point()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption);

            var masterDataWithInvalidValues = InvalidForConsumptionMeteringPoints(meteringPoint.MasterData);

            Assert.Throws<MasterDataChangeException>(() => meteringPoint.UpdateMasterData(masterDataWithInvalidValues, new MasterDataValidator()));
        }

        [Fact]
        public void Can_detect_invalid_master_data_values()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption);
            var masterDataWithInvalidValues = InvalidForConsumptionMeteringPoints(meteringPoint.MasterData);

            var validationResult =
                meteringPoint.CanUpdateMasterData(masterDataWithInvalidValues, new MasterDataValidator());
        }

        private static MasterData UpdatedMasterData(MasterData currentMasterData)
        {
            var updatedAddress = Address.Create(
                streetName: "Updated street name",
                SampleData.StreetCode,
                string.Empty,
                SampleData.CityName,
                string.Empty,
                SampleData.PostCode,
                CountryCode.DK,
                string.Empty,
                string.Empty,
                default,
                isActual: true,
                geoInfoReference: Guid.NewGuid(),
                null);

            return new MasterData(
                currentMasterData.ProductType,
                currentMasterData.UnitType,
                currentMasterData.AssetType,
                currentMasterData.ReadingOccurrence,
                currentMasterData.PowerLimit,
                currentMasterData.PowerPlantGsrnNumber,
                currentMasterData.EffectiveDate!,
                currentMasterData.Capacity,
                updatedAddress,
                currentMasterData.MeteringConfiguration,
                currentMasterData.SettlementMethod!,
                currentMasterData.ScheduledMeterReadingDate,
                currentMasterData.ConnectionType,
                currentMasterData.DisconnectionType!,
                currentMasterData.NetSettlementGroup!,
                currentMasterData.ProductionObligation);
        }

        private static MasterData InvalidForConsumptionMeteringPoints(MasterData currentMasterData)
        {
            var updatedAddress = Address.Create(
                streetName: null,
                SampleData.StreetCode,
                string.Empty,
                SampleData.CityName,
                string.Empty,
                SampleData.PostCode,
                CountryCode.DK,
                string.Empty,
                string.Empty,
                default,
                isActual: true,
                geoInfoReference: Guid.NewGuid(),
                null);

            return new MasterData(
                currentMasterData.ProductType,
                currentMasterData.UnitType,
                currentMasterData.AssetType,
                currentMasterData.ReadingOccurrence,
                currentMasterData.PowerLimit,
                currentMasterData.PowerPlantGsrnNumber,
                currentMasterData.EffectiveDate!,
                currentMasterData.Capacity,
                updatedAddress,
                currentMasterData.MeteringConfiguration,
                currentMasterData.SettlementMethod!,
                currentMasterData.ScheduledMeterReadingDate,
                currentMasterData.ConnectionType,
                currentMasterData.DisconnectionType!,
                currentMasterData.NetSettlementGroup!,
                currentMasterData.ProductionObligation);
        }
    }
}
