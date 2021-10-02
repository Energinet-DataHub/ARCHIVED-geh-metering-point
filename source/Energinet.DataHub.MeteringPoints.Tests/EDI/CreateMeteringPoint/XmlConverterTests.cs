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
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.EDI.CreateMeteringPoint
{
    [UnitTest]
    public class XmlConverterTests
    {
        private Stream _xmlStream;

        public XmlConverterTests()
        {
            _xmlStream = GetResourceStream("CreateMeteringPointCimXml.xml");
        }

        [Fact]
        public void AssertConfigurationsValid()
        {
            Action assertConfigurationValid = ConverterMapperConfigurations.AssertConfigurationValid;
            assertConfigurationValid.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_Values_From_Each_Element_ConnectMeteringPointCimXml()
        {
            var xmlMapper = new XmlMapper((processType, type) => new ConnectMeteringPointXmlMappingConfiguration());

            var xmlConverter = new XmlDeserializer(xmlMapper);

            var stream = GetResourceStream("ConnectMeteringPointCimXml.xml");
            var commandsRaw = await xmlConverter.DeserializeAsync(stream).ConfigureAwait(false);
            var commands = commandsRaw.Cast<ConnectMeteringPoint>();

            var command = commands.First();

            command.GsrnNumber.Should().Be("571234567891234605");
            command.EffectiveDate.Should().Be("2021-05-27T22:00:00.00Z");
            command.TransactionId.Should().Be("asdasweqweasedGUID");
        }

        [Fact]
        public async Task ValidateValuesFromEachElementTest()
        {
            var xmlMapper = new XmlMapper((processType, type) => new CreateMeteringPointXmlMappingConfiguration());

            var xmlConverter = new XmlDeserializer(xmlMapper);

            var commandsRaw = await xmlConverter.DeserializeAsync(_xmlStream).ConfigureAwait(false);
            var commands = commandsRaw.Cast<MeteringPoints.Application.Create.CreateMeteringPoint>();

            var command = commands.First();

            command.TypeOfMeteringPoint.Should().Be(nameof(MeteringPointType.Consumption));
            command.GsrnNumber.Should().Be("571234567891234605");
            command.MaximumPower.Should().Be(2000);
            command.UnitType.ToUpperInvariant().Should().Be(nameof(MeasurementUnitType.KWh).ToUpperInvariant());
            command.PowerPlant.Should().Be("571234567891234636");
            command.SettlementMethod.Should().Be(nameof(SettlementMethod.Flex));
            command.TypeOfMeteringPoint.Should().Be(nameof(MeteringPointType.Consumption));
            command.MeteringMethod.Should().Be(nameof(MeteringMethod.Physical));
            command.PhysicalStatusOfMeteringPoint.Should().Be(nameof(PhysicalState.New));
            command.ConnectionType.Should().Be(nameof(ConnectionType.Direct));
            command.AssetType.Should().Be(nameof(AssetType.WindTurbines));
            command.DisconnectionType.Should().Be(nameof(DisconnectionType.Remote));
            command.MeterReadingOccurrence.Should().Be(nameof(ReadingOccurrence.Hourly));

            command.LocationDescription.Should().Be("String");
            command.MeterNumber.Should().Be("123456789");
            command.EffectiveDate.Should().Be("2021-07-13T22:00:00Z");
            command.MeteringGridArea.Should().Be("870");
            command.NetSettlementGroup.Should().Be("Zero");
            command.MaximumCurrent.Should().Be(5000);
            command.TransactionId.Should().Be("1234");
            command.PostCode.Should().Be("8000");
            command.StreetName.Should().Be("Test street name");
            command.CityName.Should().Be("12");
            command.CountryCode.Should().Be("DK");
            command.CitySubDivisionName.Should().Be("Test city");
            command.MunicipalityCode.Should().Be("12");

            command.FromGrid.Should().Be("869");
            command.ToGrid.Should().Be("871");
            Assert.True(command.IsOfficialAddress);
            Assert.Null(command.ParentRelatedMeteringPoint);
        }

        [Fact]
        public async Task ValidateTranslationOfCimXmlValuesToDomainSpecificValuesTest()
        {
            var xmlMapper = new XmlMapper((processType, type) => new CreateMeteringPointXmlMappingConfiguration());

            var xmlConverter = new XmlDeserializer(xmlMapper);
            var commandsRaw = await xmlConverter.DeserializeAsync(_xmlStream).ConfigureAwait(false);
            var commands = commandsRaw.Cast<MeteringPoints.Application.Create.CreateMeteringPoint>();

            var command = commands.First();

            command.SettlementMethod.Should().Be(nameof(SettlementMethod.Flex));
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
    }
}
