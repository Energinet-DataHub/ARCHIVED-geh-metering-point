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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Production
{
    public class CreateProductionMeteringPointInitiator : ICreateMeteringPointInitiator<MasterDataDocument>
    {
        private readonly IMediator _mediator;
        private readonly ICreateMeteringPointInitiator<MasterDataDocument> _next;
        private readonly IValidator<MasterDataDocument> _validator;
        private readonly IBusinessProcessValidationContext _validationContext;

        public CreateProductionMeteringPointInitiator(IMediator mediator, ICreateMeteringPointInitiator<MasterDataDocument> next, IValidator<MasterDataDocument> validator, IBusinessProcessValidationContext validationContext)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _next = next;
            _validator = validator;
            _validationContext = validationContext ?? throw new ArgumentNullException(nameof(validationContext));
        }

        public Task ProcessAsync(MasterDataDocument message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var meteringPointType = EnumerationType.FromName<MeteringPointType>(message.TypeOfMeteringPoint);
            return meteringPointType == MeteringPointType.Production ? _mediator.Send(CreateCommandFrom(message)) : _next?.ProcessAsync(message)!;
        }

        private static CreateProductionMeteringPoint CreateCommandFrom(MasterDataDocument document)
        {
            return new CreateProductionMeteringPoint
            {
                AssetType = document.AssetType!,
                BuildingNumber = document.BuildingNumber,
                CityName = document.CityName,
                ConnectionType = document.ConnectionType,
                CountryCode = document.CountryCode,
                DisconnectionType = document.DisconnectionType,
                EffectiveDate = document.EffectiveDate,
                FloorIdentification = document.FloorIdentification,
                GsrnNumber = document.GsrnNumber,
                LocationDescription = document.LocationDescription,
                MaximumCurrent = document.MaximumCurrent,
                MaximumPower = document.MaximumPower,
                MeteringMethod = document.MeteringMethod,
                MeterNumber = document.MeterNumber,
                MunicipalityCode = document.MunicipalityCode,
                PostCode = document.PostCode,
                PowerPlant = document.PowerPlant,
                RoomIdentification = document.RoomIdentification,
                StreetCode = document.StreetCode,
                StreetName = document.StreetName,
                TransactionId = document.TransactionId,
                GeoInfoReference = document.GeoInfoReference,
                IsActualAddress = document.IsActualAddress,
                MeteringGridArea = document.MeteringGridArea,
                MeterReadingOccurrence = document.MeterReadingOccurrence,
                NetSettlementGroup = document.NetSettlementGroup,
                PhysicalConnectionCapacity = document.PhysicalConnectionCapacity,
                CitySubDivisionName = document.CitySubDivisionName,
            };
        }

        private Task HandleOrCallNextAsync(MasterDataDocument message)
        {
            var meteringPointType = EnumerationType.FromName<MeteringPointType>(message.TypeOfMeteringPoint);
            return meteringPointType == MeteringPointType.Production
                ? HandleInternalAsync(message)
                : _next?.ProcessAsync(message)!;
        }

        private async Task HandleInternalAsync(MasterDataDocument message)
        {
            var validationResult = await _validator.ValidateAsync(message).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult
                    .Errors
                    .Select(error => (ValidationError)error.CustomState)
                    .ToList()
                    .AsReadOnly();

                _validationContext.Add(validationErrors);
            }

            await _mediator.Send(CreateCommandFrom(message)).ConfigureAwait(false);
        }
    }
}
