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
using Energinet.DataHub.MeteringPoints.Application.Common.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using CreateMeteringPoint = Energinet.DataHub.MeteringPoints.Contracts.CreateMeteringPoint;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Protobuf.Mappers
{
    public class CreateMeteringPointMapper : ProtobufInboundMapper<CreateMeteringPoint>
    {
        protected override IInboundMessage Convert(CreateMeteringPoint obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return new Application.Create.CreateMeteringPoint(
                StreetName: obj.InstallationLocationAddress.StreetName,
                BuildingNumber: obj.InstallationLocationAddress.BuildingNumber,
                PostCode: obj.InstallationLocationAddress.PostCode,
                CityName: obj.InstallationLocationAddress.CityName,
                CitySubDivisionName: obj.InstallationLocationAddress.CityNameSubDivision,
                MunicipalityCode: obj.InstallationLocationAddress.MunicipalityCode,
                CountryCode: obj.InstallationLocationAddress.CountryCode,
                StreetCode: obj.InstallationLocationAddress.StreetCode,
                FloorIdentification: obj.InstallationLocationAddress.FloorIdentification,
                RoomIdentification: obj.InstallationLocationAddress.RoomIdentification,
                IsOfficialAddress: obj.InstallationLocationAddress.OfficialAddressIndicator,
                GeoInfoReference: obj.InstallationLocationAddress.GeoInfoReference,
                GsrnNumber: obj.GsrnNumber,
                TypeOfMeteringPoint: obj.TypeOfMeteringPoint,
                SubTypeOfMeteringPoint: obj.SubTypeOfMeteringPoint,
                MeterReadingOccurrence: obj.MeterReadingOccurrence,
                MaximumCurrent: obj.MaximumCurrent,
                MaximumPower: obj.MaximumPower,
                MeteringGridArea: obj.MeteringGridArea,
                PowerPlant: obj.PowerPlant,
                LocationDescription: obj.LocationDescription,
                SettlementMethod: obj.SettlementMethod,
                UnitType: obj.UnitType,
                DisconnectionType: obj.DisconnectionType,
                EffectiveDate: obj.OccurenceDate,
                MeterNumber: obj.MeterNumber,
                TransactionId: obj.TransactionId,
                PhysicalStatusOfMeteringPoint: obj.PhysicalStatusOfMeteringPoint,
                NetSettlementGroup: obj.NetSettlementGroup,
                ConnectionType: obj.ConnectionType,
                AssetType: obj.AssetType,
                ParentRelatedMeteringPoint: obj.ParentRelatedMeteringPoint,
                FromGrid: obj.FromGrid,
                ToGrid: obj.ToGrid,
                ProductType: obj.ProductType,
                MeasureUnitType: obj.MeasureUnitType,
                PhysicalConnectionCapacity: obj.PhysicalConnectionCapacity,
                ScheduledMeterReadingDate: obj.ScheduledMeterReadingDate);
        }
    }
}
