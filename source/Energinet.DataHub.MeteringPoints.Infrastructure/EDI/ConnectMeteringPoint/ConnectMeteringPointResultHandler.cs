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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common.Address;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint
{
    public sealed class ConnectMeteringPointResultHandler : IBusinessProcessResultHandler<Application.Connect.ConnectMeteringPoint>
    {
        private const string XmlNamespace = "urn:ebix.org:structure:accountingpointcharacteristics:0:1";

        private readonly ErrorMessageFactory _errorMessageFactory;
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ICorrelationContext _correlationContext;
        private readonly IMediator _mediator;
        private readonly ISystemDateTimeProvider _dateTimeProvider;

        public ConnectMeteringPointResultHandler(
            ErrorMessageFactory errorMessageFactory,
            IOutbox outbox,
            IOutboxMessageFactory outboxMessageFactory,
            IJsonSerializer jsonSerializer,
            ICorrelationContext correlationContext,
            IMediator mediator,
            ISystemDateTimeProvider dateTimeProvider)
        {
            _errorMessageFactory = errorMessageFactory;
            _outbox = outbox;
            _outboxMessageFactory = outboxMessageFactory;
            _jsonSerializer = jsonSerializer;
            _correlationContext = correlationContext;
            _mediator = mediator;
            _dateTimeProvider = dateTimeProvider;
        }

        public Task HandleAsync(Application.Connect.ConnectMeteringPoint request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return result.Success
                ? SuccessAsync(request, result)
                : RejectAsync(request, result);
        }

        private async Task SuccessAsync(Application.Connect.ConnectMeteringPoint request, BusinessProcessResult result)
        {
            var ediMessage = new ConnectMeteringPointAccepted(
                TransactionId: result.TransactionId,
                GsrnNumber: request.GsrnNumber,
                Status: "Accepted");

            var envelope = new PostOfficeEnvelope(string.Empty, string.Empty, _jsonSerializer.Serialize(ediMessage), "Accepted", _correlationContext.AsTraceContext());
            AddToOutbox(envelope);

            // TODO: check type, only send for consumption
            var meteringPoint = await _mediator.Send(new MeteringPointByGsrnQuery(request.GsrnNumber)).ConfigureAwait(false)
                                ?? throw new InvalidOperationException("Metering point not found");

            var accountingPointCharacteristicsMessage = new AccountingPointCharacteristicsMessage(
                Id: Guid.NewGuid().ToString(),
                Type: "TODO",
                ProcessType: "D15",
                BusinessSectorType: "TODO",
                Sender: new MarketRoleParticipant(
                    Id: "DataHub GLN", // TODO: Use correct GLN
                    CodingScheme: "9",
                    Role: "EZ"),
                Receiver: new MarketRoleParticipant(
                    Id: "TODO",
                    CodingScheme: "9",
                    Role: "DDQ"),
                CreatedDateTime: _dateTimeProvider.Now(),
                MarketActivityRecord: new MarketActivityRecord(
                    Id: "Id",
                    BusinessProcessReference: "BusinessProcessReference",
                    ValidityStartDateAndOrTime: "ValidityStartDateAndOrTime",
                    SnapshotDateAndOrTime: "SnapshotDateAndOrTime",
                    OriginalTransaction: "OriginalTransaction",
                    MarketEvaluationPoint: new MarketEvaluationPoint(
                        Id: new Mrid("Id", "Foo"),
                        MeteringPointResponsibleMarketRoleParticipant: new MarketParticipant("MeteringPointResponsibleMarketRoleParticipant", "Foo"),
                        Type: "Type",
                        SettlementMethod: "SettlementMethod",
                        MeteringMethod: "MeteringMethod",
                        ConnectionState: "ConnectionState",
                        ReadCycle: "ReadCycle",
                        NetSettlementGroup: "NetSettlementGroup",
                        NextReadingDate: "NextReadingDate",
                        MeteringGridAreaDomainId: new Mrid("MeteringGridAreaDomainId", "Foo"),
                        InMeteringGridAreaDomainId: new Mrid("InMeteringGridAreaDomainId", "Foo"),
                        OutMeteringGridAreaDomainId: new Mrid("OutMeteringGridAreaDomainId", "Foo"),
                        LinkedMarketEvaluationPoint: new Mrid("LinkedMarketEvaluationPoint", "Foo"),
                        PhysicalConnectionCapacity: new UnitValue("PhysicalConnectionCapacity", "Foo"),
                        ConnectionType: "ConnectionType",
                        DisconnectionMethod: "DisconnectionMethod",
                        AssetMarketPSRTypePsrType: "AssetMarketPSRTypePsrType",
                        ProductionObligation: false,
                        Series: new Series(
                            Id: "Id",
                            EstimatedAnnualVolumeQuantity: "EstimatedAnnualVolumeQuantity",
                            QuantityMeasureUnit: "QuantityMeasureUnit"),
                        ContractedConnectionCapacity: new UnitValue("ContractedConnectionCapacity", "Foo"),
                        RatedCurrent: new UnitValue("RatedCurrent", "Foo"),
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
                        UsagePointLocationOfficialAddressIndicator: false,
                        UsagePointLocationGeoInfoReference: "UsagePointLocationGeoInfoReference",
                        ParentMarketEvaluationPointId: new ParentMarketEvaluationPoint(
                            Id: "Id",
                            CodingScheme: "CodingScheme",
                            Description: "Description"),
                        ChildMarketEvaluationPoint: new ChildMarketEvaluationPoint(
                            Id: "Id",
                            CodingScheme: "CodingScheme"))));
            var serialized = AccountingPointCharacteristicsXmlSerializer.Serialize(accountingPointCharacteristicsMessage, XmlNamespace);
            var postOfficeEnvelope = new PostOfficeEnvelope(
                string.Empty,
                string.Empty,
                _jsonSerializer.Serialize(serialized),
                "accountingpointcharacteristics",
                _correlationContext.Id);
            AddToOutbox(postOfficeEnvelope);
        }

        private Task RejectAsync(Application.Connect.ConnectMeteringPoint request, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .ToArray();

            var ediMessage = new ConnectMeteringPointRejected(
                TransactionId: result.TransactionId,
                GsrnNumber: request.GsrnNumber,
                Status: "Rejected", // TODO: Is this necessary? Also, Reason?
                Reason: "TODO",
                Errors: errors);

            var envelope = new PostOfficeEnvelope(string.Empty, string.Empty, _jsonSerializer.Serialize(ediMessage), "Rejected", _correlationContext.AsTraceContext());
            AddToOutbox(envelope);

            return Task.CompletedTask;
        }

        private void AddToOutbox<TEdiMessage>(TEdiMessage ediMessage)
        {
            var outboxMessage = _outboxMessageFactory.CreateFrom(ediMessage, OutboxMessageCategory.ActorMessage);
            _outbox.Add(outboxMessage);
        }
    }
}
