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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.ChildMeteringPoints;
using Energinet.DataHub.MeteringPoints.Application.Providers;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Create
{
    public class CreateMeteringPointHandler : IBusinessRequestHandler<CreateMeteringPoint>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IGridAreaRepository _gridAreaRepository;
        private readonly IMediator _mediator;
        private readonly IActorLookup _actorLookup;
        private readonly CreateMeteringPointAuthorizer _authorizer;
        private readonly MasterDataValidator _masterDataValidator;
        private readonly ParentCouplingService _parentCouplingService;

        public CreateMeteringPointHandler(
            IMeteringPointRepository meteringPointRepository,
            IGridAreaRepository gridAreaRepository,
            IMediator mediator,
            IActorLookup actorLookup,
            CreateMeteringPointAuthorizer authorizer,
            MasterDataValidator masterDataValidator,
            ParentCouplingService parentCouplingService)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _gridAreaRepository = gridAreaRepository;
            _mediator = mediator;
            _actorLookup = actorLookup;
            _authorizer = authorizer;
            _parentCouplingService = parentCouplingService ?? throw new ArgumentNullException(nameof(parentCouplingService));
            _masterDataValidator = masterDataValidator ?? throw new ArgumentNullException(nameof(masterDataValidator));
        }

        public async Task<BusinessProcessResult> Handle(CreateMeteringPoint request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (await MeteringPointExistsAsync(request, cancellationToken).ConfigureAwait(false))
            {
                return Failure(request, new MeteringPointGsrnMustBeUniqueValidationError(request.GsrnNumber));
            }

            var gridArea = await GetGridAreaAsync(request).ConfigureAwait(false);
            if (gridArea is null)
            {
                return Failure(request, new GridAreaMustExistRuleError());
            }

            if (!await _actorLookup.ActorExistAsync(ActorId.Create(request.AdministratorId).Value).ConfigureAwait(false))
            {
                return Failure(request, new ActorDoesNotExistRuleError(request.AdministratorId));
            }

            var authorizationResult = await _authorizer.AuthorizeAsync(gridArea).ConfigureAwait(false);
            if (authorizationResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, authorizationResult.Errors);
            }

            var meteringPointType = EnumerationType.FromName<MeteringPointType>(request.MeteringPointType);
            var masterDataBuilder = MasterDataBuilderFor(request, meteringPointType);

            var masterDataValidationResult = masterDataBuilder.Validate();
            if (masterDataValidationResult.Success == false)
            {
                return Failure(request, masterDataValidationResult.Errors.ToArray());
            }

            var masterData = masterDataBuilder.Build();

            var creationValidationResult = MeteringPoint.CanCreate(meteringPointType, masterData, _masterDataValidator);
            if (creationValidationResult.Success == false)
            {
                return Failure(request, creationValidationResult.Errors.ToArray());
            }

            return await CreateMeteringPointAsync(request, meteringPointType, gridArea, masterData).ConfigureAwait(false);
        }

        private static BusinessProcessResult Failure(CreateMeteringPoint request, params ValidationError[] validationErrors)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new BusinessProcessResult(request.TransactionId, validationErrors);
        }

        private static MasterDataBuilder MasterDataBuilderFor(CreateMeteringPoint request, MeteringPointType meteringPointType)
        {
            var masterDataBuilder =
                new MasterDataBuilder(
                    new MasterDataFieldSelector().GetMasterDataFieldsFor(meteringPointType));

            masterDataBuilder
                .WithProductType(request.ProductType)
                .WithMeasurementUnitType(request.UnitType)
                .WithAssetType(request.AssetType)
                .WithConnectionType(request.ConnectionType)
                .WithDisconnectionType(request.DisconnectionType)
                .EffectiveOn(request.EffectiveDate)
                .WithMeteringConfiguration(request.MeteringMethod, request.MeterNumber)
                .WithSettlementMethod(request.SettlementMethod)
                .WithNetSettlementGroup(request.NetSettlementGroup!)
                .WithScheduledMeterReadingDate(request.ScheduledMeterReadingDate!)
                .WithReadingPeriodicity(request.MeterReadingOccurrence)
                .WithPowerLimit(request.MaximumPower, request.MaximumCurrent)
                .WithPowerPlant(request.PowerPlant)
                .WithCapacity(request.PhysicalConnectionCapacity)
                .WithAddress(
                    request.StreetName,
                    request.StreetCode,
                    request.BuildingNumber,
                    request.CityName,
                    request.CitySubDivisionName,
                    request.PostCode,
                    EnumerationType.FromName<CountryCode>(request.CountryCode),
                    request.FloorIdentification,
                    request.RoomIdentification,
                    string.IsNullOrWhiteSpace(request.MunicipalityCode) ? default : int.Parse(request.MunicipalityCode, NumberStyles.Integer, new NumberFormatInfo()),
                    request.IsActualAddress,
                    request.GeoInfoReference,
                    request.LocationDescription);
            return masterDataBuilder;
        }

        private async Task<BusinessProcessResult> CreateMeteringPointAsync(CreateMeteringPoint request, MeteringPointType meteringPointType, GridArea gridArea, MasterData masterData)
        {
            if (meteringPointType == MeteringPointType.Exchange)
            {
                return await CreateExchangeMeteringPointAsync(request, gridArea, masterData).ConfigureAwait(false);
            }

            var meteringPoint = MeteringPoint.Create(
                MeteringPointId.New(),
                GsrnNumber.Create(request.GsrnNumber),
                meteringPointType,
                gridArea.DefaultLink.Id,
                ActorId.Create(request.AdministratorId),
                masterData);

            _meteringPointRepository.Add(
                meteringPoint);

            var parentCouplingResult = await CoupleToParentIfRequestedAsync(request, meteringPoint).ConfigureAwait(false);
            return parentCouplingResult.Success == false ? parentCouplingResult : BusinessProcessResult.Ok(request.TransactionId);
        }

        private async Task<BusinessProcessResult> CoupleToParentIfRequestedAsync(CreateMeteringPoint request, MeteringPoint meteringPoint)
        {
            if (string.IsNullOrEmpty(request.ParentRelatedMeteringPoint))
                return BusinessProcessResult.Ok(request.TransactionId);

            var parent = await _meteringPointRepository.GetByGsrnNumberAsync(GsrnNumber.Create(request.ParentRelatedMeteringPoint)).ConfigureAwait(false);
            if (parent is null)
            {
                return BusinessProcessResult.Fail(request.TransactionId, new ParentMeteringPointWasNotFound());
            }

            var result = await _parentCouplingService.CoupleToParentAsync(meteringPoint, parent).ConfigureAwait(false);
            return new BusinessProcessResult(request.TransactionId, result.Errors);
        }

        private async Task<BusinessProcessResult> CreateExchangeMeteringPointAsync(CreateMeteringPoint request, GridArea gridArea, MasterData masterData)
        {
            if (request.ExchangeDetails?.SourceGridAreaCode is null ||
                request.ExchangeDetails?.TargetGridAreaCode is null)
            {
                throw new BusinessOperationException(
                    "Cannot create an Exchange metering point without specifying source and target grid areas.");
            }

            var sourceGridArea = await _gridAreaRepository.GetByCodeAsync(GridAreaCode.Create(request.ExchangeDetails.SourceGridAreaCode)).ConfigureAwait(false);
            var targetGridArea = await _gridAreaRepository.GetByCodeAsync(GridAreaCode.Create(request.ExchangeDetails.TargetGridAreaCode)).ConfigureAwait(false);

            var errors = new List<ValidationError>();
            if (sourceGridArea is null) errors.Add(new FromGridAreaMustExistRuleError(null));
            if (targetGridArea is null) errors.Add(new ToGridAreaMustExistRuleError(null));

            if (errors.Count > 0)
            {
                return Failure(
                    request, errors.ToArray());
            }

            _meteringPointRepository.Add(
                MeteringPoint.CreateExchange(
                    MeteringPointId.New(),
                    GsrnNumber.Create(request.GsrnNumber),
                    gridArea.DefaultLink.Id,
                    ExchangeGridAreas.Create(sourceGridArea!.DefaultLink.Id, targetGridArea!.DefaultLink.Id),
                    ActorId.Create(request.AdministratorId),
                    masterData));

            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private Task<GridArea?> GetGridAreaAsync(CreateMeteringPoint request)
        {
            return _gridAreaRepository.GetByCodeAsync(GridAreaCode.Create(request.MeteringGridArea));
        }

        private async Task<bool> MeteringPointExistsAsync(CreateMeteringPoint request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new MeteringPointGsrnExistsQuery(request.GsrnNumber), cancellationToken).ConfigureAwait(false);
        }
    }
}
