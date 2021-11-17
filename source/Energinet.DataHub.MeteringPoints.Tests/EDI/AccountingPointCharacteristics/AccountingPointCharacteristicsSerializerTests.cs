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
using System.IO;
using System.Reflection;
using System.Text;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common.Address;
using Energinet.DataHub.MeteringPoints.Tests.Tooling;
using FluentAssertions;
using NodaTime.Extensions;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.EDI.AccountingPointCharacteristics
{
    [UnitTest]
    public class AccountingPointCharacteristicsSerializerTests
    {
        [RunnableInDebugOnly]
        public void Output_should_match_example_document()
        {
            var expected = GetExpectedDocument("AccountingPointCharacteristics.xml");
            var message = new AccountingPointCharacteristicsMessage(
                Id: "25365897",
                Type: "E07",
                ProcessType: "D15",
                BusinessSectorType: "23",
                Sender: new MarketRoleParticipant(
                    Id: "5790001330552",
                    CodingScheme: "A10",
                    Role: "DDZ"),
                Receiver: new MarketRoleParticipant(
                    Id: "5799999933318",
                    CodingScheme: "A10",
                    Role: "DDQ"),
                CreatedDateTime: DateTimeOffset.Parse("2021-12-17T09:30:47Z", CultureInfo.InvariantCulture).ToInstant(),
                MarketActivityRecord: new MarketActivityRecord(
                    Id: "2536982547",
                    ValidityStartDateAndOrTime: "2021-12-17T23:00:00Z",
                    OriginalTransaction: "25369847",
                    MarketEvaluationPoint: new MarketEvaluationPoint(
                        Id: new Mrid("579999993331812345", "A10"),
                        MeteringPointResponsibleMarketRoleParticipant: new MarketParticipant(
                            "5790001333421", "A10"), // TODO: Update when grid operators are a thing.
                        Type: "E17",
                        SettlementMethod: "E02",
                        MeteringMethod: "D01",
                        ConnectionState: "E23",
                        ReadCycle: "PT1H",
                        NetSettlementGroup: "6",
                        NextReadingDate: "1201",
                        MeteringGridAreaDomainId: new Mrid("031", "NDK"),
                        InMeteringGridAreaDomainId: new Mrid("151", "NDK"),
                        OutMeteringGridAreaDomainId: new Mrid("031", "NDK"),
                        LinkedMarketEvaluationPoint: "579999993331714215",
                        PhysicalConnectionCapacity: new UnitValue("1300", "KWT"),
                        ConnectionType: "D01",
                        DisconnectionMethod: "D01",
                        AssetMarketPSRTypePsrType: "D09",
                        ProductionObligation: false,
                        Series: new Series(
                            Product: "8716867000115",
                            QuantityMeasureUnit: "KWH"),
                        ContractedConnectionCapacity: new UnitValue("220", "KWT"),
                        RatedCurrent: new UnitValue(
                            "32",
                            "AMP"),
                        MeterId: "29746",
                        EnergySupplierMarketParticipantId: new MarketParticipant("5799999933318", "A10"),
                        SupplyStartDateAndOrTimeDateTime: DateTimeOffset.Parse("2019-02-11T23:00:00Z", CultureInfo.InvariantCulture).UtcDateTime,
                        Description: "pumpestation",
                        UsagePointLocationMainAddress: new MainAddress(
                            StreetDetail: new StreetDetail(
                                Number: "16",
                                Name: "Vestergade",
                                Code: "0304",
                                SuiteNumber: "2",
                                FloorIdentification: "2"),
                            TownDetail: new TownDetail(
                                Code: "0526",
                                Section: "Strib",
                                Name: "Middelfart",
                                Country: "DK"),
                            PostalCode: "5500"),
                        UsagePointLocationActualAddressIndicator: true,
                        UsagePointLocationGeoInfoReference: "f26f8678-6cd3-4e12-b70e-cf96290ada94",
                        ParentMarketEvaluationPoint: new ParentMarketEvaluationPoint(
                            Id: "579999993331812345"),
                        ChildMarketEvaluationPoint: new ChildMarketEvaluationPoint(
                            Id: "579999993331812325",
                            Description: "D06"))));

            var serialized = AccountingPointCharacteristicsXmlSerializer.Serialize(message);

#if DEBUG
            DebugOutput(serialized);
#endif

            serialized.Should().Be(expected);
        }

        [RunnableInDebugOnly]
        public void Generated_message_from_factory_should_match_example_document()
        {
            var expected = GetExpectedDocument("AccountingPointCharacteristics.xml");

            var id = "25365897";
            var transactionId = "25369847";
            var meteringPointDto = new MeteringPointDto(
                Guid.Empty,
                "579999993331812345",
                "Vestergade",
                "5500",
                "Middelfart",
                "DK",
                "E23",
                "D01",
                "PT1H",
                "E17",
                32,
                220,
                "foo",
                "031",
                "579999993331714215",
                "pumpestation",
                "8716867000115",
                "KWH",
                DateTimeOffset.Parse("2021-12-17T23:00:00Z", CultureInfo.InvariantCulture).UtcDateTime,
                "29746",
                "0304",
                "Strib",
                "2",
                "2",
                "16",
                526,
                true,
                Guid.Parse("f26f8678-6cd3-4e12-b70e-cf96290ada94"),
                1300,
                "E02",
                "6",
                "D09",
                "031",
                "151",
                "D01",
                "D01",
                false);
            var createdDate = DateTimeOffset.Parse("2021-12-17T09:30:47Z", CultureInfo.InvariantCulture).ToInstant();
            var supplyStartDate = DateTimeOffset.Parse("2019-02-12T00:00:00Z", CultureInfo.InvariantCulture).ToInstant();
            var energySupplier = new EnergySupplierDto("5799999933318", supplyStartDate);

            var message = BusinessDocumentFactory.MapAccountingPointCharacteristicsMessage(
                id: id,
                requestTransactionId: transactionId,
                businessReasonCode: BusinessReasonCodes.ConnectMeteringPoint,
                meteringPoint: meteringPointDto,
                energySupplier: energySupplier,
                createdDate: createdDate);

            var serialized = AccountingPointCharacteristicsXmlSerializer.Serialize(message);

#if DEBUG
            DebugOutput(serialized);
#endif

            serialized.Should().Be(expected);
        }

        private static void DebugOutput(string serialized)
        {
            File.WriteAllText(
                @"C:\code\geh-metering-point\source\Energinet.DataHub.MeteringPoints.Tests\EDI\AccountingPointCharacteristics\AccountingPointCharacteristicsSerializerTestOutput.xml",
                serialized,
                new UTF8Encoding(true));
        }

        private static string GetExpectedDocument(string documentName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream($"Energinet.DataHub.MeteringPoints.Tests.EDI.AccountingPointCharacteristics.{documentName}")!;
            using StreamReader reader = new(stream);
            var documentWithLicense = reader.ReadToEnd();

            // Note: because of the license check in ci/cd it's necessary to make some jumps through hoops
            var documentWithoutLicense = documentWithLicense.Remove(0, documentWithLicense.IndexOf("<cim", StringComparison.Ordinal));
            var expectedDocument = documentWithoutLicense.Insert(0, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");

            return expectedDocument;
        }
    }
}
