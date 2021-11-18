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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption
{
    public class MasterDataValueUpdater
    {
        private readonly MasterDataDetails _masterDataDetails;
        private readonly ConsumptionMeteringPoint _targetMeteringPoint;
        private readonly List<ValidationError> _errors = new();

        public MasterDataValueUpdater(MasterDataDetails masterDataDetails, ConsumptionMeteringPoint targetMeteringPoint)
        {
            _masterDataDetails = masterDataDetails ?? throw new ArgumentNullException(nameof(masterDataDetails));
            _targetMeteringPoint = targetMeteringPoint ?? throw new ArgumentNullException(nameof(targetMeteringPoint));
            CheckIfValuesAreApplicable();
        }

        public bool CanUpdate => _errors.Count == 0;

        public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();

        public void Update()
        {
            if (CanUpdate == false)
            {
                throw new MasterDataChangeException(_errors);
            }

            if (_masterDataDetails.Address is not null)
            {
                _targetMeteringPoint.ChangeAddress(_masterDataDetails.Address);
            }

            if (_masterDataDetails.MeteringConfiguration is not null)
            {
                _targetMeteringPoint.ChangeMeteringConfiguration(_masterDataDetails.MeteringConfiguration, _masterDataDetails.EffectiveDate);
            }
        }

        private void CheckIfValuesAreApplicable()
        {
            if (_masterDataDetails.Address is not null)
            {
                _errors.AddRange(_targetMeteringPoint.CanChangeAddress(_masterDataDetails.Address).Errors);
            }
        }
    }
}
