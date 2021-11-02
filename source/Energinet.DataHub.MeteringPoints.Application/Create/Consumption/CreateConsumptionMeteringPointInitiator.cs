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
using Energinet.DataHub.MeteringPoints.Application.Common.Messages;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Consumption
{
    public class CreateConsumptionMeteringPointInitiator : MessageReceiver<MasterDataDocument>, ICreateMeteringPointInitiator
    {
        private readonly IMediator _mediator;
        private readonly IValidator<MasterDataDocument> _validator;
        private readonly IBusinessProcessValidationContext _validationContext;

        public CreateConsumptionMeteringPointInitiator(IMediator mediator, ICreateMeteringPointInitiator next, IValidator<MasterDataDocument> validator, IBusinessProcessValidationContext validationContext)
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
            return meteringPointType == MeteringPointType.Consumption;
        }

        protected override async Task ProcessAsync(MasterDataDocument message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            await ValidateMessageAsync(message).ConfigureAwait(false);
            await _mediator.Send(CreateCommandFrom(message)).ConfigureAwait(false);
        }

        private static CreateConsumptionMeteringPoint CreateCommandFrom(MasterDataDocument document)
        {
            return new CreateConsumptionMeteringPoint
            {
                AssetType = document.AssetType,
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
                SettlementMethod = document.SettlementMethod,
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
                ScheduledMeterReadingDate = document.ScheduledMeterReadingDate,
            };
        }

        private Task ValidateMessageAsync(MasterDataDocument message)
        {
            return _validationContext.ValidateAsync(_validator, message);
        }
    }
}
