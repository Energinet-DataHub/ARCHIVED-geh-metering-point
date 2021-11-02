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
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Messages;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Exchange
{
    public class CreateExchangeMeteringPointInitiator : MessageReceiver<MasterDataDocument>, ICreateMeteringPointInitiator
    {
        private readonly IMediator _mediator;
        private readonly IValidator<MasterDataDocument> _validator;
        private readonly IBusinessProcessValidationContext _validationContext;

        public CreateExchangeMeteringPointInitiator(IMediator mediator, ICreateMeteringPointInitiator next, IValidator<MasterDataDocument> validator, IBusinessProcessValidationContext validationContext)
            : base(next)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _validator = validator;
            _validationContext = validationContext ?? throw new ArgumentNullException(nameof(validationContext));
        }

        protected override bool ShouldHandle(MasterDataDocument message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var meteringPointType = EnumerationType.FromName<MeteringPointType>(message.TypeOfMeteringPoint);
            return meteringPointType == MeteringPointType.Exchange;
        }

        protected override async Task ProcessAsync(MasterDataDocument message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            await _validationContext.ValidateAsync(_validator, message).ConfigureAwait(false);
            await _mediator.Send(CreateCommandFrom(message)).ConfigureAwait(false);
        }

        private static CreateExchangeMeteringPoint CreateCommandFrom(MasterDataDocument document)
        {
            return new CreateExchangeMeteringPoint
            {
                BuildingNumber = document.BuildingNumber,
                CityName = document.CityName,
                CountryCode = document.CountryCode,
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
                RoomIdentification = document.RoomIdentification,
                StreetCode = document.StreetCode,
                StreetName = document.StreetName,
                TransactionId = document.TransactionId,
                MeteringGridArea = document.MeteringGridArea,
                MeterReadingOccurrence = document.MeterReadingOccurrence,
                PhysicalConnectionCapacity = document.PhysicalConnectionCapacity,
                CitySubDivisionName = document.CitySubDivisionName,
                FromGrid = document.FromGrid ?? string.Empty,
                ToGrid = document.ToGrid ?? string.Empty,
            };
        }
    }
}
