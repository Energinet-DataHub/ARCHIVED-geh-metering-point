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
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.RequestResponse.Response;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Requests.MasterData
{
    public class MasterDataRequestResponseOutBoundMapper : ProtobufOutboundMapper<Application.RequestMasterData.MasterData>
    {
        protected override IMessage Convert(Application.RequestMasterData.MasterData obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return new MasterDataRequestResponse
            {
                GsrnNumber = obj.GsrnNumber,
                Address = new Address
                {
                    StreetName = obj.Address.StreetName,
                    StreetCode = obj.Address.StreetCode,
                    PostCode = obj.Address.PostCode,
                    City = obj.Address.City,
                    CountryCode = obj.Address.CountryCode,
                    CitySubDivision = obj.Address.CitySubDivision,
                    Floor = obj.Address.Floor,
                    Room = obj.Address.Room,
                    BuildingNumber = obj.Address.BuildingNumber,
                    MunicipalityCode = obj.Address.MunicipalityCode,
                    IsActualAddress = obj.Address.IsActualAddress,
                    GeoInfoReference = obj.Address.GeoInfoReference.ToString(),
                    LocationDescription = obj.Address.LocationDescription,
                },
                Series = new Series
                {
                    Product = obj.Series.Product,
                    UnitType = obj.Series.UnitType,
                },
                GridAreaDetails = new GridAreaDetails
                {
                    Code = obj.GridAreaDetails.Code,
                    ToCode = obj.GridAreaDetails.ToCode,
                    FromCode = obj.GridAreaDetails.FromCode,
                },
                ConnectionState = obj.ConnectionState,
                MeteringMethod = obj.MeteringMethod,
                ReadingPeriodicity = obj.ReadingPeriodicity,
                Type = obj.Type,
                MaximumCurrent = obj.MaximumCurrent,
                MaximumPower = obj.MaximumPower,
                PowerPlantGsrnNumber = obj.PowerPlantGsrnNumber,
                EffectiveDate = obj.EffectiveDate.ToUniversalTime().ToTimestamp(),
                MeterNumber = obj.MeterNumber,
                Capacity = obj.Capacity,
                AssetType = obj.AssetType,
                SettlementMethod = obj.SettlementMethod,
                ScheduledMeterReadingDate = obj.ScheduledMeterReadingDate,
                ProductionObligation = obj.ProductionObligation,
                NetSettlementGroup = obj.NetSettlementGroup,
                DisconnetionType = obj.DisconnectionType,
                ConnectionType = obj.ConnectionType,
                ParentRelatedMeteringPoint = obj.ParentRelatedMeteringPoint.ToString(),
                GridOperatorId = obj.GridOperatorId.ToString(),
            };
        }
    }
}
