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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings;
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
            ConverterMapperConfigurations.AssertConfigurationValid();
            Assert.True(true);
        }

        [Fact]
        public async Task ValidateValuesFromEachElementTest()
        {
            var xmlMapper = new XmlMapper((processType, type) => new CreateMeteringPointXmlMappingConfiguration());

            var xmlConverter = new XmlConverter(xmlMapper);

            var commandsRaw = await xmlConverter.DeserializeAsync(_xmlStream);
            var commands = commandsRaw.Cast<MeteringPoints.Application.CreateMeteringPoint>();

            var command = commands.First();

            Assert.Equal(nameof(MeteringPointType.Consumption), command.TypeOfMeteringPoint);
            Assert.Equal("571234567891234605", command.GsrnNumber);
            Assert.Equal(666, command.MaximumPower);
            Assert.Equal(nameof(MeasurementUnitType.KWh).ToLower(), command.UnitType.ToLower());
            Assert.Equal("571234567891234636", command.PowerPlant);
            Assert.Equal(nameof(SettlementMethod.Flex), command.SettlementMethod);
            Assert.Equal(nameof(MeteringPointType.Consumption), command.TypeOfMeteringPoint);
            Assert.Equal(nameof(MeteringPointSubType.Physical), command.SubTypeOfMeteringPoint);
            Assert.Equal(nameof(PhysicalState.New), command.PhysicalStatusOfMeteringPoint);
            Assert.Equal(nameof(ConnectionType.Direct), command.ConnectionType);
            Assert.Equal(nameof(AssetType.WindTurbines), command.AssetType);
            Assert.Equal(nameof(DisconnectionType.Remote), command.DisconnectionType);
            Assert.Equal(nameof(ReadingOccurrence.Hourly), command.MeterReadingOccurrence);

            Assert.Equal("3. Gadelygte fra højre", command.LocationDescription);
            Assert.Equal("1234567890", command.MeterNumber);
            Assert.Equal("2021-05-27T22:00:00.00Z", command.OccurenceDate);
            Assert.Equal("822", command.MeteringGridArea);
            Assert.Equal("99", command.NetSettlementGroup);
            Assert.Equal(666, command.MaximumCurrent);
            Assert.Equal("asdasweqweasedGUID", command.TransactionId);

            Assert.Null(command.ParentRelatedMeteringPoint);

            // // Main address
            // Assert.Equal("6000", command.InstallationLocationAddress.PostCode);
            //
            // // Street detail
            // Assert.Equal("Test street name", command.InstallationLocationAddress.StreetName);
            //
            // // Town detail
            // Assert.Equal("Test city", command.InstallationLocationAddress.CityName);
        }

        [Fact]
        public async Task ValidateTranslationOfCimXmlValuesToDomainSpecificValuesTest()
        {
            var xmlMapper = new XmlMapper((processType, type) => new CreateMeteringPointXmlMappingConfiguration());

            var xmlConverter = new XmlConverter(xmlMapper);
            var commandsRaw = await xmlConverter.DeserializeAsync(_xmlStream);
            var commands = commandsRaw.Cast<MeteringPoints.Application.CreateMeteringPoint>();

            var command = commands.First();

            Assert.Equal(nameof(SettlementMethod.Flex), command.SettlementMethod);
            Assert.Equal(nameof(MeteringPointType.Consumption), command.TypeOfMeteringPoint);
            Assert.Equal(nameof(MeteringPointSubType.Physical), command.SubTypeOfMeteringPoint);
            Assert.Equal(nameof(PhysicalState.New), command.PhysicalStatusOfMeteringPoint);
            Assert.Equal(nameof(ConnectionType.Direct), command.ConnectionType);
            Assert.Equal(nameof(AssetType.WindTurbines), command.AssetType);
            Assert.Equal(nameof(DisconnectionType.Remote), command.DisconnectionType);
            Assert.Equal(nameof(ReadingOccurrence.Hourly), command.MeterReadingOccurrence);
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
