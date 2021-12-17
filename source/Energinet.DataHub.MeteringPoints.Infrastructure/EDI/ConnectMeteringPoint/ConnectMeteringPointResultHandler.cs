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
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Parties;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint
{
    public sealed class ConnectMeteringPointResultHandler : IBusinessProcessResultHandler<ConnectMeteringPointRequest>
    {
        private readonly IActorMessageFactory _actorMessageFactory;
        private readonly IMessageHubDispatcher _messageHubDispatcher;
        private readonly ErrorMessageFactory _errorMessageFactory;
        private readonly ActorProvider _actorProvider;

        public ConnectMeteringPointResultHandler(
            IActorMessageFactory actorMessageFactory,
            IMessageHubDispatcher messageHubDispatcher,
            ErrorMessageFactory errorMessageFactory,
            ActorProvider actorProvider)
        {
            _actorMessageFactory = actorMessageFactory ?? throw new ArgumentNullException(nameof(actorMessageFactory));
            _messageHubDispatcher =
                messageHubDispatcher ?? throw new ArgumentNullException(nameof(messageHubDispatcher));
            _errorMessageFactory = errorMessageFactory ?? throw new ArgumentNullException(nameof(errorMessageFactory));
            _actorProvider = actorProvider ?? throw new ArgumentNullException(nameof(actorProvider));
        }

        public Task HandleAsync(ConnectMeteringPointRequest request, BusinessProcessResult result)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (result == null) throw new ArgumentNullException(nameof(result));

            return result.Success
                ? CreateAcceptMessageAsync(request)
                : CreateRejectResponseAsync(request, result);
        }

        private Task CreateAcceptMessageAsync(ConnectMeteringPointRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var message = _actorMessageFactory.ConnectMeteringPointConfirmation(request.GsrnNumber, request.EffectiveDate, request.TransactionId, _actorProvider.DataHub, _actorProvider.CurrentActor);
            return _messageHubDispatcher.DispatchAsync(message, DocumentType.ConnectMeteringPointAccepted, request.GsrnNumber);
        }

        private Task CreateRejectResponseAsync(ConnectMeteringPointRequest request, BusinessProcessResult result)
        {
            var errors = result.ValidationErrors
                .Select(error => _errorMessageFactory.GetErrorMessage(error))
                .AsEnumerable();

            var message = _actorMessageFactory.ConnectMeteringPointReject(request.GsrnNumber, request.EffectiveDate, request.TransactionId, errors, _actorProvider.DataHub, _actorProvider.CurrentActor);
            return _messageHubDispatcher.DispatchAsync(message, DocumentType.ConnectMeteringPointRejected, request.GsrnNumber);
        }
    }
}
