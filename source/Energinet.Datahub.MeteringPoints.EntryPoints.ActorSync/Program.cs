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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.ActorSync;

public class Program : EntryPoint
{
    public static async Task Main()
    {
        var program = new Program();
        var host = program.ConfigureApplication();
        program.AssertConfiguration();
        await program.ExecuteApplicationAsync(host).ConfigureAwait(false);
    }

    protected override void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder options)
    {
        base.ConfigureFunctionsWorkerDefaults(options);
    }

    protected override void ConfigureServiceCollection(IServiceCollection services)
    {
        base.ConfigureServiceCollection(services);
    }

    protected override void ConfigureContainer(Container container)
    {
        base.ConfigureContainer(container);
    }
}
