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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter
{
    public class ConverterMapperConfiguration
    {
        private readonly Type _type;
        private readonly string _xmlElementName;
        private readonly Dictionary<string, ExtendedPropertyInfo?> _properties;

        public ConverterMapperConfiguration(Type type, string xmlElementName, Dictionary<string, ExtendedPropertyInfo?> properties)
        {
            _type = type;
            _xmlElementName = xmlElementName;
            _properties = properties;
        }

        public Dictionary<string, ExtendedPropertyInfo?> GetProperties()
        {
            return _properties;
        }

        public string GetXmlElementName()
        {
            return _xmlElementName;
        }

        public new Type GetType()
        {
            return _type;
        }
    }
}
