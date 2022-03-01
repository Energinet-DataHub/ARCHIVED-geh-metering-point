// Copyright 2022 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing
{
    public class BusinessProcessResultHandlerRejectReasonDecorator<TMeteringPoint> : IBusinessProcessResultHandler<TMeteringPoint>
        where TMeteringPoint : IBusinessRequest
    {
        private readonly IBusinessProcessResultHandler<TMeteringPoint> _decoratedProcessHandler;
        private readonly ILogger _logger;

        public BusinessProcessResultHandlerRejectReasonDecorator(IBusinessProcessResultHandler<TMeteringPoint> decoratedProcessHandler, ILogger logger)
        {
            _decoratedProcessHandler = decoratedProcessHandler;
            _logger = logger;
        }

        public Task HandleAsync(TMeteringPoint request, BusinessProcessResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            foreach (var resultValidationError in result.ValidationErrors)
            {
                _logger.LogInformation("Reject reason: {Message}", resultValidationError);
            }

            return Task.CompletedTask;
        }
    }
}
