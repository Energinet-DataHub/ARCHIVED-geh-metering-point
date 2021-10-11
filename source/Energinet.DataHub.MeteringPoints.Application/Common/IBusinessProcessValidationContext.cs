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

using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Common
{
    /// <summary>
    /// Service that contains validation error raised during a business process
    /// </summary>
    public interface IBusinessProcessValidationContext
    {
        /// <summary>
        /// Indicates if any error has been registered
        /// </summary>
        bool HasErrors { get; }

        /// <summary>
        /// Add validation errors to context
        /// </summary>
        /// <param name="validationErrors"></param>
        void Add(IEnumerable<ValidationError> validationErrors);

        /// <summary>
        /// Registered errors
        /// </summary>
        /// <returns><see cref="ValidationError"/></returns>
        IEnumerable<ValidationError> GetErrors();
    }
}
