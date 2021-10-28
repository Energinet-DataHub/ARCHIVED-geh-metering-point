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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData
{
    public class ChangeMasterDataHandler : IBusinessRequestHandler<ChangeMasterDataRequest>
    {
        private IMeteringPointRepository _meteringPointRepository;
        private MeteringPoint? _targetMeteringPoint;
        private Address? _newAddress;

        public ChangeMasterDataHandler(IMeteringPointRepository meteringPointRepository)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
        }

        public async Task<BusinessProcessResult> Handle(ChangeMasterDataRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await InitializeAsync(request).ConfigureAwait(false);

            var preValidationResult = await PreValidateAsync(request).ConfigureAwait(false);
            if (preValidationResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, preValidationResult.Errors);
            }

            await PrepareAsync(request).ConfigureAwait(false);

            var validationResult = await ValidateAsync(request).ConfigureAwait(false);
            if (!validationResult.Success)
            {
                return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
            }

            return await ExecuteBusinessProcessAsync(request).ConfigureAwait(false);
        }

        private static Task<BusinessRulesValidationResult> PreValidateAsync(ChangeMasterDataRequest request)
        {
            var addressValidationResult = Address.CheckRules(
                request.StreetName,
                request.StreetCode,
                request.BuildingNumber,
                request.City,
                request.CitySubDivision,
                request.PostCode,
                request.CountryCode != null ? EnumerationType.FromName<CountryCode>(request.CountryCode) : null,
                request.Floor,
                request.Room,
                request.MunicipalityCode);

            return Task.FromResult(addressValidationResult);
        }

        private async Task InitializeAsync(ChangeMasterDataRequest request)
        {
            _targetMeteringPoint = await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false);
        }

        private Task<BusinessProcessResult> ExecuteBusinessProcessAsync(ChangeMasterDataRequest request)
        {
            _targetMeteringPoint!.ChangeAddress(_newAddress!);
            return Task.FromResult(BusinessProcessResult.Ok(request.TransactionId));
        }

        private Task PrepareAsync(ChangeMasterDataRequest request)
        {
            CreateNewAddressFrom(request);
            return Task.CompletedTask;
        }

        private void CreateNewAddressFrom(ChangeMasterDataRequest request)
        {
            _newAddress = Address.Create(
                request.StreetName,
                request.StreetCode,
                request.BuildingNumber,
                request.City,
                request.CitySubDivision,
                request.PostCode,
                request.CountryCode != null ? EnumerationType.FromName<CountryCode>(request.CountryCode) : null,
                request.Floor,
                request.Room,
                request.MunicipalityCode,
                request.IsActual.GetValueOrDefault(),
                request.GeoInfoReference);
        }

        private Task<BusinessRulesValidationResult> ValidateAsync(ChangeMasterDataRequest request)
        {
            var errors = new List<ValidationError>();

            if (_targetMeteringPoint == null)
            {
                errors.Add(new MeteringPointMustBeKnownValidationError(request.GsrnNumber));
            }
            else
            {
                errors.AddRange(_targetMeteringPoint.CanChangeAddress(_newAddress!).Errors);
            }

            return Task.FromResult(new BusinessRulesValidationResult(errors));
        }
    }
}
