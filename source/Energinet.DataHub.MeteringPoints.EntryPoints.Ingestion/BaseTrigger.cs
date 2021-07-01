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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public abstract class BaseTrigger
    {
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageDispatcher _dispatcher;
        private readonly IXmlConverter _xmlConverter;

        protected BaseTrigger(
            ICorrelationContext correlationContext,
            MessageDispatcher dispatcher,
            IXmlConverter xmlConverter)
        {
            _correlationContext = correlationContext;
            _dispatcher = dispatcher;
            _xmlConverter = xmlConverter;
        }

        protected async Task<HttpResponseData> CreateResponseAsync(HttpRequestData request)
        {
             var response = request.CreateResponse(HttpStatusCode.OK);

             response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

             await response.WriteStringAsync("Correlation id: " + _correlationContext.GetCorrelationId())
                .ConfigureAwait(false);
             return response;
        }

        protected async Task DispatchCommandsAsync(Stream stream, ILogger logger)
        {
            IEnumerable<IBusinessRequest>? commands = null;

            try
            {
                commands = await _xmlConverter.DeserializeAsync(stream).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to deserialize request");
                throw new Exception(exception.Message);
            }

            foreach (var command in commands)
            {
                await _dispatcher.DispatchAsync((IOutboundMessage)command).ConfigureAwait(false);
            }
        }
    }
}
