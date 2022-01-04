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
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Abstractions;
using Energinet.DataHub.MeteringPoints.Application.Common.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion.Functions
{
    public class MeteringPointHttpTrigger
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageDispatcher _dispatcher;
        private readonly IXmlDeserializer _xmlDeserializer;
        private readonly XmlSenderValidator _xmlSenderValidator;

        public MeteringPointHttpTrigger(
            ILogger logger,
            ICorrelationContext correlationContext,
            MessageDispatcher dispatcher,
            IXmlDeserializer xmlDeserializer,
            XmlSenderValidator xmlSenderValidator)
        {
            _logger = logger;
            _correlationContext = correlationContext;
            _dispatcher = dispatcher;
            _xmlDeserializer = xmlDeserializer;
            _xmlSenderValidator = xmlSenderValidator;
        }

        [Function("MeteringPoint")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestData request)
        {
            _logger.LogInformation($"Received MeteringPoint request");

            if (request == null) throw new ArgumentNullException(nameof(request));

            IEnumerable<IInternalMarketDocument> commands;
            try
            {
                var result = await _xmlDeserializer.DeserializeAsync(request.Body).ConfigureAwait(false);

                var senderValidationResult = _xmlSenderValidator.ValidateSender(result.HeaderData.Sender);

                if (!senderValidationResult.IsValid)
                    return await CreateForbiddenResponseAsync(request, senderValidationResult.ErrorMessage).ConfigureAwait(false);

                commands = result.Documents;
            }
            #pragma warning disable CA1031 // TODO: We'll allow catching Exception in the entrypoint, I guess?
            catch (Exception exception)
            #pragma warning restore CA1031
            {
                _logger.LogError(exception, "Unable to deserialize request");
                return request.CreateResponse(HttpStatusCode.BadRequest);
            }

            await DispatchCommandsAsync(commands).ConfigureAwait(false);
            return await CreateOkResponseAsync(request).ConfigureAwait(false);
        }

        private static async Task<HttpResponseData> CreateForbiddenResponseAsync(HttpRequestData request, string errorMessage)
        {
            var response = request.CreateResponse(HttpStatusCode.Forbidden);

            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync(errorMessage).ConfigureAwait(false);
            return response;
        }

        private async Task<HttpResponseData> CreateOkResponseAsync(HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);

            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync("Correlation id: " + _correlationContext.Id)
                .ConfigureAwait(false);
            return response;
        }

        private async Task DispatchCommandsAsync(IEnumerable<IInternalMarketDocument> commands)
        {
            foreach (var command in commands)
            {
                await _dispatcher.DispatchAsync((IOutboundMessage)command).ConfigureAwait(false);
            }
        }
    }
}
