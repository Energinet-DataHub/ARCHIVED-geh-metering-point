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
using System.Net.Http;
using Energinet.DataHub.MeteringPoints.Client.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.MeteringPoints.Client.Extensions
{
    public static class ContainerExtensions
    {
        public static IServiceCollection AddMeteringPointClient(this IServiceCollection services, Uri baseUrl)
        {
            services.AddScoped<IMeteringPointClient>(x => new MeteringPointClientFactory(x.GetRequiredService<IHttpClientFactory>()).CreateClient(baseUrl));

            if (services.Any(x => x.ServiceType == typeof(IHttpClientFactory))) return services;

            services.AddHttpClient();

            return services;
        }
    }
}
