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
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Authorization;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Users;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.Policies;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption
{
    public class ChangeMasterDataHandler : IBusinessRequestHandler<ChangeMasterDataRequest>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;
        private readonly ChangeMasterDataSettings _settings;
        private readonly IUserContext _authenticatedUserContext;
        private readonly IMeteringPointOwnershipProvider _ownershipProvider;

        public ChangeMasterDataHandler(IMeteringPointRepository meteringPointRepository, ISystemDateTimeProvider systemDateTimeProvider, ChangeMasterDataSettings settings, IUserContext authenticatedUserContext, IMeteringPointOwnershipProvider ownershipProvider)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _systemDateTimeProvider = systemDateTimeProvider ?? throw new ArgumentNullException(nameof(systemDateTimeProvider));
            _settings = settings;
            _authenticatedUserContext = authenticatedUserContext ?? throw new ArgumentNullException(nameof(authenticatedUserContext));
            _ownershipProvider = ownershipProvider ?? throw new ArgumentNullException(nameof(ownershipProvider));
        }

        public async Task<BusinessProcessResult> Handle(ChangeMasterDataRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (_authenticatedUserContext.CurrentUser is null)
            {
                throw new AuthenticationException("No authenticated user");
            }

            var authorizationHandler = new GridOperatorOwnsMeteringPointPolicy(_ownershipProvider);
            var authResult = await authorizationHandler.AuthorizeAsync(request.GsrnNumber, _authenticatedUserContext.CurrentUser.GlnNumber).ConfigureAwait(false);
            if (authResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, new List<ValidationError>()
                {
                    new GridOperatorIsNotOwnerOfMeteringPoint(request.GsrnNumber),
                });
            }

            var targetMeteringPoint = await FetchTargetMeteringPointAsync(request).ConfigureAwait(false);
            if (targetMeteringPoint == null)
            {
                return new BusinessProcessResult(request.TransactionId, new List<ValidationError>()
                {
                    new MeteringPointMustBeKnownValidationError(request.GsrnNumber),
                });
            }

            var validationResult = await ValidateAsync(request, targetMeteringPoint).ConfigureAwait(false);
            if (!validationResult.Success)
            {
                return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
            }

            return await ChangeMasterDataAsync(request, targetMeteringPoint).ConfigureAwait(false);
        }

        private static MasterDataDetails CreateChangeDetails(ChangeMasterDataRequest request, ConsumptionMeteringPoint targetMeteringPoint)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new MasterDataDetails(EffectiveDate.Create(request.EffectiveDate))
                with
                {
                    Address = CreateNewAddressFrom(request, targetMeteringPoint),
                };
        }

        private static Task<BusinessProcessResult> ChangeMasterDataAsync(ChangeMasterDataRequest request, ConsumptionMeteringPoint targetMeteringPoint)
        {
            targetMeteringPoint.Change(CreateChangeDetails(request, targetMeteringPoint));
            return Task.FromResult(BusinessProcessResult.Ok(request.TransactionId));
        }

        private static Domain.Addresses.Address? CreateNewAddressFrom(ChangeMasterDataRequest request, ConsumptionMeteringPoint targetMeteringPoint)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Address is null)
            {
                return null;
            }

            return targetMeteringPoint.Address.MergeFrom(Domain.Addresses.Address.Create(
                request.Address.StreetName,
                request.Address.StreetCode,
                request.Address.BuildingNumber,
                request.Address.City,
                request.Address.CitySubDivision,
                request.Address.PostCode,
                string.IsNullOrEmpty(request.Address.CountryCode) ? null : EnumerationType.FromName<CountryCode>(request.Address.CountryCode),
                request.Address.Floor,
                request.Address.Room,
                request.Address.MunicipalityCode,
                request.Address.IsActual.GetValueOrDefault(),
                request.Address.GeoInfoReference));
        }

        private Task<BusinessRulesValidationResult> ValidateAsync(ChangeMasterDataRequest request, ConsumptionMeteringPoint targetMeteringPoint)
        {
            var details = CreateChangeDetails(request, targetMeteringPoint);
            var validationResults = new List<BusinessRulesValidationResult>()
            {
                targetMeteringPoint.CanChange(details),
                new EffectiveDatePolicy(_settings.NumberOfDaysEffectiveDateIsAllowedToBeforeToday).Check(_systemDateTimeProvider.Now(), details.EffectiveDate),
            };

            var validationErrors = validationResults.SelectMany(results => results.Errors);

            return Task.FromResult(new BusinessRulesValidationResult(validationErrors));
        }

        private async Task<ConsumptionMeteringPoint?> FetchTargetMeteringPointAsync(ChangeMasterDataRequest request)
        {
            return await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false) as ConsumptionMeteringPoint;
        }
    }

#pragma warning disable
    public class GridOperatorOwnsMeteringPointPolicy
    {
        private readonly IMeteringPointOwnershipProvider _ownershipProvider;

        public GridOperatorOwnsMeteringPointPolicy(IMeteringPointOwnershipProvider ownershipProvider)
        {
            _ownershipProvider = ownershipProvider ?? throw new ArgumentNullException(nameof(ownershipProvider));
        }

        public async Task<AuthorizationResult> AuthorizeAsync(string gsrnNumber, string gridOperatorGlnNumber)
        {
            if (gsrnNumber == null) throw new ArgumentNullException(nameof(gsrnNumber));
            if (gridOperatorGlnNumber == null) throw new ArgumentNullException(nameof(gridOperatorGlnNumber));
            var ownerOfMeteringPoint = await _ownershipProvider.GetOwnerAsync(gsrnNumber).ConfigureAwait(false);
            if (ownerOfMeteringPoint.GlnNumber.Equals(gridOperatorGlnNumber, StringComparison.OrdinalIgnoreCase))
            {
                return AuthorizationResult.Ok();
            }

            return new AuthorizationResult(new List<ValidationError>()
            {
                new GridOperatorIsNotOwnerOfMeteringPoint(gsrnNumber),
            });
        }
    }

    public interface IMeteringPointOwnershipProvider
    {
        Task<Owner> GetOwnerAsync(string gsrnNumber);
    }

    public class MeteringPointOwnershipProvider : IMeteringPointOwnershipProvider
    {
        public Task<Owner> GetOwnerAsync(string gsrnNumber)
        {
            return Task.FromResult(new Owner("8200000001409"));
        }
    }

    public record Owner(string GlnNumber);

    public class GridOperatorIsNotOwnerOfMeteringPoint : ValidationError
    {
        public GridOperatorIsNotOwnerOfMeteringPoint(string gsrnNumber)
        {
            GsrnNumber = gsrnNumber;
        }

        public string GsrnNumber { get; }
    }
}
