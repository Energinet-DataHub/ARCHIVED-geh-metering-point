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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MeteringPoints.Application.RequestMasterData;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.RequestResponse.Requests;
using Google.Protobuf;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions
{
    public class MasterDataRequestListener
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly ServiceBusSender _serviceBusSender;
        private readonly ProtobufOutboundMapperFactory _factory;

        public MasterDataRequestListener(
            ILogger logger,
            IMediator mediator,
            ServiceBusSender serviceBusSender,
            ProtobufOutboundMapperFactory factory)
        {
            _logger = logger;
            _mediator = mediator;
            _serviceBusSender = serviceBusSender;
            _factory = factory;
        }

        [Function("MasterDataRequestListener")]
        public async Task RunAsync(
            [ServiceBusTrigger("%MASTER_DATA_REQUEST_QUEUE_NAME%", Connection = "SERVICE_BUS_LISTEN_CONNECTION_STRING")] byte[] data,
            FunctionContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var request = MasterDataRequest.Parser.ParseFrom(data);
            var query = new GetMasterDataQuery(request.GsrnNumber);

            var result = await _mediator.Send(query).ConfigureAwait(false);

            var mapper = _factory.GetMapper(result);
            var message = mapper.Convert(result);
            var bytes = message.ToByteArray();
            ServiceBusMessage serviceBusMessage = new(bytes)
            {
                ContentType = "application/octet-stream;charset=utf-8",
            };
            await _serviceBusSender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);

            _logger.LogInformation($"Received request for master data: {data}");
        }
    }
}
