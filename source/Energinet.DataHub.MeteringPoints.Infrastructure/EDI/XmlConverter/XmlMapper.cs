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

            var elements = InternalMap(currentMappingConfiguration, rootElement, ns);

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

        private static XElement? GetXmlElement(XContainer? container, Stack<string> hierarchy, XNamespace ns)
        {
            if (container is null)
            {
                throw new ArgumentNullException();
            }

            var elementName = hierarchy.Pop();
            var element = container.Element(ns + elementName);

            return hierarchy.Any() ? GetXmlElement(element, hierarchy, ns) : element;
        }

        private static IEnumerable<IOutboundMessage> InternalMap(XmlMappingConfigurationBase xmlMappingConfigurationBase, XElement rootElement, XNamespace ns)
        {
            var properties = xmlMappingConfigurationBase.GetProperties();

            var messages = new List<IOutboundMessage>();

            var elements = rootElement.Elements(ns + xmlMappingConfigurationBase.GetXmlElementName());

            foreach (var element in elements)
            {
                var args = properties.Select(property =>
                {
                    if (property.Value is null)
                    {
                        throw new ArgumentNullException($"Missing map for property with name: {property.Key}");
                    }

                    if (property.Value.IsComplex())
                    {
                        return new Address();
                    }

                    var xmlHierarchyStack = new Stack<string>(property.Value.XmlHierarchy.Reverse());
                    var correspondingXmlElement = GetXmlElement(element, xmlHierarchyStack, ns);

                    return Convert(correspondingXmlElement?.Value, property.Value.PropertyInfo.PropertyType);
                }).ToArray();

                if (xmlMappingConfigurationBase.CreateInstance(args) is not IOutboundMessage instance)
                {
                    throw new InvalidOperationException("Could not create instance");
                }

                messages.Add(instance);
            }

            return messages;
        }

        private static object? Convert(string? source, Type dest)
        {
            if (dest == typeof(Nullable<>))
            {
                return null;
            }

            if (dest == typeof(string))
            {
                return source;
            }

            return System.Convert.ChangeType(source, dest);
        }
    }
}
