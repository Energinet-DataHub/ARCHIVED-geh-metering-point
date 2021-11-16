using System;
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
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
                Type: "E07", // TODO: Hardcoded, correct?
                ProcessType: businessReasonCode,
                BusinessSectorType: "23", // Hardcoded: Electricity supply industry
                Sender: new MarketRoleParticipant(
                    Id: "5790001330552", // TODO: Fix hardcoded Energinet GLN
                    CodingScheme: "A10", // TODO: Hardcoded, correct?
                    Role: "DDZ"), // TODO: Hardcoded, correct?
                Receiver: new MarketRoleParticipant(
                    Id: energySupplier.GlnNumber,
                    CodingScheme: "A10", // TODO: Hardcoded, correct?
                    Role: "DDQ"), // TODO: Hardcoded, correct?
                CreatedDateTime: createdDate,
                MarketActivityRecord: new MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(), // TODO: Generated, correct?
                    ValidityStartDateAndOrTime: meteringPoint.EffectiveDate!.Value.ToUtcString(),
                    OriginalTransaction: requestTransactionId,
                    MarketEvaluationPoint: new MarketEvaluationPoint(
                        Id: new Mrid(meteringPoint.GsrnNumber, "A10"), // TODO: Hardcoded, correct?
                        MeteringPointResponsibleMarketRoleParticipant: new MarketParticipant(
                            "GLN number of grid operator", "A10"), // TODO: Update when grid operators are a thing. And codingScheme, correct?
                        Type: meteringPoint.MeteringPointType,
                        SettlementMethod: meteringPoint.SettlementMethod,
                        MeteringMethod: meteringPoint.MeteringPointSubType,
                        ConnectionState: meteringPoint.ConnectionState,
                        ReadCycle: meteringPoint.ReadingOccurrence,
                        NetSettlementGroup: meteringPoint.NetSettlementGroup,
                        NextReadingDate: "N/A", // TODO: What is this?
                        MeteringGridAreaDomainId: new Mrid(meteringPoint.GridAreaCode, "NDK"),
                        InMeteringGridAreaDomainId: meteringPoint.FromGridAreaCode != null ? new Mrid(meteringPoint.FromGridAreaCode!, "NDK") : null!,
                        OutMeteringGridAreaDomainId: meteringPoint.ToGridAreaCode != null ? new Mrid(meteringPoint.ToGridAreaCode!, "NDK") : null!,
                        LinkedMarketEvaluationPoint: meteringPoint.PowerPlantGsrnNumber,
                        PhysicalConnectionCapacity: new UnitValue(
                            meteringPoint.Capacity.HasValue
                                ? meteringPoint.Capacity.Value.ToString(CultureInfo.InvariantCulture)
                                : string.Empty,
                            "KWT"), // TODO: Hardcoded, correct?
                        ConnectionType: meteringPoint.ConnectionType,
                        DisconnectionMethod: meteringPoint.DisconnectionType,
                        AssetMarketPSRTypePsrType: meteringPoint.AssetType,
                        ProductionObligation: false,
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
                        EnergySupplierMarketParticipantId: new MarketParticipant(energySupplier.GlnNumber, "A10"), // TODO: Hardcoded, correct?
                        SupplyStartDateAndOrTimeDateTime: DateTime.MinValue, // TODO: EnergySupplier_StartOfSupply?
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
                            Id: "579999993331812345"), // TODO: Hardcoded
                        ChildMarketEvaluationPoint: new ChildMarketEvaluationPoint(
                            Id: "579999993331812325", // TODO: Hardcoded
                            Description: "D06")))); // TODO: Hardcoded

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
