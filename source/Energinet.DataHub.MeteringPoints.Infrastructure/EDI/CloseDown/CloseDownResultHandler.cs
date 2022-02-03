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
using Energinet.DataHub.MeteringPoints.Application.CloseDown;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CloseDown
{
    public class RequestCloseDownResultHandler : IBusinessProcessResultHandler<RequestCloseDown>
    {
        private readonly ErrorMessageFactory _errorMessageFactory;
        private readonly IActorMessageService _actorMessageService;

        public RequestCloseDownResultHandler(
            ErrorMessageFactory errorMessageFactory,
            IActorMessageService actorMessageService)
        {
            _errorMessageFactory = errorMessageFactory ?? throw new ArgumentNullException(nameof(errorMessageFactory));
            _actorMessageService = actorMessageService;
        }

        public Task HandleAsync(RequestCloseDown request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return result.Success
                ? CreateAcceptMessageAsync(request)
                : CreateRejectResponseAsync(request, result);
        }

        private async Task CreateAcceptMessageAsync(RequestCloseDown request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await _actorMessageService
                .SendRequestCloseDownAcceptedAsync(request.TransactionId, request.GsrnNumber)
                .ConfigureAwait(false);
        }

        private async Task CreateRejectResponseAsync(RequestCloseDown request, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .AsEnumerable();

            await _actorMessageService
                .SendRequestCloseDownRejectedAsync(request.TransactionId, request.GsrnNumber, errors)
                .ConfigureAwait(false);
        }
    }
}
