using System;
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.EnergySuppliers;
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
            MeteringPointDto meteringPointDto,
            GlnNumber energySupplierGlnNumber,
            Instant createdDate)
        {
            if (requestTransactionId == null) throw new ArgumentNullException(nameof(requestTransactionId));
            if (meteringPointDto == null) throw new ArgumentNullException(nameof(meteringPointDto));
            if (energySupplierGlnNumber == null) throw new ArgumentNullException(nameof(energySupplierGlnNumber));

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
                    Id: energySupplierGlnNumber.Value,
                    CodingScheme: "A10", // TODO: Hardcoded, correct?
                    Role: "DDQ"), // TODO: Hardcoded, correct?
                CreatedDateTime: createdDate,
                MarketActivityRecord: new MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(), // TODO: Generated, correct?
                    ValidityStartDateAndOrTime: meteringPointDto.EffectiveDate!.Value.ToUtcString(),
                    OriginalTransaction: requestTransactionId,
                    MarketEvaluationPoint: new MarketEvaluationPoint(
                        Id: new Mrid(meteringPointDto.GsrnNumber, "A10"), // TODO: Hardcoded, correct?
                        MeteringPointResponsibleMarketRoleParticipant: new MarketParticipant(
                            "GLN number of grid operator", "A10"), // TODO: Update when grid operators are a thing. And codingScheme, correct?
                        Type: meteringPointDto.MeteringPointType,
                        SettlementMethod: meteringPointDto.SettlementMethod,
                        MeteringMethod: meteringPointDto.MeteringPointSubType,
                        ConnectionState: meteringPointDto.ConnectionState,
                        ReadCycle: meteringPointDto.ReadingOccurrence,
                        NetSettlementGroup: meteringPointDto.NetSettlementGroup,
                        NextReadingDate: "N/A", // TODO: What is this?
                        MeteringGridAreaDomainId: new Mrid(meteringPointDto.GridAreaCode, "NDK"),
                        InMeteringGridAreaDomainId: meteringPointDto.FromGridAreaCode != null ? new Mrid(meteringPointDto.FromGridAreaCode!, "NDK") : null!,
                        OutMeteringGridAreaDomainId: meteringPointDto.ToGridAreaCode != null ? new Mrid(meteringPointDto.ToGridAreaCode!, "NDK") : null!,
                        LinkedMarketEvaluationPoint: meteringPointDto.PowerPlantGsrnNumber,
                        PhysicalConnectionCapacity: new UnitValue(
                            meteringPointDto.Capacity.HasValue
                                ? meteringPointDto.Capacity.Value.ToString(CultureInfo.InvariantCulture)
                                : string.Empty,
                            "KWT"), // TODO: Hardcoded, correct?
                        ConnectionType: meteringPointDto.ConnectionType,
                        DisconnectionMethod: meteringPointDto.DisconnectionType,
                        AssetMarketPSRTypePsrType: meteringPointDto.AssetType,
                        ProductionObligation: false,
                        Series: new Series(
                            Product: meteringPointDto.Product,
                            QuantityMeasureUnit: meteringPointDto.UnitType),
                        ContractedConnectionCapacity: new UnitValue(
                            meteringPointDto.MaximumPower.HasValue
                                ? meteringPointDto.MaximumPower.Value.ToString(CultureInfo.InvariantCulture)
                                : string.Empty,
                            "KWT"),
                        RatedCurrent: new UnitValue(
                            meteringPointDto.MaximumCurrent.HasValue
                                ? meteringPointDto.MaximumCurrent.Value.ToString(CultureInfo.InvariantCulture)
                                : string.Empty,
                            "AMP"),
                        MeterId: meteringPointDto.MeterNumber,
                        EnergySupplierMarketParticipantId: new MarketParticipant(energySupplierGlnNumber.Value, "A10"), // TODO: Hardcoded, correct?
                        SupplyStartDateAndOrTimeDateTime: meteringPointDto.EffectiveDate.Value,
                        Description: meteringPointDto.LocationDescription,
                        UsagePointLocationMainAddress: new MainAddress(
                            StreetDetail: new StreetDetail(
                                Number: meteringPointDto.BuildingNumber,
                                Name: meteringPointDto.StreetName,
                                Code: meteringPointDto.StreetCode,
                                SuiteNumber: meteringPointDto.Suite,
                                FloorIdentification: meteringPointDto.Floor),
                            TownDetail: new TownDetail(
                                Code: $"{meteringPointDto.MunicipalityCode:0000}",
                                Section: meteringPointDto.CitySubDivisionName,
                                Name: meteringPointDto.CityName,
                                Country: meteringPointDto.CountryCode),
                            PostalCode: meteringPointDto.PostCode),
                        UsagePointLocationActualAddressIndicator: meteringPointDto.IsActualAddress ?? false,
                        UsagePointLocationGeoInfoReference: "f26f8678-6cd3-4e12-b70e-cf96290ada94",
                        ParentMarketEvaluationPoint: new ParentMarketEvaluationPoint(
                            Id: "579999993331812345"),
                        ChildMarketEvaluationPoint: new ChildMarketEvaluationPoint(
                            Id: "579999993331812325",
                            Description: "D06"))));

            return accountingPointCharacteristicsMessage;
        }

        public void CreateAccountingPointCharacteristicsMessage(
            string transactionId,
            string businessReasonCode,
            MeteringPointDto meteringPointDto,
            GlnNumber energySupplierGlnNumber)
        {
            var accountingPointCharacteristicsMessage = MapAccountingPointCharacteristicsMessage(
                Guid.NewGuid().ToString(),
                transactionId,
                businessReasonCode,
                meteringPointDto,
                energySupplierGlnNumber,
                _dateTimeProvider.Now());

            var serializedMessage = AccountingPointCharacteristicsXmlSerializer
                .Serialize(accountingPointCharacteristicsMessage);

            var messageHubEnvelope = new MessageHubEnvelope(
                string.Empty,
                _jsonSerializer.Serialize(serializedMessage),
                DocumentType.AccountingPointCharacteristicsMessage,
                _correlationContext.Id,
                meteringPointDto.GsrnNumber);

            AddToOutbox(messageHubEnvelope);
        }

        private void AddToOutbox<TEdiMessage>(TEdiMessage ediMessage)
        {
            var outboxMessage = _outboxMessageFactory.CreateFrom(ediMessage, OutboxMessageCategory.ActorMessage);
            _outbox.Add(outboxMessage);
        }
    }
}
