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
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Create.Consumption;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Common
{
    public class BusinessProcessCommandFactory : IBusinessProcessCommandFactory
    {
        public IBusinessRequest? CreateFrom(MasterDataDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            var processType = EnumerationType.FromName<BusinessProcessType>(document.ProcessType);
            if (processType == BusinessProcessType.CreateMeteringPoint)
            {
                return new CreateConsumptionMeteringPoint
                {
                    AssetType = document.AssetType,
                    BuildingNumber = document.BuildingNumber,
                    CityName = document.CityName,
                    ConnectionType = document.ConnectionType,
                    CountryCode = document.CountryCode,
                    DisconnectionType = document.DisconnectionType,
                    EffectiveDate = document.EffectiveDate,
                    FloorIdentification = document.FloorIdentification,
                    FromGrid = document.FromGrid,
                    GsrnNumber = document.GsrnNumber,
                    LocationDescription = document.LocationDescription,
                    MaximumCurrent = document.MaximumCurrent,
                    MaximumPower = document.MaximumPower,
                    MeteringMethod = document.MeteringMethod,
                    MeterNumber = document.MeterNumber,
                    MunicipalityCode = document.MunicipalityCode,
                    PostCode = document.PostCode,
                    PowerPlant = document.PowerPlant,
                    ProductType = document.ProductType,
                    RoomIdentification = document.RoomIdentification,
                    SettlementMethod = document.SettlementMethod,
                    StreetCode = document.StreetCode,
                    StreetName = document.StreetName,
                    ToGrid = document.ToGrid,
                    TransactionId = document.TransactionId,
                    UnitType = document.UnitType,
                    GeoInfoReference = document.GeoInfoReference,
                    IsOfficialAddress = document.IsOfficialAddress,
                    MeasureUnitType = document.MeasureUnitType,
                    MeteringGridArea = document.MeteringGridArea,
                    MeterReadingOccurrence = document.MeterReadingOccurrence,
                    NetSettlementGroup = document.NetSettlementGroup,
                    PhysicalConnectionCapacity = document.PhysicalConnectionCapacity,
                    CitySubDivisionName = document.CitySubDivisionName,
                    ScheduledMeterReadingDate = document.ScheduledMeterReadingDate,
                    TypeOfMeteringPoint = document.TypeOfMeteringPoint,
                    PhysicalStatusOfMeteringPoint = document.PhysicalStatusOfMeteringPoint,
                };
            }

            return null;
        }
    }
}
