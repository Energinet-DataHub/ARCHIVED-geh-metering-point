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
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.MeteringPoints.Connect;
using Energinet.DataHub.MeteringPoints.Application.MeteringPoints.Create.Consumption;
using Energinet.DataHub.MeteringPoints.Application.MeteringPoints.Create.Exchange;
using Energinet.DataHub.MeteringPoints.Application.MeteringPoints.Create.Production;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.MarketDocuments.CommandFactories
{
    public class BusinessProcessCommandFactory : IBusinessProcessCommandFactory
    {
        public IBusinessRequest? CreateFrom(MasterDataDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            var processType = EnumerationType.FromName<BusinessProcessType>(document.ProcessType);

            if (processType == BusinessProcessType.CreateMeteringPoint) return CreateNewMeteringPointCommand(document);
            if (processType == BusinessProcessType.ConnectMeteringPoint) return CreateConnectMeteringPointCommand(document);
            return null;
        }

        private static IBusinessRequest? CreateConnectMeteringPointCommand(MasterDataDocument document)
        {
            return new ConnectMeteringPoint(document.GsrnNumber, document.EffectiveDate, document.TransactionId);
        }

        private static IBusinessRequest? CreateNewMeteringPointCommand(MasterDataDocument document)
        {
            var meteringPointType = EnumerationType.FromName<MeteringPointType>(document.TypeOfMeteringPoint);

            if (meteringPointType == MeteringPointType.Consumption)
            {
                return CreateConsumptionMeteringPoint(document);
            }

            if (meteringPointType == MeteringPointType.Production)
            {
                return CreateProductionMeteringPoint(document);
            }

            if (meteringPointType == MeteringPointType.Exchange)
            {
                return CreateExchangeMeteringPoint(document);
            }

            throw new NotImplementedException(meteringPointType.Name);
        }

        private static IBusinessRequest CreateConsumptionMeteringPoint(MasterDataDocument document)
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
                GsrnNumber = document.GsrnNumber,
                LocationDescription = document.LocationDescription,
                MaximumCurrent = document.MaximumCurrent,
                MaximumPower = document.MaximumPower,
                MeteringMethod = document.MeteringMethod,
                MeterNumber = document.MeterNumber,
                MunicipalityCode = document.MunicipalityCode,
                PostCode = document.PostCode,
                PowerPlant = document.PowerPlant,
                RoomIdentification = document.RoomIdentification,
                SettlementMethod = document.SettlementMethod,
                StreetCode = document.StreetCode,
                StreetName = document.StreetName,
                TransactionId = document.TransactionId,
                GeoInfoReference = document.GeoInfoReference,
                IsActualAddress = document.IsActualAddress,
                MeteringGridArea = document.MeteringGridArea,
                MeterReadingOccurrence = document.MeterReadingOccurrence,
                NetSettlementGroup = document.NetSettlementGroup,
                PhysicalConnectionCapacity = document.PhysicalConnectionCapacity,
                CitySubDivisionName = document.CitySubDivisionName,
                ScheduledMeterReadingDate = document.ScheduledMeterReadingDate,
            };
        }

        private static IBusinessRequest CreateProductionMeteringPoint(MasterDataDocument document)
        {
            return new CreateProductionMeteringPoint
            {
                AssetType = document.AssetType!,
                BuildingNumber = document.BuildingNumber,
                CityName = document.CityName,
                ConnectionType = document.ConnectionType,
                CountryCode = document.CountryCode,
                DisconnectionType = document.DisconnectionType,
                EffectiveDate = document.EffectiveDate,
                FloorIdentification = document.FloorIdentification,
                GsrnNumber = document.GsrnNumber,
                LocationDescription = document.LocationDescription,
                MaximumCurrent = document.MaximumCurrent,
                MaximumPower = document.MaximumPower,
                MeteringMethod = document.MeteringMethod,
                MeterNumber = document.MeterNumber,
                MunicipalityCode = document.MunicipalityCode,
                PostCode = document.PostCode,
                PowerPlant = document.PowerPlant,
                RoomIdentification = document.RoomIdentification,
                StreetCode = document.StreetCode,
                StreetName = document.StreetName,
                TransactionId = document.TransactionId,
                GeoInfoReference = document.GeoInfoReference,
                IsActualAddress = document.IsActualAddress,
                MeteringGridArea = document.MeteringGridArea,
                MeterReadingOccurrence = document.MeterReadingOccurrence,
                NetSettlementGroup = document.NetSettlementGroup,
                PhysicalConnectionCapacity = document.PhysicalConnectionCapacity,
                CitySubDivisionName = document.CitySubDivisionName,
            };
        }

        private static IBusinessRequest CreateExchangeMeteringPoint(MasterDataDocument document)
        {
            return new CreateExchangeMeteringPoint
            {
                BuildingNumber = document.BuildingNumber,
                CityName = document.CityName,
                CountryCode = document.CountryCode,
                EffectiveDate = document.EffectiveDate,
                FloorIdentification = document.FloorIdentification,
                GsrnNumber = document.GsrnNumber,
                LocationDescription = document.LocationDescription,
                MaximumCurrent = document.MaximumCurrent,
                MaximumPower = document.MaximumPower,
                MeteringMethod = document.MeteringMethod,
                MeterNumber = document.MeterNumber,
                MunicipalityCode = document.MunicipalityCode,
                PostCode = document.PostCode,
                RoomIdentification = document.RoomIdentification,
                StreetCode = document.StreetCode,
                StreetName = document.StreetName,
                TransactionId = document.TransactionId,
                MeteringGridArea = document.MeteringGridArea,
                MeterReadingOccurrence = document.MeterReadingOccurrence,
                PhysicalConnectionCapacity = document.PhysicalConnectionCapacity,
                CitySubDivisionName = document.CitySubDivisionName,
                FromGrid = document.FromGrid ?? string.Empty,
                ToGrid = document.ToGrid ?? string.Empty,
            };
        }
    }
}
