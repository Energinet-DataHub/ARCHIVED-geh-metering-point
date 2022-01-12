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
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.AccountingPointCharacteristics;
using FluentAssertions;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.EDI.AccountingPointCharacteristics
{
    [UnitTest]
    public class AccountingPointCharacteristicsSerializerTests
    {
        [Fact(Skip = "Not required yet.")]
        public async Task Message_should_validate_against_schema()
        {
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
                Instant.FromDateTimeUtc(new DateTime(2021, 12, 17, 23, 00, 00, DateTimeKind.Utc)),
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
                "D09",
                "E02",
                "031",
                "151",
                "6",
                Instant.FromDateTimeUtc(new DateTime(2019, 02, 12, 00, 00, 00, DateTimeKind.Utc)),
                "D01",
                "D01",
                false,
                null);
            var createdDate = DateTimeOffset.Parse("2021-12-17T09:30:47Z", CultureInfo.InvariantCulture).ToInstant();
            var supplyStartDate = DateTimeOffset.Parse("2019-02-12T00:00:00Z", CultureInfo.InvariantCulture).ToInstant();
            var energySupplier = new EnergySupplierDto("5799999933318", supplyStartDate);

            var message = AccountingPointCharacteristicsMessageFactory.Create(
                transactionId: transactionId,
                businessReasonCode: BusinessReasonCodes.ConnectMeteringPoint,
                meteringPoint: meteringPointDto,
                energySupplier: energySupplier,
                createdDate: createdDate,
                sender: new MarketRoleParticipant(
                    Id: "5790001330552",
                    CodingScheme: "A10",
                    Role: "DDZ"),
                receiver: new MarketRoleParticipant(
                    Id: "5799999933318",
                    CodingScheme: "A10",
                    Role: "DDQ"));

            var serialized = new AccountingPointCharacteristicsMessageXmlSerializer().Serialize(message);

            var bytes = Encoding.ASCII.GetBytes(serialized);
            var stream = new MemoryStream(bytes);

            var schemaValidationReader = new SchemaValidatingReader(
                stream,
                Schemas.CimXml.StructureAccountingPointCharacteristics);

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
