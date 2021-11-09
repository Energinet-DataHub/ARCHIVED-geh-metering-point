using System;
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common.Address;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI
{
    public class BusinessDocumentFactory : IBusinessDocumentFactory
    {
        private const string XmlNamespace = "urn:ebix.org:structure:accountingpointcharacteristics:0:1";

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

        public void CreateAccountingPointCharacteristicsMessage(
            string requestTransactionId,
            MeteringPointDto meteringPointDto)
        {
            if (requestTransactionId == null) throw new ArgumentNullException(nameof(requestTransactionId));
            if (meteringPointDto == null) throw new ArgumentNullException(nameof(meteringPointDto));

            var accountingPointCharacteristicsMessage = new AccountingPointCharacteristicsMessage(
                Id: Guid.NewGuid().ToString(),
                Type: "E07",
                ProcessType: "D15",
                BusinessSectorType: "N/A",
                Sender: new MarketRoleParticipant(
                    Id: "DataHub GLN", // TODO: Use correct GLN
                    CodingScheme: "9",
                    Role: "DDZ"),
                Receiver: new MarketRoleParticipant(
                    Id: "consumptionMeteringPoint.,", // TODO: Get from energy supplier changed event-ish
                    CodingScheme: "9",
                    Role: "DDQ"),
                CreatedDateTime: _dateTimeProvider.Now(),
                MarketActivityRecord: new MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    BusinessProcessReference: _correlationContext.Id,
                    ValidityStartDateAndOrTime: "consumptionMeteringPoint.OccurenceDate", // TODO: Use occurence date (as effective date)
                    SnapshotDateAndOrTime: "N/A",
                    OriginalTransaction: requestTransactionId,
                    MarketEvaluationPoint: new MarketEvaluationPoint(
                        Id: new Mrid(meteringPointDto.GsrnNumber, "N/A"),
                        MeteringPointResponsibleMarketRoleParticipant: new MarketParticipant(
                            "GLN number of grid operator", "N/A"), // TODO: Update when grid operators are a thing.
                        Type: meteringPointDto.MeteringPointType,
                        SettlementMethod: meteringPointDto.SettlementMethod,
                        MeteringMethod: meteringPointDto.MeteringPointSubType,
                        ConnectionState: meteringPointDto.ConnectionState,
                        ReadCycle: meteringPointDto.ReadingOccurrence,
                        NetSettlementGroup: meteringPointDto.NetSettlementGroup,
                        NextReadingDate: "N/A",
                        MeteringGridAreaDomainId: new Mrid(meteringPointDto.GridAreaCode, "N/A"),
                        InMeteringGridAreaDomainId: new Mrid(meteringPointDto.FromGridAreaCode, "N/A"), // TODO: Only applicable for exchange
                        OutMeteringGridAreaDomainId: new Mrid(meteringPointDto.ToGridAreaCode, "N/A"), // TODO: Only applicable for exchange
                        LinkedMarketEvaluationPoint: new Mrid(meteringPointDto.PowerPlantGsrnNumber, "N/A"),
                        PhysicalConnectionCapacity: new UnitValue(
                            meteringPointDto.Capacity.HasValue
                                ? meteringPointDto.Capacity.Value.ToString(CultureInfo.InvariantCulture)
                                : string.Empty,
                            "N/A"),
                        ConnectionType: meteringPointDto.ConnectionType,
                        DisconnectionMethod: meteringPointDto.DisconnectionType,
                        AssetMarketPSRTypePsrType: meteringPointDto.AssetType,
                        ProductionObligation: false,
                        Series: new Series(
                            Id: "Id",
                            EstimatedAnnualVolumeQuantity: "EstimatedAnnualVolumeQuantity",
                            QuantityMeasureUnit: "QuantityMeasureUnit"),
                        ContractedConnectionCapacity: new UnitValue("ContractedConnectionCapacity", "Foo"),
                        RatedCurrent: new UnitValue(
                            meteringPointDto.MaximumCurrent.HasValue
                                ? meteringPointDto.MaximumCurrent.Value.ToString(CultureInfo.InvariantCulture)
                                : string.Empty,
                            "AMP"),
                        MeterId: "MeterId",
                        EnergySupplierMarketParticipantId: new MarketParticipant("EnergySupplierMarketParticipantId", "Foo"),
                        SupplyStartDateAndOrTimeDateTime: DateTime.Now,
                        Description: "Description",
                        UsagePointLocationMainAddress: new MainAddress(
                            StreetDetail: new StreetDetail(
                                Number: "Number",
                                Name: "Name",
                                Type: "Type",
                                Code: "Code",
                                BuildingName: "BuildingName",
                                SuiteNumber: "SuiteNumber",
                                FloorIdentification: "FloorIdentification"),
                            TownDetail: new TownDetail(
                                Code: "Code",
                                Section: "Section",
                                Name: "Name",
                                StateOrProvince: "StateOrProvince",
                                Country: "Country"),
                            Status: new Status(
                                Value: "Value",
                                DateTime: "DateTime",
                                Remark: "Remark",
                                Reason: "Reason"),
                            PostalCode: "PostalCode",
                            PoBox: "PoBox",
                            Language: "Language"),
                        UsagePointLocationActualAddressIndicator: false,
                        UsagePointLocationGeoInfoReference: "UsagePointLocationGeoInfoReference",
                        ParentMarketEvaluationPointId: new ParentMarketEvaluationPoint(
                            Id: "Id",
                            CodingScheme: "CodingScheme",
                            Description: "Description"),
                        ChildMarketEvaluationPoint: new ChildMarketEvaluationPoint(
                            Id: "Id",
                            CodingScheme: "CodingScheme"))));

            var serializedMessage = AccountingPointCharacteristicsXmlSerializer
                .Serialize(accountingPointCharacteristicsMessage, XmlNamespace);

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
