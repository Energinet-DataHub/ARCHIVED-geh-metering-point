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
        private Domain.Addresses.Address? _newAddress;

        public ChangeMasterDataHandler(IMeteringPointRepository meteringPointRepository)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
        }

        public async Task<BusinessProcessResult> Handle(ChangeMasterDataRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await InitializeAsync(request).ConfigureAwait(false);

            await PrepareAsync(request).ConfigureAwait(false);

            var validationResult = await ValidateAsync(request).ConfigureAwait(false);
            if (!validationResult.Success)
            {
                return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
            }

            return await ExecuteBusinessProcessAsync(request).ConfigureAwait(false);
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
            if (request.Address != null)
            {
                _newAddress = Domain.Addresses.Address.Create(
                    request.Address.StreetName,
                    request.Address.StreetCode,
                    request.Address.BuildingNumber,
                    request.Address.City,
                    request.Address.CitySubDivision,
                    request.Address.PostCode,
                    request.Address.CountryCode != null ? EnumerationType.FromName<CountryCode>(request.Address.CountryCode) : null,
                    request.Address.Floor,
                    request.Address.Room,
                    request.Address.MunicipalityCode,
                    request.Address.IsActual.GetValueOrDefault(),
                    request.Address.GeoInfoReference);
            }
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
