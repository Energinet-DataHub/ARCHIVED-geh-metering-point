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
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Google.Protobuf;
using CreateMeteringPoint = Energinet.DataHub.MeteringPoints.Application.Create.CreateMeteringPoint;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Mappers
{
    public class CreateMeteringPointMapper : ProtobufOutboundMapper<CreateMeteringPoint>
    {
        protected override IMessage Convert(CreateMeteringPoint obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return new MeteringPointEnvelope()
            {
                CreateMeteringPoint = new Contracts.CreateMeteringPoint
                {
                    GsrnNumber = obj.GsrnNumber,
                    TypeOfMeteringPoint = obj.TypeOfMeteringPoint,
                    SubTypeOfMeteringPoint = obj.SubTypeOfMeteringPoint,
                    MeterReadingOccurrence = obj.MeterReadingOccurrence,
                    MaximumCurrent = obj.MaximumCurrent,
                    MaximumPower = obj.MaximumPower,
                    MeteringGridArea = obj.MeteringGridArea,
                    PowerPlant = obj.PowerPlant,
                    LocationDescription = obj.LocationDescription,
                    InstallationLocationAddress = new Address
                    {
                        StreetName = obj.StreetName,
                        PostCode = obj.PostCode,
                        CityName = obj.CityName,
                        CountryCode = obj.CountryCode,
                        OfficialAddressIndicator = obj.IsOfficialAddress,
                        StreetCode = obj.StreetCode,
                        FloorIdentification = obj.FloorIdentification,
                        RoomIdentification = obj.RoomIdentification,
                        BuildingNumber = obj.BuildingNumber,
                        MunicipalityCode = obj.MunicipalityCode,
                        CityNameSubDivision = obj.CitySubDivisionName,
                        GeoInfoReference = obj.GeoInfoReference,
                    },
                    SettlementMethod = obj.SettlementMethod,
                    UnitType = obj.UnitType,
                    DisconnectionType = obj.DisconnectionType,
                    OccurenceDate = obj.EffectiveDate,
                    MeterNumber = obj.MeterNumber,
                    TransactionId = obj.TransactionId,
                    PhysicalStatusOfMeteringPoint = obj.PhysicalStatusOfMeteringPoint,
                    NetSettlementGroup = obj.NetSettlementGroup,
                    ConnectionType = obj.ConnectionType,
                    AssetType = obj.AssetType,
                    ParentRelatedMeteringPoint = obj.ParentRelatedMeteringPoint,
                    FromGrid = obj.FromGrid,
                    ToGrid = obj.ToGrid,
                    ProductType = obj.ProductType,
                    MeasureUnitType = obj.MeasureUnitType,
                    PhysicalConnectionCapacity = obj.PhysicalConnectionCapacity,
                    ContractedConnectionCapacity = obj.ContractedConnectionCapacity,
                    RatedCurrent = obj.RatedCurrent,
                },
            };
        }
    }
}
