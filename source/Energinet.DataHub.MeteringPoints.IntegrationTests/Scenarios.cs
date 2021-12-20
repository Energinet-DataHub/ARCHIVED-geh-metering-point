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
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public static class Scenarios
    {
        internal static Address CreateAddress()
        {
            return new Address(
                SampleData.StreetName,
                SampleData.PostCode,
                SampleData.CityName,
                SampleData.StreetCode,
                SampleData.BuildingNumber,
                SampleData.CitySubDivisionName,
                SampleData.CountryCode,
                SampleData.FloorIdentification,
                SampleData.RoomIdentification,
                int.Parse(SampleData.MunicipalityCode, NumberStyles.Any, new NumberFormatInfo()),
                SampleData.IsActualAddress,
                Guid.Parse(SampleData.GeoInfoReference));
        }

        internal static CreateMeteringPoint CreateConsumptionMeteringPointCommand()
        {
            return new CreateMeteringPoint(
                MeteringPointType: MeteringPointType.Consumption.Name,
                SampleData.StreetName,
                SampleData.BuildingNumber,
                SampleData.PostCode,
                SampleData.CityName,
                SampleData.CitySubDivisionName,
                SampleData.MunicipalityCode,
                SampleData.CountryCode,
                SampleData.StreetCode,
                SampleData.FloorIdentification,
                SampleData.RoomIdentification,
                SampleData.IsActualAddress,
                SampleData.GsrnNumber,
                MeteringMethod.Calculated.Name,
                SampleData.ReadingOccurrence,
                0,
                0,
                SampleData.MeteringGridArea,
                SampleData.PowerPlantGsrnNumber,
                string.Empty,
                SampleData.SettlementMethod,
                SampleData.DisconnectionType,
                SampleData.EffectiveDate,
                string.Empty,
                Guid.NewGuid().ToString(),
                NetSettlementGroup.Six.Name,
                ConnectionType.Installation.Name,
                SampleData.AssetType,
                "0",
                SampleData.GeoInfoReference,
                "0101");
        }

        internal static CreateMeteringPoint CreateCommand(MeteringPointType meteringPointType)
        {
            return new CreateMeteringPoint(
                MeteringPointType: meteringPointType.Name,
                SampleData.StreetName,
                SampleData.BuildingNumber,
                SampleData.PostCode,
                SampleData.CityName,
                SampleData.CitySubDivisionName,
                SampleData.MunicipalityCode,
                SampleData.CountryCode,
                SampleData.StreetCode,
                SampleData.FloorIdentification,
                SampleData.RoomIdentification,
                SampleData.IsActualAddress,
                SampleData.GsrnNumber,
                MeteringMethod.Calculated.Name,
                SampleData.ReadingOccurrence,
                0,
                0,
                SampleData.MeteringGridArea,
                SampleData.PowerPlantGsrnNumber,
                string.Empty,
                SampleData.SettlementMethod,
                SampleData.DisconnectionType,
                SampleData.EffectiveDate,
                string.Empty,
                Guid.NewGuid().ToString(),
                NetSettlementGroup.Six.Name,
                ConnectionType.Installation.Name,
                SampleData.AssetType,
                "0",
                SampleData.GeoInfoReference,
                "0101",
                new ExchangeDetails(SampleData.MeteringGridArea, SampleData.MeteringGridArea),
                UnitType: MeasurementUnitType.KWh.Name);
        }

        internal static MasterDataDocument CreateDocument()
        {
            return new(
                BusinessProcessType.CreateMeteringPoint.Name,
                SampleData.StreetName,
                SampleData.BuildingNumber,
                SampleData.PostCode,
                SampleData.CityName,
                SampleData.CitySubDivisionName,
                SampleData.MunicipalityCode,
                SampleData.CountryCode,
                SampleData.StreetCode,
                SampleData.FloorIdentification,
                SampleData.RoomIdentification,
                SampleData.IsActualAddress,
                SampleData.GsrnNumber,
                SampleData.TypeOfMeteringPoint,
                SampleData.SubTypeOfMeteringPoint,
                SampleData.ReadingOccurrence,
                0,
                0,
                SampleData.MeteringGridArea,
                SampleData.PowerPlantGsrnNumber,
                string.Empty,
                SampleData.SettlementMethod,
                SampleData.DisconnectionType,
                SampleData.EffectiveDate,
                SampleData.MeterNumber,
                SampleData.Transaction,
                SampleData.PhysicalState,
                SampleData.NetSettlementGroup,
                SampleData.ConnectionType,
                SampleData.AssetType,
                "123",
                ToGrid: "456",
                ParentRelatedMeteringPoint: null,
                SampleData.ProductType,
                null,
                SampleData.GeoInfoReference,
                SampleData.MeasurementUnitType,
                SampleData.ScheduledMeterReadingDate);
        }
    }
}
