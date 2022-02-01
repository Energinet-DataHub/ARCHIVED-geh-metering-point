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
using System.Linq;
using System.Reflection;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions
{
    public static class MasterDataHandlingRegistrationExtensions
    {
        public static void AddMasterDataValidators(this Container container, params Assembly[] assembliesToScan)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var validatorTypes = container.GetTypesToRegister(typeof(IMasterDataValidatorStrategy), assembliesToScan).ToList();
            container.Collection.Register<IMasterDataValidatorStrategy>(validatorTypes, Lifestyle.Transient);
            container.Register<MasterDataValidator>(() =>
                new MasterDataValidator(container.GetAllInstances<IMasterDataValidatorStrategy>().ToArray()));
        }

        public static void AddMasterDataUpdateServices(this Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            container.Register<MasterDataUpdateHandler>(Lifestyle.Transient);
        }
    }
}
