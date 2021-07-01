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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public class ConnectMeteringPointHttpTrigger : BaseTrigger
    {
        public ConnectMeteringPointHttpTrigger(
            MessageDispatcher dispatcher,
            IXmlConverter xmlConverter,
            ICorrelationContext correlationContext)
            : base(correlationContext, dispatcher, xmlConverter)
        {
        }

        [Function("ConnectMeteringPoint")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestData request,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ConnectMeteringPoint");
            logger.LogInformation("Received ConnectMeteringPoint request");

            try
            {
                await DispatchCommandsAsync(request.Body, logger).ConfigureAwait(false);
            }
            catch
            {
                return request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return await CreateResponseAsync(request).ConfigureAwait(false);
        }
    }
}
