﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        public async Task ValidateValuesFromEachElementTest()
        {
            var configurations = new List<XmlMappingConfigurationBase>
            {
                new CreateMeteringPointXmlMappingConfiguration(),
            }.ToImmutableList();

            var xmlConverter = new XmlConverter(configurations);

            var commandsRaw = await xmlConverter.DeserializeAsync(_xmlStream);
            var commands = commandsRaw.Cast<MeteringPoints.Application.CreateMeteringPoint>();

            var command = commands.First();

            // MarketEvaluationPoint
            Assert.Equal("571234567891234605", command.GsrnNumber);

            // Contracted Connection Capacity
            Assert.Equal(666, command.MaximumPower);

            // Series
            Assert.Equal("kWh", command.UnitType);

            // Linked Market EvaluationPoint aka Power Plant
            Assert.Equal("571234567891234636", command.PowerPlant);

            // // Main address
            // Assert.Equal("6000", command.InstallationLocationAddress.PostCode);
            //
            // // Street detail
            // Assert.Equal("Test street name", command.InstallationLocationAddress.StreetName);
            //
            // // Town detail
            // Assert.Equal("Test city", command.InstallationLocationAddress.CityName);
        }

        // [Fact]
        // public async Task ValidateTranslateSettlementMethodTest()
        // {
        //     var configurations = new List<XmlMappingConfigurationBase>
        //     {
        //         new CreateMeteringPointXmlMappingConfiguration(),
        //     }.ToImmutableList();
        //
        //     var xmlConverter = new XmlConverter(configurations);
        //     var commandsRaw = await xmlConverter.DeserializeAsync(_xmlStream);
        //     var commands = commandsRaw.Cast<MeteringPoints.Application.CreateMeteringPoint>();
        //
        //     var command = commands.First();
        //
        //     // Validate that we translate "D01" to "Flex"
        //     Assert.Equal("Flex", command.SettlementMethod);
        // }
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
