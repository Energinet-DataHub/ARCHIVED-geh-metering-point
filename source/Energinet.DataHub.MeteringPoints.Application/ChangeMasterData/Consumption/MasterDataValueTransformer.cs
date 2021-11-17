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
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption
{
    public class MasterDataValueTransformer
    {
        private readonly ChangeMasterDataRequest _request;
        private readonly ConsumptionMeteringPoint _targetMeteringPoint;
        private readonly List<ValidationError> _errors = new();
        private readonly MasterDataDetails _updatedValues;

        public MasterDataValueTransformer(ChangeMasterDataRequest request, ConsumptionMeteringPoint targetMeteringPoint)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _targetMeteringPoint = targetMeteringPoint ?? throw new ArgumentNullException(nameof(targetMeteringPoint));
            _updatedValues = CreateUpdatedValues();
        }

        public bool HasError => _errors.Count > 0;

        public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();

        public MasterDataDetails UpdatedValues() => _updatedValues;

        private MasterDataDetails CreateUpdatedValues()
        {
            return new MasterDataDetails(
                EffectiveDate: TryCreateEffectiveDate(),
                Address: TryCreateAddress(),
                MeteringConfiguration: TryCreateMeteringConfiguration());
        }

        private MeteringConfiguration? TryCreateMeteringConfiguration()
        {
            if (_request.MeterId is null && _request.MeteringMethod is null) return null;

            var currentConfiguration = _targetMeteringPoint.GetMeteringConfiguration();
            var meteringMethod = _request.MeteringMethod is null ? currentConfiguration.Method : EnumerationType.FromName<MeteringMethod>(_request.MeteringMethod);
            var meterId = _request.MeterId is null
                ? currentConfiguration.Meter
                : meteringMethod == MeteringMethod.Physical ? MeterId.Create(_request.MeterId) : MeterId.Empty();

            var checkResult = MeteringConfiguration.CheckRules(meteringMethod, meterId);
            if (checkResult.Success)
            {
                return MeteringConfiguration.Create(meteringMethod, meterId);
            }

            _errors.AddRange(checkResult.Errors);
            return null;
        }

        private Domain.Addresses.Address? TryCreateAddress()
        {
            if (_request.Address is null)
            {
                return null;
            }

            var address = Domain.Addresses.Address.Create(
                _request.Address.StreetName,
                _request.Address.StreetCode,
                _request.Address.BuildingNumber,
                _request.Address.City,
                _request.Address.CitySubDivision,
                _request.Address.PostCode,
                string.IsNullOrEmpty(_request.Address.CountryCode) ? null : EnumerationType.FromName<CountryCode>(_request.Address.CountryCode),
                _request.Address.Floor,
                _request.Address.Room,
                _request.Address.MunicipalityCode,
                _request.Address.IsActual.GetValueOrDefault(),
                _request.Address.GeoInfoReference);

            return _targetMeteringPoint.Address.MergeFrom(address);
        }

        private EffectiveDate TryCreateEffectiveDate()
        {
            return EffectiveDate.Create(_request.EffectiveDate);
        }
    }
}