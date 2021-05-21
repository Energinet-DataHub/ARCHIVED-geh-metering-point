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

using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Transport;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public class CreateMeteringPointHttpTrigger
    {
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageDispatcher _dispatcher;

        public CreateMeteringPointHttpTrigger(
            ICorrelationContext correlationContext,
            MessageDispatcher dispatcher)
        {
            _correlationContext = correlationContext;
            _dispatcher = dispatcher;
        }

        [Function("CreateMeteringPoint")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData request,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("CreateMeteringPointHttpTrigger");
            logger.LogInformation("Received CreateMeteringPoint request");

            using var r = new StreamReader(request.Body, Encoding.UTF8);
            var bodyStr = await r.ReadToEndAsync();
            var root = XDocument.Parse(bodyStr);
            XNamespace ns = "urn:ediel:org:requestchangeofapcharacteristics:0:1";

            var response = request.CreateResponse(HttpStatusCode.OK);
            var testElement = root
                .Descendants(ns + "mRID")
                .Where(el => el.Parent?.Name == ns + "MktActivityRecord")
                .Select(el => el).ToList();

            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync("Correlation id: " + _correlationContext.GetCorrelationId()).ConfigureAwait(false);

            var command = new CreateMeteringPoint
            {
                GsrnNumber = "1234567",
            };

            await _dispatcher.DispatchAsync(command).ConfigureAwait(false);

            return response;
        }
    }
}
