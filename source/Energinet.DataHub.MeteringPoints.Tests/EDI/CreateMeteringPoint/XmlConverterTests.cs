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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using Energinet.DataHub.Core.XmlConversion.XmlConverter;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Configuration;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.EDI.CreateMeteringPoint
{
    [UnitTest]
    public class XmlConverterTests
    {
        [Fact]
        public void AssertConfigurationsValid()
        {
            Action assertConfigurationValid = () => ConverterMapperConfigurations.AssertConfigurationValid(typeof(MasterDataDocument), typeof(MasterDataDocumentXmlMappingConfiguration).Assembly);
            assertConfigurationValid.Should().NotThrow();
        }

        [Fact]
        public async Task ValidateValuesFromEachElementTest()
        {
            var xmlMapper = new XmlMapper((type) => new MasterDataDocumentXmlMappingConfiguration(), (processType) => BusinessProcessType.CreateMeteringPoint.Name);
            var xmlConverter = new XmlDeserializer(xmlMapper);

            var (errors, element) = await ValidateAndReadXml().ConfigureAwait(false);

            errors.Should().BeEmpty();
            element.Should().NotBeNull();

            var deserializationResult = xmlConverter.Deserialize(element!);
            deserializationResult.HeaderData.Sender.Id.Should().Be("5799999933318");

            var commands = deserializationResult.Documents.Cast<MasterDataDocument>();

            var command = commands.First();

            command.TypeOfMeteringPoint.Should().Be(nameof(MeteringPointType.Consumption));
            command.GsrnNumber.Should().Be("579999993331812345");
            command.MaximumPower.Should().Be(230);
            command.MeasureUnitType.ToUpperInvariant().Should().Be(nameof(MeasurementUnitType.KWh).ToUpperInvariant());
            command.PowerPlant.Should().Be("579999993331812327");
            command.SettlementMethod.Should().Be(nameof(SettlementMethod.NonProfiled));
            command.TypeOfMeteringPoint.Should().Be(nameof(MeteringPointType.Consumption));
            command.MeteringMethod.Should().Be(nameof(MeteringMethod.Physical));
            command.PhysicalStatusOfMeteringPoint.Should().Be(nameof(PhysicalState.New));
            command.ConnectionType.Should().Be(nameof(ConnectionType.Direct));
            command.AssetType.Should().Be(nameof(AssetType.WindTurbines));
            command.DisconnectionType.Should().Be(nameof(DisconnectionType.Remote));
            command.MeterReadingOccurrence.Should().Be(nameof(ReadingOccurrence.Hourly));

            command.LocationDescription.Should().Be("3. bygning til venstre");
            command.MeterNumber.Should().Be("2536258974");
            command.EffectiveDate.Should().Be("2021-12-17T23:00:00Z");
            command.MeteringGridArea.Should().Be("244");
            command.NetSettlementGroup.Should().Be("Six");
            command.MaximumCurrent.Should().Be(32);
            command.TransactionId.Should().Be("25361487");
            command.PostCode.Should().Be("5500");
            command.StreetName.Should().Be("Vestergade");
            command.CityName.Should().Be("0625");
            command.CountryCode.Should().Be("DK");
            command.CitySubDivisionName.Should().Be("Middelfart");
            command.MunicipalityCode.Should().Be("0625");

            command.FromGrid.Should().Be("031");
            command.ToGrid.Should().Be("244");
            command.IsActualAddress.Should().BeTrue();
            command.ParentRelatedMeteringPoint.Should().Be("579999993331812345");
        }

        [Fact]
        public async Task ValidateTranslationOfCimXmlValuesToDomainSpecificValuesTest()
        {
            var xmlMapper = new XmlMapper((type) => new MasterDataDocumentXmlMappingConfiguration(), (processType) => BusinessProcessType.CreateMeteringPoint.Name);

            var xmlConverter = new XmlDeserializer(xmlMapper);

            var (errors, element) = await ValidateAndReadXml().ConfigureAwait(false);

            errors.Should().BeEmpty();
            element.Should().NotBeNull();

            var deserializationResult = xmlConverter.Deserialize(element!);
            var commands = deserializationResult.Documents.Cast<MasterDataDocument>();

            var command = commands.First();

            command.SettlementMethod.Should().Be(nameof(SettlementMethod.NonProfiled));
            command.TypeOfMeteringPoint.Should().Be(nameof(MeteringPointType.Consumption));
            command.MeteringMethod.Should().Be(nameof(MeteringMethod.Physical));
            command.PhysicalStatusOfMeteringPoint.Should().Be(nameof(PhysicalState.New));
            command.ConnectionType.Should().Be(nameof(ConnectionType.Direct));
            command.AssetType.Should().Be(nameof(AssetType.WindTurbines));
            command.DisconnectionType.Should().Be(nameof(DisconnectionType.Remote));
            command.MeterReadingOccurrence.Should().Be(nameof(ReadingOccurrence.Hourly));
        }

        private static Stream GetResourceStream(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = new List<string>(assembly.GetManifestResourceNames());

            var resourceName = resourcePath.Replace(@"/", ".", StringComparison.Ordinal);
            var resource = resourceNames.FirstOrDefault(r => r.Contains(resourceName, StringComparison.Ordinal))
                ?? throw new FileNotFoundException("Resource not found");

            return assembly.GetManifestResourceStream(resource)
                   ?? throw new InvalidOperationException($"Couldn't get requested resource: {resourcePath}");
        }

        private static async Task<(List<string> Errors, XElement? Element)> ValidateAndReadXml()
        {
            var reader = new SchemaValidatingReader(
                GetResourceStream("CreateMeteringPointCimXml.xml"),
                Schemas.CimXml.StructureRequestChangeAccountingPointCharacteristics);

            var element = await reader.AsXElementAsync().ConfigureAwait(false);

            return (reader.Errors.Select(x => $"{x.Description}:{x.LineNumber}-{x.LinePosition}").ToList(), element);
        }
    }
}
