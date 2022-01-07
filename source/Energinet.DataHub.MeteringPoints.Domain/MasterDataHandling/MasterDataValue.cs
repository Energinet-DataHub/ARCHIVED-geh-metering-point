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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterDataValue
    {
        private readonly List<ValidationError> _validationErrors = new();

        public MasterDataValue(string name, Type valueType, Applicability applicability)
        {
            Name = name;
            ValueType = valueType;
            Applicability = applicability;
        }

        public string Name { get; }

        public Type ValueType { get; }

        public Applicability Applicability { get; private set; }

        public object? Value { get; private set; }

        public IReadOnlyList<ValidationError> ValidationErrors => _validationErrors.AsReadOnly();

        public void SetValue<T>(T value)
        {
            Value = value;
        }

        public void SetValue<T>(Func<BusinessRulesValidationResult> validator, Func<T> creator)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (creator == null) throw new ArgumentNullException(nameof(creator));
            if (Applicability == Applicability.NotAllowed)
            {
                return;
            }

            var validationResult = validator.Invoke();
            if (validationResult.Success)
            {
                SetValue(creator.Invoke());
            }
            else
            {
                _validationErrors.AddRange(validationResult.Errors);
            }
        }

        public void SetApplicability(Applicability applicability)
        {
            Applicability = applicability;
        }

        public bool HasRequiredValue()
        {
            return !(Applicability == Applicability.Required && Value is null);
        }

        public bool HasErrors()
        {
            return _validationErrors.Count > 0;
        }
    }
}
