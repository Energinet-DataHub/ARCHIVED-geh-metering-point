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
using System.Xml.Linq;
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
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
                var (succeeded, errorResponse, element) = await ValidateAndReadXmlAsync(request).ConfigureAwait(false);

                if (!succeeded) return errorResponse ?? CreateResponse(request, HttpStatusCode.BadRequest);

                var result = _xmlDeserializer.Deserialize(element!);
                var senderValidationResult = _xmlSenderValidator.ValidateSender(result.HeaderData.Sender);

                _logger.LogInformation($"Received request of type {result.HeaderData.ProcessType} from sender {result.HeaderData.Sender.Id}");

                if (!senderValidationResult.IsValid)
                    return await CreateForbiddenResponseAsync(request, senderValidationResult.ErrorMessage).ConfigureAwait(false);

                commands = result.Documents;
            }
            #pragma warning disable CA1031 // TODO: We'll allow catching Exception in the entrypoint, I guess?
            catch (Exception exception)
            #pragma warning restore CA1031
            {
                _logger.LogError(exception, "Unable to deserialize request");
                return CreateResponse(request, HttpStatusCode.BadRequest);
            }

            await DispatchCommandsAsync(commands).ConfigureAwait(false);
            return CreateResponse(request, HttpStatusCode.Accepted);
        }

        private async Task<HttpResponseData> CreateForbiddenResponseAsync(HttpRequestData request, string errorMessage)
        {
            var response = CreateResponse(request, HttpStatusCode.Forbidden);
            await response.WriteStringAsync(errorMessage).ConfigureAwait(false);

            return response;
        }

        private async Task<(bool Succeeded, HttpResponseData? ErrorResponse, XElement? Element)> ValidateAndReadXmlAsync(HttpRequestData request)
        {
            var reader = new SchemaValidatingReader(request.Body, Schemas.CimXml.StructureRequestChangeAccountingPointCharacteristics);

            HttpResponseData? response = null;
            var isSucceeded = true;

            var xmlElement = await reader.AsXElementAsync().ConfigureAwait(false);

            if (!reader.HasErrors) return (isSucceeded, response, xmlElement);

            isSucceeded = false;
            response = CreateResponse(request, HttpStatusCode.BadRequest);

            await reader
                .CreateErrorResponse()
                .WriteAsXmlAsync(response.Body)
                .ConfigureAwait(false);

            return (isSucceeded, response, xmlElement);
        }

        private async Task DispatchCommandsAsync(IEnumerable<IInternalMarketDocument> commands)
        {
            foreach (var command in commands)
            {
                _logger.LogInformation($"Dispatching command for internal processing. CorrelationId: {_correlationContext.Id}");
                await _dispatcher.DispatchAsync((IOutboundMessage)command).ConfigureAwait(false);
            }
        }

        private HttpResponseData CreateResponse(HttpRequestData request, HttpStatusCode statusCode)
        {
            var response = request.CreateResponse(statusCode);
            response.Headers.Add("CorrelationId", _correlationContext.Id);

            return response;
        }
    }
}
