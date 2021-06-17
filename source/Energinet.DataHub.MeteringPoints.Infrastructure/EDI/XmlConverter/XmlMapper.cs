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
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter
{
    public class XmlMapper
    {
        private readonly ImmutableList<XmlMappingConfigurationBase> _configurations;

        public XmlMapper(ImmutableList<XmlMappingConfigurationBase> configurations)
        {
            _configurations = configurations;
        }

        public IEnumerable<IOutboundMessage> Map(XElement rootElement)
        {
            XNamespace ns = rootElement.FirstAttribute?.Value ?? throw new Exception("Found no namespace for XML Document");

            var headerData = MapHeaderData(rootElement, ns);

            XmlMappingConfigurationBase currentMappingConfiguration;

            switch (headerData.ProcessType)
            {
                case "E02":
                    currentMappingConfiguration = new CreateMeteringPointXmlMappingConfiguration();
                    break;

                default:
                    throw new NotImplementedException(headerData.ProcessType);
            }

            var marketActivityRecords = rootElement
                .Elements(ns + "MktActivityRecord");

            var elements = InternalMap(currentMappingConfiguration, marketActivityRecords, ns);

            return elements;
        }

        private static XmlHeaderData MapHeaderData(XContainer rootElement, XNamespace ns)
        {
            var mrid = ExtractElementValue(rootElement, ns + "mRID");
            var type = ExtractElementValue(rootElement, ns + "type");
            var processType = ExtractElementValue(rootElement, ns + "process.processType");

            var headerData = new XmlHeaderData(mrid, type, processType);

            return headerData;
        }

        private static string ExtractElementValue(XContainer element, XName name)
        {
            return element.Element(name)?.Value ?? string.Empty;
        }

        private static XElement ElementByName(XContainer parent, XName name)
        {
            var element = parent.Descendants(name).FirstOrDefault();

            // TODO: find a more suitable way to achieve further investigation in case element is not found
            if (element == null)
            {
                element = parent.Parent?.Descendants(name).FirstOrDefault();
            }

            if (element == null)
            {
                throw new Exception($"Element not found: {name}");
            }

            if (element.HasElements)
            {
                return element.Elements().First();
            }

            return element;
        }

        private static IEnumerable<IOutboundMessage> InternalMap(XmlMappingConfigurationBase xmlMappingConfigurationBase, IEnumerable<XElement> marketActivityRecords, XNamespace ns)
        {
            var properties = xmlMappingConfigurationBase.GetProperties();

            var messages = new List<IOutboundMessage>();

            foreach (var marketActivityRecord in marketActivityRecords)
            {
                var args = properties.Select(x =>
                {
                    if (x.Key == nameof(Application.CreateMeteringPoint.InstallationLocationAddress))
                    {
                        return new Address(); // TODO: mapper must be able to handle complex sub types
                    }

                    if (x.Value is null)
                    {
                        throw new ArgumentNullException($"Missing map for property with name: {x.Key}");
                    }

                    var marketEvaluationPoint = marketActivityRecord.Element(ns + "MarketEvaluationPoint") ?? throw new Exception();

                    var correspondingXmlElement = ElementByName(marketEvaluationPoint, ns + x.Value.XmlPropName);
                    return Convert(correspondingXmlElement.Value, x.Value.PropertyInfo.PropertyType);
                }).ToArray();

                if (xmlMappingConfigurationBase.CreateInstance(args) is not IOutboundMessage instance)
                {
                    throw new InvalidOperationException("Could not create instance");
                }

                messages.Add(instance);
            }

            return messages;
        }

        private static object Convert(string source, Type dest)
        {
            if (dest == typeof(string))
            {
                return source;
            }

            return System.Convert.ChangeType(source, dest);
        }
    }
}
