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
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Domain.Extensions;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common.Address;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI
{
    public class BusinessDocumentFactory : IBusinessDocumentFactory
    {
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ICorrelationContext _correlationContext;
        private readonly ISystemDateTimeProvider _dateTimeProvider;

        public BusinessDocumentFactory(
            IOutbox outbox,
            IOutboxMessageFactory outboxMessageFactory,
            IJsonSerializer jsonSerializer,
            ICorrelationContext correlationContext,
            ISystemDateTimeProvider dateTimeProvider)
        {
            _outbox = outbox;
            _outboxMessageFactory = outboxMessageFactory;
            _jsonSerializer = jsonSerializer;
            _correlationContext = correlationContext;
            _dateTimeProvider = dateTimeProvider;
        }

        public static AccountingPointCharacteristicsMessage MapAccountingPointCharacteristicsMessage(
            string id,
            string requestTransactionId,
            string businessReasonCode,
            MeteringPointDto meteringPoint,
            EnergySupplierDto energySupplier,
            Instant createdDate)
        {
            if (requestTransactionId == null) throw new ArgumentNullException(nameof(requestTransactionId));
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));
            if (energySupplier == null) throw new ArgumentNullException(nameof(energySupplier));

            var accountingPointCharacteristicsMessage = new AccountingPointCharacteristicsMessage(
                Id: id,
                Type: "E07",
                ProcessType: businessReasonCode,
                BusinessSectorType: "23",
                Sender: new MarketRoleParticipant(
                    Id: "5790001330552", // TODO: Fix hardcoded Energinet GLN, Actor register?
                    CodingScheme: "A10", // TODO: A10 = GLN, A01 = EIC, Actor register
                    Role: "DDZ"), // TODO: Actor register? DDZ is correct for now for this document.
                Receiver: new MarketRoleParticipant(
                    Id: energySupplier.GlnNumber,
                    CodingScheme: "A10", // TODO: A10 = GLN, A01 = EIC, Actor register
                    Role: "DDQ"), // TODO: Actor register, this will differ for other RSM22 scenarios
                CreatedDateTime: createdDate,
                MarketActivityRecord: new MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    ValidityStartDateAndOrTime: meteringPoint.EffectiveDate!.Value.ToUtcString(),
                    OriginalTransaction: requestTransactionId,
                    MarketEvaluationPoint: new MarketEvaluationPoint(
                        Id: new Mrid(meteringPoint.GsrnNumber, "A10"),
                        MeteringPointResponsibleMarketRoleParticipant: new MarketParticipant(
                            "GLN number of grid operator", "A10"), // TODO: Update when grid operators are a thing. And codingScheme from Actor Register
                        Type: meteringPoint.MeteringPointType,
                        SettlementMethod: meteringPoint.SettlementMethod,
                        MeteringMethod: meteringPoint.MeteringPointSubType,
                        ConnectionState: meteringPoint.PhysicalState,
                        ReadCycle: meteringPoint.ReadingOccurrence,
                        NetSettlementGroup: meteringPoint.NetSettlementGroup,
                        NextReadingDate: "N/A", // TODO: Only for netsettlement group 6, format: "MMdd"
                        MeteringGridAreaDomainId: new Mrid(meteringPoint.GridAreaCode, "NDK"),
                        InMeteringGridAreaDomainId: meteringPoint.FromGridAreaCode != null ? new Mrid(meteringPoint.FromGridAreaCode!, "NDK") : null!,
                        OutMeteringGridAreaDomainId: meteringPoint.ToGridAreaCode != null ? new Mrid(meteringPoint.ToGridAreaCode!, "NDK") : null!,
                        LinkedMarketEvaluationPoint: meteringPoint.PowerPlantGsrnNumber,
                        PhysicalConnectionCapacity: new UnitValue(
                            meteringPoint.Capacity.HasValue
                                ? meteringPoint.Capacity.Value.ToString(CultureInfo.InvariantCulture)
                                : string.Empty,
                            "KWT"),
                        ConnectionType: meteringPoint.ConnectionType,
                        DisconnectionMethod: meteringPoint.DisconnectionType,
                        AssetMarketPSRTypePsrType: meteringPoint.AssetType,
                        ProductionObligation: meteringPoint.ProductionObligation ?? false,
                        Series: new Series(
                            Product: meteringPoint.Product,
                            QuantityMeasureUnit: meteringPoint.UnitType),
                        ContractedConnectionCapacity: new UnitValue(
                            meteringPoint.MaximumPower.HasValue
                                ? meteringPoint.MaximumPower.Value.ToString(CultureInfo.InvariantCulture)
                                : string.Empty,
                            "KWT"),
                        RatedCurrent: new UnitValue(
                            meteringPoint.MaximumCurrent.HasValue
                                ? meteringPoint.MaximumCurrent.Value.ToString(CultureInfo.InvariantCulture)
                                : string.Empty,
                            "AMP"),
                        MeterId: meteringPoint.MeterNumber,
                        EnergySupplierMarketParticipantId: new MarketParticipant(energySupplier.GlnNumber, "A10"), // TODO: Actor register
                        SupplyStartDateAndOrTimeDateTime: energySupplier.StartOfSupplyDate.AsDateString(),
                        Description: meteringPoint.LocationDescription,
                        UsagePointLocationMainAddress: new MainAddress(
                            StreetDetail: new StreetDetail(
                                Number: meteringPoint.BuildingNumber,
                                Name: meteringPoint.StreetName,
                                Code: meteringPoint.StreetCode,
                                SuiteNumber: meteringPoint.Suite,
                                FloorIdentification: meteringPoint.Floor),
                            TownDetail: new TownDetail(
                                Code: $"{meteringPoint.MunicipalityCode:0000}",
                                Section: meteringPoint.CitySubDivisionName,
                                Name: meteringPoint.CityName,
                                Country: meteringPoint.CountryCode),
                            PostalCode: meteringPoint.PostCode),
                        UsagePointLocationActualAddressIndicator: meteringPoint.IsActualAddress ?? false,
                        UsagePointLocationGeoInfoReference: meteringPoint.GeoInfoReference.HasValue ? meteringPoint.GeoInfoReference.ToString()! : string.Empty,
                        ParentMarketEvaluationPoint: new ParentMarketEvaluationPoint(
                            Id: "579999993331812345"), // TODO: Hardcoded, not implemented yet
                        // Only for BRS 5, request stam data, possible multiple
                        ChildMarketEvaluationPoint: new ChildMarketEvaluationPoint(
                            Id: "579999993331812325", // TODO: Hardcoded, not implemented yet
                            Description: "D06")))); // TODO: Hardcoded, not implemented yet

            return accountingPointCharacteristicsMessage;
        }

        public void CreateAccountingPointCharacteristicsMessage(
            string transactionId,
            string businessReasonCode,
            MeteringPointDto meteringPoint,
            EnergySupplierDto energySupplier)
        {
            var accountingPointCharacteristicsMessage = MapAccountingPointCharacteristicsMessage(
                Guid.NewGuid().ToString(),
                transactionId,
                businessReasonCode,
                meteringPoint,
                energySupplier,
                _dateTimeProvider.Now());

            var serializedMessage = AccountingPointCharacteristicsXmlSerializer
                .Serialize(accountingPointCharacteristicsMessage);

            var messageHubEnvelope = new MessageHubEnvelope(
                string.Empty,
                _jsonSerializer.Serialize(serializedMessage),
                DocumentType.AccountingPointCharacteristicsMessage,
                _correlationContext.Id,
                meteringPoint.GsrnNumber);

            AddToOutbox(messageHubEnvelope);
        }

        private void AddToOutbox<TEdiMessage>(TEdiMessage ediMessage)
        {
            var outboxMessage = _outboxMessageFactory.CreateFrom(ediMessage, OutboxMessageCategory.ActorMessage);
            _outbox.Add(outboxMessage);
        }
    }
}
