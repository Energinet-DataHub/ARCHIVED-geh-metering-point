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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint
{
    public class CreateMeteringPointNullResultHandler : IBusinessProcessResultHandler<Application.CreateMeteringPoint>
    {
        // Note: Logger is only meant for the null implementation.
        private readonly ILogger _logger;
        private readonly ErrorMessageFactory _errorMessageFactory;

        public CreateMeteringPointNullResultHandler(
            ILogger logger,
            ErrorMessageFactory errorMessageFactory)
        {
            _logger = logger;
            _errorMessageFactory = errorMessageFactory;
        }

        public Task HandleAsync(Application.CreateMeteringPoint request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            if (result.Success)
            {
                _logger.LogInformation($"Great success in {nameof(CreateMeteringPointNullResultHandler)}");
            }
            else
            {
                _logger.LogInformation($"Minor setback in {nameof(CreateMeteringPointNullResultHandler)}");
                foreach (var validationError in result.ValidationErrors)
                {
                    var errorMessage = _errorMessageFactory.GetErrorMessage(validationError);
                    _logger.LogInformation("Error: {errorMessage}", errorMessage);
                }
            }

            // TODO: put result in outbox
            return Task.CompletedTask;
        }
    }
}
