﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Core.XmlConversion.XmlConverter.Abstractions;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Transport;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.MarketDocuments
{
    public record MasterDataDocument(
            string ProcessType = "",
            string? StreetName = null,
            string? BuildingNumber = null,
            string? PostCode = null,
            string? CityName = null,
            string? CitySubDivisionName = null,
            string? MunicipalityCode = null,
            string? CountryCode = null,
            string? StreetCode = null,
            string? FloorIdentification = null,
            string? RoomIdentification = null,
            bool? IsActualAddress = null,
            string GsrnNumber = "",
            string TypeOfMeteringPoint = "",
            string? MeteringMethod = null,
            string? MeterReadingOccurrence = null,
            string? MaximumCurrent = null,
            string? MaximumPower = null,
            string MeteringGridArea = "",
            string? PowerPlant = null,
            string? LocationDescription = null,
            string? SettlementMethod = null,
            string? DisconnectionType = null,
            string EffectiveDate = "",
            string? MeterNumber = null,
            string TransactionId = "",
            string PhysicalStatusOfMeteringPoint = "",
            string? NetSettlementGroup = null,
            string? ConnectionType = null,
            string? AssetType = null,
            string? FromGrid = null,
            string? ToGrid = null,
            string? ParentRelatedMeteringPoint = null,
            string? ProductType = null,
            string? PhysicalConnectionCapacity = null,
            string? GeoInfoReference = null,
            string? MeasureUnitType = null,
            string? ScheduledMeterReadingDate = null,
            bool? ProductionObligation = null)
        : IInternalMarketDocument,
            IOutboundMessage,
            IInboundMessage,
            IRequest<BusinessProcessResult>;
}
