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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Confirm;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Reject;
using FluentAssertions;
using NodaTime.Extensions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.EDI.Acknowledgements
{
    [UnitTest]
    public class AcknowledgementSerializerTests
    {
        [Fact]
        public async Task Confirm_message_should_validate_against_schema()
        {
            var message = ConfirmMessageFactory.CreateMeteringPoint(
                sender: new MarketRoleParticipant(
                    Id: "5790001330552",
                    CodingScheme: "A10",
                    Role: "DDZ"),
                receiver: new MarketRoleParticipant(
                    Id: "5799999933318",
                    CodingScheme: "A10",
                    Role: "DDQ"),
                createdDateTime: DateTimeOffset.Parse("2021-12-17T09:30:47Z", CultureInfo.InvariantCulture).ToInstant(),
                marketActivityRecord: new MarketActivityRecord(
                    Id: "25369814",
                    OriginalTransaction: "25369814",
                    MarketEvaluationPoint: "579999993331812345"));

            var serialized = new ConfirmMessageXmlSerializer().Serialize(message);
            var bytes = Encoding.ASCII.GetBytes(serialized);
            var stream = new MemoryStream(bytes);

            var schemaValidationReader = new SchemaValidatingReader(
                stream,
                Schemas.CimXml.StructureConfirmRequestChangeOfAccountingPointCharacteristics);

            while (await schemaValidationReader.AdvanceAsync().ConfigureAwait(false)) { }

            schemaValidationReader.HasErrors.Should().BeFalse(Because(schemaValidationReader));
        }

        [Fact]
        public async Task Reject_message_should_validate_against_schema()
        {
            var message = RejectMessageFactory.CreateMeteringPoint(
                sender: new MarketRoleParticipant(
                    Id: "5790001330552",
                    CodingScheme: "A10",
                    Role: "DDZ"),
                receiver: new MarketRoleParticipant(
                    Id: "5799999933318",
                    CodingScheme: "A10",
                    Role: "DDQ"),
                createdDateTime: DateTimeOffset.Parse("2021-12-17T09:30:47Z", CultureInfo.InvariantCulture).ToInstant(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: "25369814",
                    OriginalTransaction: "25369814",
                    MarketEvaluationPoint: "579999993331812345",
                    new[] { new Reason("D18", "foo") }));

            var serialized = new RejectMessageXmlSerializer().Serialize(message);
            var bytes = Encoding.ASCII.GetBytes(serialized);
            var stream = new MemoryStream(bytes);

            var schemaValidationReader = new SchemaValidatingReader(
                stream,
                Schemas.CimXml.StructureRejectRequestChangeOfAccountingPointCharacteristics);

            while (await schemaValidationReader.AdvanceAsync().ConfigureAwait(false)) { }

            schemaValidationReader.HasErrors.Should().BeFalse(Because(schemaValidationReader));
        }

        [Fact]
        public async Task Reject_message_with_multiple_errors_should_validate_against_schema()
        {
            var message = RejectMessageFactory.CreateMeteringPoint(
                sender: new MarketRoleParticipant(
                    Id: "5790001330552",
                    CodingScheme: "A10",
                    Role: "DDZ"),
                receiver: new MarketRoleParticipant(
                    Id: "5799999933318",
                    CodingScheme: "A10",
                    Role: "DDQ"),
                createdDateTime: DateTimeOffset.Parse("2021-12-17T09:30:47Z", CultureInfo.InvariantCulture).ToInstant(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: "25369814",
                    OriginalTransaction: "25369814",
                    MarketEvaluationPoint: "579999993331812345",
                    new[]
                    {
                        new Reason("D18", "foo"),
                        new Reason("D19", "bar"),
                    }));

            var serialized = new RejectMessageXmlSerializer().Serialize(message);
            var bytes = Encoding.ASCII.GetBytes(serialized);
            var stream = new MemoryStream(bytes);

            var schemaValidationReader = new SchemaValidatingReader(
                stream,
                Schemas.CimXml.StructureRejectRequestChangeOfAccountingPointCharacteristics);

            while (await schemaValidationReader.AdvanceAsync().ConfigureAwait(false)) { }

            schemaValidationReader.HasErrors.Should().BeFalse(Because(schemaValidationReader));
        }

        private static string Because(SchemaValidatingReader schemaValidationReader)
        {
            var errors = schemaValidationReader.Errors.Select(error => $"{error.LineNumber}:{error.LinePosition} : {error.Description}");
            return $"these errors shouldn't happen:{Environment.NewLine}: {string.Join(Environment.NewLine, errors)}";
        }
    }
}
