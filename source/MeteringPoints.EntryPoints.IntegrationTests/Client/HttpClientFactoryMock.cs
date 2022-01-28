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
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Tooling.WebApi;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Client
{
    public class HttpClientFactoryMock : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public HttpClientFactoryMock(WebApiFactory webApiFactory)
        {
            if (webApiFactory == null) throw new ArgumentNullException(nameof(webApiFactory));
            _httpClient = webApiFactory.CreateClient();
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }
}
