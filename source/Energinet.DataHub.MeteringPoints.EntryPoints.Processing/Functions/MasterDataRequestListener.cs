﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MeteringPoints.RequestResponse.Requests;
using Energinet.DataHub.MeteringPoints.RequestResponse.Response;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MasterData = Energinet.DataHub.MeteringPoints.Application.RequestMasterData.MasterData;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions
{
    public class MasterDataRequestListener
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly ServiceBusSender _serviceBusSender;

        public MasterDataRequestListener(
            ILogger logger,
            IMediator mediator,
            ServiceBusSender serviceBusSender)
        {
            _logger = logger;
            _mediator = mediator;
            _serviceBusSender = serviceBusSender;
        }

        [Function("MasterDataRequestListener")]
        public async Task RunAsync(
            [ServiceBusTrigger("%MASTER_DATA_REQUEST_QUEUE_NAME%", Connection = "SHARED_SERVICE_BUS_LISTEN_CONNECTION_STRING")] byte[] data,
            FunctionContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var request = MeteringPointMasterDataRequest.Parser.ParseFrom(data);
            var query = new GetMasterDataQuery(request.GsrnNumber);

            var result = await _mediator.Send(query).ConfigureAwait(false);
            await RespondAsync(CreateResponseFrom(result), ParseCorrelationIdFromMessage(context)).ConfigureAwait(false);

            _logger.LogInformation($"Received request for master data: {data}");
        }

        private static string ParseCorrelationIdFromMessage(FunctionContext context)
        {
            context.BindingContext.BindingData.TryGetValue("CorrelationId", out var correlationIdValue);
            if (correlationIdValue is string correlationId)
            {
                return correlationId;
            }

            throw new InvalidOperationException("Correlation id is not set on metering point master data request message.");
        }

        private static MeteringPointMasterDataResponse CreateResponseFrom(MasterData result)
        {
            return new MeteringPointMasterDataResponse
            {
                MasterData = new RequestResponse.Response.MasterData()
                {
                    GsrnNumber = result.GsrnNumber,
                    Address = new RequestResponse.Response.Address
                    {
                        StreetName = result.Address.StreetName,
                        StreetCode = result.Address.StreetCode,
                        PostCode = result.Address.PostCode,
                        City = result.Address.City,
                        CountryCode = result.Address.CountryCode,
                        CitySubDivision = result.Address.CitySubDivision,
                        Floor = result.Address.Floor,
                        Room = result.Address.Room,
                        BuildingNumber = result.Address.BuildingNumber,
                        MunicipalityCode = result.Address.MunicipalityCode,
                        IsActualAddress = result.Address.IsActualAddress,
                        GeoInfoReference = result.Address.GeoInfoReference.ToString(),
                        LocationDescription = result.Address.LocationDescription,
                    },
                    Series = new RequestResponse.Response.Series { Product = result.Series.Product, UnitType = result.Series.UnitType, },
                    GridAreaDetails =
                        new RequestResponse.Response.GridAreaDetails
                        {
                            Code = result.GridAreaDetails.Code,
                            ToCode = result.GridAreaDetails.ToCode,
                            FromCode = result.GridAreaDetails.FromCode,
                        },
                    ConnectionState = result.ConnectionState,
                    MeteringMethod = result.MeteringMethod,
                    ReadingPeriodicity = result.ReadingPeriodicity,
                    Type = result.Type,
                    MaximumCurrent = result.MaximumCurrent,
                    MaximumPower = result.MaximumPower,
                    PowerPlantGsrnNumber = result.PowerPlantGsrnNumber,
                    EffectiveDate = result.EffectiveDate.ToUniversalTime().ToTimestamp(),
                    MeterNumber = result.MeterNumber,
                    Capacity = result.Capacity,
                    AssetType = result.AssetType,
                    SettlementMethod = result.SettlementMethod,
                    ScheduledMeterReadingDate = result.ScheduledMeterReadingDate,
                    ProductionObligation = result.ProductionObligation,
                    NetSettlementGroup = result.NetSettlementGroup,
                    DisconnetionType = result.DisconnectionType,
                    ConnectionType = result.ConnectionType,
                    ParentRelatedMeteringPoint = result.ParentRelatedMeteringPoint.ToString(),
                    GridOperatorId = result.GridOperatorId.ToString(),
                },
            };
        }

        private Task RespondAsync(MeteringPointMasterDataResponse response, string correlationId)
        {
            var bytes = response.ToByteArray();
            ServiceBusMessage serviceBusMessage = new(bytes)
            {
                ContentType = "application/octet-stream;charset=utf-8",
            };
            serviceBusMessage.CorrelationId = correlationId;
            return _serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
