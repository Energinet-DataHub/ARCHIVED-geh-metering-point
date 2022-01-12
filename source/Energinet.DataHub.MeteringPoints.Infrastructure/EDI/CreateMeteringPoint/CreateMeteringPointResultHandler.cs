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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Actors;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint
{
    public class CreateMeteringPointResultHandler<TMeteringPoint> :
        IBusinessProcessResultHandler<TMeteringPoint>
        where TMeteringPoint : ICreateMeteringPointRequest
    {
        private readonly ErrorMessageFactory _errorMessageFactory;
        private readonly IActorMessageService _actorMessageService;

        public CreateMeteringPointResultHandler(
            ErrorMessageFactory errorMessageFactory,
            IActorMessageService actorMessageService)
        {
            _errorMessageFactory = errorMessageFactory;
            _actorMessageService = actorMessageService;
        }

        public Task HandleAsync(TMeteringPoint request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return result.Success
                ? CreateAcceptMessageAsync(request.GsrnNumber, request.TransactionId)
                : CreateRejectResponseAsync(request.GsrnNumber, request.TransactionId, result);
        }

        private async Task CreateAcceptMessageAsync(string gsrnNumber, string transactionId)
        {
            await _actorMessageService
                .SendCreateMeteringPointConfirmAsync(transactionId, gsrnNumber)
                .ConfigureAwait(false);
        }

        private async Task CreateRejectResponseAsync(string gsrnNumber, string transactionId, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .AsEnumerable();

            await _actorMessageService
                .SendCreateMeteringPointRejectAsync(transactionId, gsrnNumber, errors)
                .ConfigureAwait(false);
        }
    }
}
