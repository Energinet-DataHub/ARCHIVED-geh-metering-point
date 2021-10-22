﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint
{
    public class CreateMeteringPointResultHandler<TMeteringPoint> :
        IBusinessProcessResultHandler<TMeteringPoint>
        where TMeteringPoint : ICreateMeteringPointRequest
    {
        private readonly IActorMessageFactory _actorMessageFactory;
        private readonly IMessageHubDispatcher _messageHubDispatcher;
        private readonly ErrorMessageFactory _errorMessageFactory;

        public CreateMeteringPointResultHandler(
            IActorMessageFactory actorMessageFactory,
            IMessageHubDispatcher messageHubDispatcher,
            ErrorMessageFactory errorMessageFactory)
        {
            _actorMessageFactory = actorMessageFactory;
            _messageHubDispatcher = messageHubDispatcher;
            _errorMessageFactory = errorMessageFactory;
        }

        public Task HandleAsync(TMeteringPoint request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return result.Success
                ? CreateAcceptMessageAsync(request.GsrnNumber, request.EffectiveDate, request.TransactionId)
                : CreateRejectResponseAsync(request.GsrnNumber, request.EffectiveDate, request.TransactionId, result);
        }

        private Task CreateAcceptMessageAsync(string gsrnNumber, string effectiveDate, string transactionId)
        {
            var message = _actorMessageFactory.CreateNewMeteringPointConfirmation(gsrnNumber, effectiveDate, transactionId);
            return _messageHubDispatcher.DispatchAsync(message, DocumentType.CreateMeteringPointAccepted);
        }

        private Task CreateRejectResponseAsync(string gsrnNumber, string effectiveDate, string transactionId, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .AsEnumerable();

            var message = _actorMessageFactory.CreateNewMeteringPointReject(gsrnNumber, effectiveDate, transactionId, errors);
            return _messageHubDispatcher.DispatchAsync(message, DocumentType.CreateMeteringPointRejected);
        }
    }
}
