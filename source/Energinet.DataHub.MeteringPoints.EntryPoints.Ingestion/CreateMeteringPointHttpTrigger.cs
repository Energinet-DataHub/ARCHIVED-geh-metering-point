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
using System.ComponentModel.Design;
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

            // TODO: Currently we assume that we will have a function for each metering point event. This might change if we're not able to handle the routing in the API Gateway.
            // TODO: In that case we would need to make a switch case or something like that to look at the value in the "process.processType" element.
            var commands = DeserializeCreateMeteringPointXml(bodyStr);

            var response = request.CreateResponse(HttpStatusCode.OK);

            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync("Correlation id: " + _correlationContext.GetCorrelationId())
                .ConfigureAwait(false);

            foreach (var command in commands)
            {
                await _dispatcher.DispatchAsync(command).ConfigureAwait(false);
            }

            return response;
        }

        private static IEnumerable<CreateMeteringPoint> DeserializeCreateMeteringPointXml(string bodyStr)
        {
            var root = XElement.Parse(bodyStr);
            XNamespace ns = "urn:ediel:org:requestchangeofapcharacteristics:0:1";

            var marketActivityRecords = root
                .Elements(ns + "MktActivityRecord");

            return marketActivityRecords.Select(record =>
            {
                var marketEvaluationPoint = record.Element(ns + "MarketEvaluationPoint");
                var mainAddress = marketEvaluationPoint?.Element(ns + "usagePointLocation.mainAddress");
                var streetDetail = mainAddress?.Element(ns + "streetDetail");
                var townDetail = mainAddress?.Element(ns + "townDetail");
                var contractedConnectionCapacity =
                    marketEvaluationPoint?.Element(ns +
                                                   "marketAgreement.contractedConnectionCapacity"); // TODO: Make this use partial match instead since "marketAgreement" isn't certain
                var series = marketEvaluationPoint?.Element(ns + "Series");

                // Power plant
                var linkedMarketEvaluationPoint = marketEvaluationPoint?.Element(ns + "Linked_MarketEvaluationPoint");

                var address = new Address(
                    ExtractElementValue(streetDetail, ns + "name"),
                    ExtractElementValue(mainAddress, ns + "postalCode"),
                    ExtractElementValue(townDetail, ns + "name"),
                    ExtractElementValue(townDetail, ns + "country"),
                    ExtractElementValue(mainAddress, ns + "usagePointLocation.remark") == "D01");

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
                    ExtractElementValue(marketEvaluationPoint, ns + "usagePointLocation.remark"),
                    ExtractElementValue(marketEvaluationPoint, ns + "product"),
                    ExtractElementValue(marketEvaluationPoint, ns + "parent_MarketEvaluationPoint.mRID"),
                    ExtractElementValue(marketEvaluationPoint, ns + "settlementMethod"),
                    ExtractElementValue(series, ns + "quantity_Measure_Unit.name"),
                    ExtractElementValue(marketEvaluationPoint, ns + "disconnectionMethod"),
                    ExtractElementValue(record, ns + "start_DateAndOrTime.dateTime"),
                    ExtractElementValue(marketEvaluationPoint, ns + "meter.mRID"),
                    ExtractElementValue(record, ns + "mRID"));
            });
        }

        private static string ExtractElementValue(XElement? element, XName name)
        {
            return element?.Element(name)?.Value ?? string.Empty;
        }
    }
}
