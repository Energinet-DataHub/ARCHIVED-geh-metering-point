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
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Disconnect;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.UpdateMasterData;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Common
{
    public class BusinessProcessCommandFactory : IBusinessProcessCommandFactory
    {
        public IBusinessRequest? CreateFrom(MasterDataDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            var processType = EnumerationType.FromName<BusinessProcessType>(document.ProcessType);

            if (processType == BusinessProcessType.CreateMeteringPoint) return CreateNewMeteringPointCommand(document);
            if (processType == BusinessProcessType.ConnectMeteringPoint) return CreateConnectMeteringPointCommand(document);
            if (processType == BusinessProcessType.DisconnectReconnectMeteringPoint) return CreateDisconnectReconnectMeteringPointCommand(document);
            if (processType == BusinessProcessType.ChangeMasterData) return CreateChangeMasterDataCommand(document);
            return null;
        }

        private static IBusinessRequest? CreateChangeMasterDataCommand(MasterDataDocument document)
        {
            return new UpdateMasterDataRequest(
                TransactionId: document.TransactionId,
                GsrnNumber: document.GsrnNumber,
                EffectiveDate: document.EffectiveDate,
                Address: new Address(
                    document.StreetName,
                    document.PostCode,
                    document.CityName,
                    document.StreetCode,
                    document.BuildingNumber,
                    document.CitySubDivisionName,
                    document.CountryCode,
                    document.FloorIdentification,
                    document.RoomIdentification,
                    string.IsNullOrWhiteSpace(document.MunicipalityCode) ? null : int.Parse(document.MunicipalityCode, NumberStyles.Integer, new NumberFormatInfo()),
                    document.IsActualAddress,
                    document.GeoInfoReference,
                    document.LocationDescription),
                MeteringMethod: document.MeteringMethod,
                MeterNumber: document.MeterNumber,
                SettlementMethod: document.SettlementMethod,
                ConnectionType: document.ConnectionType,
                ProductType: document.ProductType,
                UnitType: document.MeasureUnitType,
                AssetType: document.AssetType,
                ReadingPeriodicity: document.MeterReadingOccurrence,
                MaximumCurrent: document.MaximumCurrent,
                MaximumPower: document.MaximumPower,
                PowerPlantGsrnNumber: document.PowerPlant,
                CapacityInKw: document.PhysicalConnectionCapacity,
                ScheduledMeterReadingDate: document.ScheduledMeterReadingDate,
                DisconnectionType: document.DisconnectionType,
                NetSettlementGroup: document.NetSettlementGroup,
                ProductionObligation: document.ProductionObligation,
                ParentRelatedMeteringPoint: document.ParentRelatedMeteringPoint);
        }

        private static IBusinessRequest? CreateConnectMeteringPointCommand(MasterDataDocument document)
        {
            return new ConnectMeteringPointRequest(document.GsrnNumber, document.EffectiveDate, document.TransactionId);
        }

        private static IBusinessRequest? CreateDisconnectReconnectMeteringPointCommand(MasterDataDocument document)
        {
            return new DisconnectReconnectMeteringPointRequest(document.GsrnNumber, document.EffectiveDate, document.TransactionId, document.PhysicalStatusOfMeteringPoint);
        }

        private static IBusinessRequest? CreateNewMeteringPointCommand(MasterDataDocument document)
        {
            return new CreateMeteringPoint
            {
                MeteringPointType = document.TypeOfMeteringPoint,
                AssetType = document.AssetType,
                BuildingNumber = document.BuildingNumber,
                CityName = document.CityName,
                ConnectionType = document.ConnectionType,
                CountryCode = document.CountryCode ?? string.Empty,
                DisconnectionType = document.DisconnectionType ?? string.Empty,
                EffectiveDate = document.EffectiveDate,
                FloorIdentification = document.FloorIdentification,
                GsrnNumber = document.GsrnNumber,
                LocationDescription = document.LocationDescription,
                MaximumCurrent = document.MaximumCurrent,
                MaximumPower = document.MaximumPower,
                MeteringMethod = document.MeteringMethod ?? string.Empty,
                MeterNumber = document.MeterNumber,
                MunicipalityCode = document.MunicipalityCode ?? string.Empty,
                PostCode = document.PostCode,
                PowerPlant = document.PowerPlant,
                RoomIdentification = document.RoomIdentification,
                SettlementMethod = document.SettlementMethod,
                StreetCode = document.StreetCode ?? string.Empty,
                StreetName = document.StreetName,
                TransactionId = document.TransactionId,
                GeoInfoReference = document.GeoInfoReference,
                IsActualAddress = document.IsActualAddress,
                MeteringGridArea = document.MeteringGridArea,
                MeterReadingOccurrence = document.MeterReadingOccurrence ?? string.Empty,
                NetSettlementGroup = document.NetSettlementGroup,
                PhysicalConnectionCapacity = document.PhysicalConnectionCapacity,
                CitySubDivisionName = document.CitySubDivisionName,
                ScheduledMeterReadingDate = document.ScheduledMeterReadingDate,
                ExchangeDetails = new ExchangeDetails(document.FromGrid, document.ToGrid),
                ParentRelatedMeteringPoint = document.ParentRelatedMeteringPoint,
                UnitType = document.MeasureUnitType,
                ProductType = document.ProductType,
            };
        }
    }
}
