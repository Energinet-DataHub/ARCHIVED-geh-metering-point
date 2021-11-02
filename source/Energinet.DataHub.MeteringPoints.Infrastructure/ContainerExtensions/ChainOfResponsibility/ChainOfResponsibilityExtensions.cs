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

using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions.ChainOfResponsibility
{
    /// <summary>
    /// Extension methods for creating a new <see cref="ChainOfResponsibilityBuilder{TChain}"/>.
    /// </summary>
    public static class ChainOfResponsibilityExtensions
    {
        /// <summary>
        /// Creates a new instance of the chain builder of the provided chain interface with the provided service
        /// collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <typeparam name="TChain">The chain interface.</typeparam>
        /// <returns>The chain builder.</returns>
        public static ChainOfResponsibilityBuilder<TChain> AddChain<TChain>(this Container services)
            where TChain : class
        {
            return new ChainOfResponsibilityBuilder<TChain>(services);
        }
    }
}
