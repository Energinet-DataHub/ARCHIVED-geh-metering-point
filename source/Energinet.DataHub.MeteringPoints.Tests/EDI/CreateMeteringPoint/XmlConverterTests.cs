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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
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
        public async Task ValidateValuesFromEachElementTest()
        {
            var xmlMapper = new XmlMapper((processType, type) => new CreateMeteringPointXmlMappingConfiguration());

            var xmlConverter = new XmlConverter(xmlMapper);

            var commandsRaw = await xmlConverter.DeserializeAsync(_xmlStream);
            var commands = commandsRaw.Cast<MeteringPoints.Application.CreateMeteringPoint>();

            var command = commands.First();

            command.TypeOfMeteringPoint.Should().Be(nameof(MeteringPointType.Consumption));
            command.GsrnNumber.Should().Be("571234567891234605");
            command.MaximumPower.Should().Be(666);
            command.UnitType.ToLower().Should().Be(nameof(MeasurementUnitType.KWh).ToLower());
            command.PowerPlant.Should().Be("571234567891234636");
            command.SettlementMethod.Should().Be(nameof(SettlementMethod.Flex));
            command.TypeOfMeteringPoint.Should().Be(nameof(MeteringPointType.Consumption));
            command.SubTypeOfMeteringPoint.Should().Be(nameof(MeteringPointSubType.Physical));
            command.PhysicalStatusOfMeteringPoint.Should().Be(nameof(PhysicalState.New));
            command.ConnectionType.Should().Be(nameof(ConnectionType.Direct));
            command.AssetType.Should().Be(nameof(AssetType.WindTurbines));
            command.DisconnectionType.Should().Be(nameof(DisconnectionType.Remote));
            command.MeterReadingOccurrence.Should().Be(nameof(ReadingOccurrence.Hourly));

            command.LocationDescription.Should().Be("D01");
            command.MeterNumber.Should().Be("1234567890");
            command.OccurenceDate.Should().Be("2021-05-27T22:00:00.00Z");
            command.MeteringGridArea.Should().Be("822");
            command.NetSettlementGroup.Should().Be("99");
            command.MaximumCurrent.Should().Be(666);
            command.TransactionId.Should().Be("asdasweqweasedGUID");
            command.PostCode.Should().Be("6000");
            command.StreetName.Should().Be("Test street name");
            command.CityName.Should().Be("Test city");
            command.CountryCode.Should().Be("DK");
            Assert.True(command.IsWashable);
            Assert.Null(command.ParentRelatedMeteringPoint);
        }

        [Fact]
        public async Task ValidateTranslationOfCimXmlValuesToDomainSpecificValuesTest()
        {
            var xmlMapper = new XmlMapper((processType, type) => new CreateMeteringPointXmlMappingConfiguration());

            var xmlConverter = new XmlConverter(xmlMapper);
            var commandsRaw = await xmlConverter.DeserializeAsync(_xmlStream);
            var commands = commandsRaw.Cast<MeteringPoints.Application.CreateMeteringPoint>();

            var command = commands.First();

            command.SettlementMethod.Should().Be(nameof(SettlementMethod.Flex));
            command.TypeOfMeteringPoint.Should().Be(nameof(MeteringPointType.Consumption));
            command.SubTypeOfMeteringPoint.Should().Be(nameof(MeteringPointSubType.Physical));
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

            resourcePath = resourcePath.Replace(@"/", ".");
            resourcePath = resourceNames.FirstOrDefault(r => r.Contains(resourcePath));

            if (resourcePath == null)
            {
                throw new FileNotFoundException("Resource not found");
            }

            return assembly.GetManifestResourceStream(resourcePath);
        }
    }
}
