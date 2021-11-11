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
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption
{
    public class ChangeMasterDataFacade
    {
        private readonly ConsumptionMeteringPoint _target;
        private readonly MasterDataDetails _details;
        private readonly List<ValidationError> _validationErrors = new();
        private Address? _newAddress;
        private MeteringConfiguration? _newMeteringConfiguration;

        public ChangeMasterDataFacade(ConsumptionMeteringPoint target, MasterDataDetails details)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _details = details ?? throw new ArgumentNullException(nameof(details));
            CreateChangedValues();
        }

        public BusinessRulesValidationResult CanChange()
        {
            return new BusinessRulesValidationResult(_validationErrors);
        }

        public void Change()
        {
            var checkResult = CanChange();
            if (checkResult.Success == false)
            {
                throw new MasterDataChangeException(checkResult.Errors.ToList());
            }

            SetAddressIfChanged();
            SetMeteringConfigurationIfChanged();
        }

        private void CreateChangedValues()
        {
            CreateAndValidateNewAddress();
            CreateAndValidateMeteringConfiguration();
        }

        private void CreateAndValidateNewAddress()
        {
            if (_details.Address is null) return;
            var address = _target.Address.MergeFrom(_details.Address);
            var checkResult = _target.CanChangeAddress(address);
            if (checkResult.Success == false)
            {
                _validationErrors.AddRange(checkResult.Errors);
            }
            else
            {
                _newAddress = address;
            }
        }

        private void SetAddressIfChanged()
        {
            if (_newAddress is not null)
            {
                _target.ChangeAddress(_newAddress);
            }
        }

        private void CreateAndValidateMeteringConfiguration()
        {
            if (_details.MeterId is null && _details.MeteringMethod is null) return;

            var meteringMethod = _details.MeteringMethod ?? _target.MeteringConfiguration.Method;
            MeterId meterId = meteringMethod == MeteringMethod.Physical
                ? _details.MeterId ?? _target.MeteringConfiguration.Meter
                : MeterId.Empty();

            var checkResult = MeteringConfiguration.CheckRules(meteringMethod, meterId);
            if (checkResult.Success)
            {
                _newMeteringConfiguration = MeteringConfiguration.Create(meteringMethod, meterId);
            }
            else
            {
                _validationErrors.AddRange(checkResult.Errors);
            }
        }

        private void SetMeteringConfigurationIfChanged()
        {
            if (_newMeteringConfiguration is not null)
            {
                _target.ChangeMeteringConfiguration(_newMeteringConfiguration, _details.EffectiveDate);
            }
        }
    }
}
