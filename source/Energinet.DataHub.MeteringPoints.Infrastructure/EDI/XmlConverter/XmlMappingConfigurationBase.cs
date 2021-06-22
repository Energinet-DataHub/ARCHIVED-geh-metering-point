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
using System.Linq;
using System.Linq.Expressions;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter
{
    public class XmlMappingConfigurationBase
    {
        private ConverterMapperConfiguration? _configuration;

        private ConstructorDelegate? _cachedConstructor;

        private delegate object ConstructorDelegate(params object[] args);

        public Dictionary<string, ExtendedPropertyInfo?> GetProperties()
        {
            return _configuration?.GetProperties() ?? throw new InvalidOperationException();
        }

        public string GetXmlElementName()
        {
            return _configuration?.GetXmlElementName() ?? throw new InvalidOperationException();
        }

        public object CreateInstance(params object[] parameters)
        {
            if (_cachedConstructor is null)
            {
                throw new Exception();
            }

            return _cachedConstructor(parameters);
        }

        protected void CreateMapping<T>(string xmlElementName, Func<ConverterMapperConfigurationBuilder<T>, ConverterMapperConfigurationBuilder<T>> createFunc)
        {
            _configuration = createFunc(new ConverterMapperConfigurationBuilder<T>(xmlElementName)).Build();
            CreateConstructor();
        }

        private void CreateConstructor()
        {
            var constructorInfo = _configuration?.GetType().GetConstructors().SingleOrDefault() ?? throw new Exception("No constructor found for type");
            var parameters = constructorInfo.GetParameters().Select(x => x.ParameterType);
            var paramExpr = Expression.Parameter(typeof(object[]));
            var constructorParameters = parameters.Select((paramType, index) => Expression.Convert(Expression.ArrayAccess(paramExpr, Expression.Constant(index)), paramType!)).ToArray<Expression>();
            var body = Expression.New(constructorInfo!, constructorParameters);
            _cachedConstructor = Expression.Lambda<ConstructorDelegate>(body, paramExpr).Compile();
        }
    }
}
