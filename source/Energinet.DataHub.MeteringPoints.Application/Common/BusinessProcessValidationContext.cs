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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Common
{
    public class BusinessProcessValidationContext : IBusinessProcessValidationContext
    {
        private readonly List<ValidationError> _validationErrors = new();

        public bool HasErrors => _validationErrors.Count > 0;

        public void Add(IEnumerable<ValidationError> validationErrors)
        {
            _validationErrors.AddRange(validationErrors);
        }

        public IEnumerable<ValidationError> GetErrors()
        {
            return _validationErrors.AsReadOnly();
        }

        public async Task ValidateAsync<TMessage>(IValidator<TMessage> validator, TMessage message)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (message == null) throw new ArgumentNullException(nameof(message));
            var validationResult = await validator.ValidateAsync(message).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult
                    .Errors
                    .Select(error => (ValidationError)error.CustomState)
                    .ToList()
                    .AsReadOnly();

                Add(validationErrors);
            }
        }
    }
}
