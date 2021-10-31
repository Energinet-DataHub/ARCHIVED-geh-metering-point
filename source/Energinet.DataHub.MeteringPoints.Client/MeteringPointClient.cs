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
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Client.Abstractions;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;

namespace Energinet.DataHub.MeteringPoints.Client
{
    public sealed class MeteringPointClient : IMeteringPointClient
    {
        private readonly HttpClient _httpClient;

        internal MeteringPointClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<MeteringPointDto?> GetMeteringPointByGsrnAsync(string gsrn)
        {
            var response = await _httpClient.GetFromJsonAsync<MeteringPointDto?>(new Uri("MeteringPoint/GetMeteringPointByGsrn/?gsrn=" + gsrn, UriKind.Relative)).ConfigureAwait(false);

            return response;
        }
    }
}
