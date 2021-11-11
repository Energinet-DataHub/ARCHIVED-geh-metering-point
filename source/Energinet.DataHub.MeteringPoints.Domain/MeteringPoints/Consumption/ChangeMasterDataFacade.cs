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
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption
{
    public class ChangeMasterDataFacade
    {
        private readonly ConsumptionMeteringPoint _target;
        private readonly MasterDataDetails _details;

        public ChangeMasterDataFacade(ConsumptionMeteringPoint target, MasterDataDetails details)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _details = details ?? throw new ArgumentNullException(nameof(details));
        }

        public BusinessRulesValidationResult CanChange()
        {
            var validationErrors = new List<ValidationError>();
            if (_details.Address is not null)
            {
                validationErrors.AddRange(_target.CanChangeAddress(_details.Address).Errors);
            }

            if (_details.MeterId is not null || _details.MeteringMethod is not null)
            {
                var meteringMethod = _details.MeteringMethod ?? _target.MeteringConfiguration.Method;
                MeterId meterId;
                if (meteringMethod == MeteringMethod.Physical)
                {
                    meterId = _details.MeterId ?? _target.MeteringConfiguration.Meter;
                }
                else
                {
                    meterId = MeterId.Empty();
                }

                var checkResult = MeteringConfiguration.CheckRules(meteringMethod, meterId!);
                validationErrors.AddRange(checkResult.Errors);
            }

            return new BusinessRulesValidationResult(validationErrors);
        }

        public void Change()
        {
            var checkResult = CanChange();
            if (checkResult.Success == false)
            {
                throw new MasterDataChangeException(checkResult.Errors.ToList());
            }

            if (_details.Address is not null)
            {
                _target.ChangeAddress(_details.Address);
            }

            if (_details.MeterId is not null || _details.MeteringMethod is not null)
            {
                var meteringMethod = _details.MeteringMethod ?? _target.MeteringConfiguration.Method;
                MeterId meterId;
                if (meteringMethod == MeteringMethod.Physical)
                {
                    meterId = _details.MeterId ?? _target.MeteringConfiguration.Meter;
                }
                else
                {
                    meterId = MeterId.Empty();
                }

                _target.ChangeMeteringConfiguration(
                    MeteringConfiguration.Create(meteringMethod, meterId),
                    _details.EffectiveDate);
            }

            // _details.Address ?? _target.ChangeAddress(_details.Address);
            // _target.Change(_details);
        }
    }
}
