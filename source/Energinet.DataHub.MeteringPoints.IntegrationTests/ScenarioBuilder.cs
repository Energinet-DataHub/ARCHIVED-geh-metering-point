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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public class ScenarioBuilder
    {
        private readonly MeteringPointContext _context;
        private readonly IMediator _mediator;
        private readonly string _effectiveDate = EffectiveDateFactory.InstantAsOfToday().ToString();
        private readonly string _transaction = "175C1079-4E53-47CB-BE27-02E9BB674487";
        private readonly string _physicalConnectionCapacity = "0";
        private readonly string _scheduledMeterReadingDate = string.Empty;
        private MeteringPointType _meteringPointType = MeteringPointType.Consumption;
        private Guid _administratorId = Guid.NewGuid();
        private Address? _address;
        private GsrnNumber? _gsrnNumber;
        private MeteringMethod? _meteringMethod;
        private ReadingOccurrence _readingOccurrence = ReadingOccurrence.Hourly;
        private GsrnNumber? _powerPlantGsrnNumber;
        private string? _maxPower;
        private string? _maxCurrent;
        private SettlementMethod? _settlementMethod = SettlementMethod.Flex;
        private DisconnectionType? _disconnectionType = DisconnectionType.Manual;
        private MeterId? _meterId;
        private NetSettlementGroup? _netSettlementGroup;
        private ConnectionType? _connectionType;
        private AssetType? _assetType;
        private MeasurementUnitType? _unitType = MeasurementUnitType.KWh;
        private ProductType? _productType = ProductType.EnergyActive;
        private GridArea? _gridArea;
        private string? _sourceGridArea;
        private string? _targetGridArea;

        public ScenarioBuilder(MeteringPointContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
            _context.Database.ExecuteSqlRaw("DELETE FROM dbo.GridAreaLinks");
            _context.Database.ExecuteSqlRaw("DELETE FROM dbo.GridAreas");
        }

        public ScenarioBuilder WithGridArea(string code, Guid operatorId)
        {
            _gridArea = GridArea.Create(
                GridAreaId.New(),
                GridAreaLinkId.New(),
                GridAreaName.Create("Test"),
                GridAreaCode.Create(code),
                GridOperatorId.Create(operatorId));
            _context.GridAreas.Add(_gridArea);
            return this;
        }

        public ScenarioBuilder OfType(MeteringPointType type)
        {
            _meteringPointType = type;
            return this;
        }

        public ScenarioBuilder WithAdministratorId(Guid administratorId)
        {
            _administratorId = administratorId;
            return this;
        }

        public ScenarioBuilder WithAddress(Address address)
        {
            _address = address;
            return this;
        }

        public ScenarioBuilder WithGsrnNumber(GsrnNumber gsrnNumber)
        {
            _gsrnNumber = gsrnNumber;
            return this;
        }

        public ScenarioBuilder WithMeteringMethod(MeteringMethod meteringMethod)
        {
            _meteringMethod = meteringMethod;
            return this;
        }

        public ScenarioBuilder WithReadingOccurrence(ReadingOccurrence readingOccurrence)
        {
            _readingOccurrence = readingOccurrence;
            return this;
        }

        public ScenarioBuilder WithCapacity(string maxPower, string maxCurrent)
        {
            _maxPower = maxPower;
            _maxCurrent = maxCurrent;
            return this;
        }

        public ScenarioBuilder WithPowerPlantGsrnNumber(GsrnNumber gsrnNumber)
        {
            _powerPlantGsrnNumber = gsrnNumber;
            return this;
        }

        public ScenarioBuilder WithSettlementMethod(SettlementMethod settlementMethod)
        {
            _settlementMethod = settlementMethod;
            return this;
        }

        public ScenarioBuilder WithDisconnectionType(DisconnectionType disconnectionType)
        {
            _disconnectionType = disconnectionType;
            return this;
        }

        public ScenarioBuilder WithMeterNumber(MeterId meterId)
        {
            _meterId = meterId;
            return this;
        }

        public ScenarioBuilder WithNetSettlementGroup(NetSettlementGroup netSettlementGroup)
        {
            _netSettlementGroup = netSettlementGroup;
            return this;
        }

        public ScenarioBuilder WithConnectionType(ConnectionType connectionType)
        {
            _connectionType = connectionType;
            return this;
        }

        public ScenarioBuilder WithAssetType(AssetType assetType)
        {
            _assetType = assetType;
            return this;
        }

        public ScenarioBuilder OfExchangeMeteringPoint(string sourceGridArea, string targetGridArea)
        {
            _sourceGridArea = sourceGridArea;
            _targetGridArea = targetGridArea;
            _meteringPointType = MeteringPointType.Exchange;
            return this;
        }

        public ScenarioBuilder WithUnitType(MeasurementUnitType unitType)
        {
            _unitType = unitType;
            return this;
        }

        public ScenarioBuilder WithProductType(ProductType productType)
        {
            _productType = productType;
            return this;
        }

        public async Task BuildAsync()
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
            await SendCommandAsync(CreateCommand()).ConfigureAwait(false);
        }

        private async Task SendCommandAsync(object command, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        }

        private CreateMeteringPoint CreateCommand()
        {
            return new CreateMeteringPoint(
                MeteringPointType: _meteringPointType.Name,
                AdministratorId: _administratorId.ToString(),
                StreetName: _address?.StreetName,
                StreetCode: _address?.StreetCode!,
                BuildingNumber: _address?.BuildingNumber,
                PostCode: _address?.PostCode,
                CityName: _address?.City,
                CitySubDivisionName: _address?.CitySubDivision,
                MunicipalityCode: _address?.MunicipalityCode?.ToString(NumberFormatInfo.InvariantInfo)!,
                CountryCode: _address?.CountryCode?.Name!,
                FloorIdentification: _address?.Floor,
                RoomIdentification: _address?.Room,
                IsActualAddress: _address?.IsActual,
                GsrnNumber: _gsrnNumber?.Value!,
                MeteringMethod: _meteringMethod?.Name!,
                MeterReadingOccurrence: _readingOccurrence?.Name!,
                MaximumCurrent: _maxCurrent,
                MaximumPower: _maxPower,
                MeteringGridArea: _gridArea?.Code.Value!,
                PowerPlant: _powerPlantGsrnNumber?.Value,
                LocationDescription: _address?.LocationDescription,
                SettlementMethod: _settlementMethod?.Name,
                DisconnectionType: _disconnectionType?.Name,
                EffectiveDate: _effectiveDate,
                MeterNumber: _meterId?.Value,
                TransactionId: _transaction,
                NetSettlementGroup: _netSettlementGroup?.Name,
                ConnectionType: _connectionType?.Name,
                AssetType: _assetType?.Name,
                PhysicalConnectionCapacity: _physicalConnectionCapacity,
                GeoInfoReference: _address?.GeoInfoReference?.ToString(),
                ScheduledMeterReadingDate: _scheduledMeterReadingDate,
                ExchangeDetails: new ExchangeDetails(_sourceGridArea, _targetGridArea),
                ParentRelatedMeteringPoint: null,
                UnitType: _unitType?.Name,
                ProductType: _productType?.Name);
        }
    }
}
