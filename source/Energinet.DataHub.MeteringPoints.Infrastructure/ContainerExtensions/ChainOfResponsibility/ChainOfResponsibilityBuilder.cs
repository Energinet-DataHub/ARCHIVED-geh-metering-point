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
using System.Reflection;
using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions.ChainOfResponsibility
{
    /// <summary>
    /// Build a new Chain of Responsibility in the provided service collection.
    /// </summary>
    /// <typeparam name="TChain">The chain interface.</typeparam>
    public class ChainOfResponsibilityBuilder<TChain>
        where TChain : class
    {
        private readonly Container _services;

        private IList<Type> _handlers;

        internal ChainOfResponsibilityBuilder(Container services)
        {
            _services = services;
            _handlers = new List<Type>();
        }

        /// <summary>
        /// Adds an handler to the chain.
        /// </summary>
        /// <typeparam name="THandler">The handler.</typeparam>
        /// <returns>The current chain builder.</returns>
        public ChainOfResponsibilityBuilder<TChain> WithHandler<THandler>()
            where THandler : class, TChain
        {
            _handlers.Add(typeof(THandler));
            return this;
        }

        /// <summary>
        /// Builds the chain.
        /// </summary>
        /// <returns>The service collection with the chain.</returns>
        /// <exception cref="EmptyChainException">When the chain is empty.</exception>
        /// <exception cref="MissingPublicConstructorException">When the handler does not have any public constructor.</exception>
        public Container BuildChain()
        {
            if (_handlers.Count == 0)
            {
                throw new EmptyChainException();
            }

            _services.Register(() => InstantiateRecursively(_services));
            return _services;
        }

        /// <summary>
        /// Creates recursively a new instance of every chain member.
        /// </summary>
        /// <param name="services">The service provider.</param>
        /// <param name="current">The index of the current handler.</param>
        /// <returns>The instantiated handler.</returns>
        /// <exception cref="MissingPublicConstructorException">When the handler does not have any public constructor.</exception>
        private TChain InstantiateRecursively(IServiceProvider services, int current = 0)
        {
            var constructor = _handlers[current].GetConstructors().Where(_ => _.IsPublic)
                .OrderByDescending(_ => _.GetParameters().Length).FirstOrDefault();

            if (constructor is null)
            {
                throw new MissingPublicConstructorException(_handlers[current].FullName!);
            }

            var constructorParameters = constructor.GetParameters();

            IList<object> parametersInstances = new List<object>();

            foreach (ParameterInfo parameter in constructorParameters)
            {
                var type = parameter.ParameterType;

                if (type == typeof(TChain))
                {
                    if (current == _handlers.Count - 1)
                    {
                        parametersInstances.Add(null!);
                    }
                    else
                    {
                        parametersInstances.Add(InstantiateRecursively(services, current + 1));
                    }
                }
                else
                {
                    parametersInstances.Add(services.GetService(type)!);
                }
            }

            return (TChain)Activator.CreateInstance(_handlers[current], parametersInstances.ToArray())!;
        }
    }
}
