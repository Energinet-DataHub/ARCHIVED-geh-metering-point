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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
            HttpRequestData request,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("CreateMeteringPointHttpTrigger");
            logger.LogInformation("Received CreateMeteringPoint request");

            using var r = new StreamReader(request.Body, Encoding.UTF8);
            var bodyStr = await r.ReadToEndAsync();
            DeserializeCreateMeteringPointXml(bodyStr);

            var response = request.CreateResponse(HttpStatusCode.OK);

            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync("Correlation id: " + _correlationContext.GetCorrelationId())
                .ConfigureAwait(false);

            var command = new CreateMeteringPoint(new Address()) {GsrnNumber = "1234567",};

            await _dispatcher.DispatchAsync(command).ConfigureAwait(false);

            return response;
        }

        private static void DeserializeCreateMeteringPointXml(string bodyStr)
        {
            var root = XElement.Parse(bodyStr);
            XNamespace ns = "urn:ediel:org:requestchangeofapcharacteristics:0:1";

            // var occurenceDate = root.Element(ns + "MktActivityRecord")?.Element(ns + "start_DateAndOrTime.dateTime")?.Value;
            var marketActivityRecords = root
                .Elements(ns + "MktActivityRecord");

            marketActivityRecords.Select(record =>
            {
                var mainAddress = record.Element(ns + "usagePointLocation.mainAddress");
                var streetDetail = mainAddress?.Element(ns + "streetDetail");
                var townDetail = mainAddress?.Element(ns + "townDetail");

                var address = new Address(
                    ExtractElementValue(streetDetail, ns + "name"),
                    ExtractElementValue(mainAddress, ns + "postalCode"),
                    ExtractElementValue(townDetail, ns + "name"),
                    ExtractElementValue(townDetail, ns + "country"),
                    ExtractElementValue(mainAddress, ns + "usagePointLocation.remark") == "D01");

                var marketEvaluationPoint = record.Element(ns + "MarketEvaluationPoint");
                var contractedConnectionCapacity = marketEvaluationPoint?.Element(ns + "marketAgreement.contractedConnectionCapacity"); // TODO: Make this use partial match instead since "marketAgreement" isn't certain

                // Power plant
                var linkedMarketEvaluationPoint = record.Element(ns + "Linked_MarketEvaluationPoint");

                return new CreateMeteringPoint(
                    address,
                    ExtractElementValue(marketEvaluationPoint, ns + "mRID"),
                    ExtractElementValue(marketEvaluationPoint, ns + "type"),
                    ExtractElementValue(marketEvaluationPoint, ns + "meteringMethod"),
                    ExtractElementValue(marketEvaluationPoint, ns + "readCycle"),
                    Convert.ToInt32(ExtractElementValue(marketEvaluationPoint, ns + "ratedCurrent")),
                    Convert.ToInt32(ExtractElementValue(contractedConnectionCapacity, ns + "value")),
                    ExtractElementValue(marketEvaluationPoint, ns + "meteringGridArea_Domain.mRID"),
                    ExtractElementValue(linkedMarketEvaluationPoint, ns + "mRID"),



                    ExtractElementValue(marketEvaluationPoint, ns + "name"),
                    ExtractElementValue(marketEvaluationPoint, ns + "name"),
                    ExtractElementValue(marketEvaluationPoint, ns + "name"),
                    ExtractElementValue(marketEvaluationPoint, ns + "name"),
                    ExtractElementValue(marketEvaluationPoint, ns + "name"),
                    ExtractElementValue(marketEvaluationPoint, ns + "name"),
                    ExtractElementValue(marketEvaluationPoint, ns + "name"),
                    ExtractElementValue(marketEvaluationPoint, ns + "name"),
                    ExtractElementValue(marketEvaluationPoint, ns + "name")
                );
            })
            // .Where(el => el.Parent?.Name == ns + "MktActivityRecord")
            // .Select(el => el).ToList();
        }

        private static string ExtractElementValue(XElement? element, XName name)
        {
            return element?.Element(name)?.Value ?? string.Empty;
        }
    }
}
