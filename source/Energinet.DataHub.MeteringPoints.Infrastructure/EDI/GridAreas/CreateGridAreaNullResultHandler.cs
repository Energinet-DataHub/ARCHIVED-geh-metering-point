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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.GridAreas.Create;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GridAreas
{
    public sealed class CreateGridAreaNullResultHandler : IBusinessProcessResultHandler<CreateGridArea>
    {
        private readonly List<ValidationError> _errors = new();

        public IReadOnlyCollection<ValidationError> Errors => _errors;

        public Task HandleAsync(CreateGridArea request, BusinessProcessResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            if (!result.Success)
            {
                _errors.AddRange(result.ValidationErrors);
            }

            return Task.CompletedTask;
        }
    }
}
