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
using Energinet.DataHub.MeteringPoints.Application.Common.Messages;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Connect
{
    public class ConnectMeteringPointMessageReceiver : IMessageReceiver
    {
        private readonly IMediator _mediator;
        private readonly IMessageReceiver _next;
        private readonly IValidator<MasterDataDocument> _validator;
        private readonly IBusinessProcessValidationContext _validationContext;

        public ConnectMeteringPointMessageReceiver(IMediator mediator, IMessageReceiver next, IValidator<MasterDataDocument> validator, IBusinessProcessValidationContext validationContext)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _next = next;
            _validator = validator;
            _validationContext = validationContext ?? throw new ArgumentNullException(nameof(validationContext));
        }

        public Task HandleAsync(MasterDataDocument message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return HandleOrCallNextAsync(message);
        }

        private static ConnectMeteringPoint CreateCommandFrom(MasterDataDocument document)
        {
            return new ConnectMeteringPoint(document.GsrnNumber, document.EffectiveDate, document.TransactionId);
        }

        private Task HandleOrCallNextAsync(MasterDataDocument message)
        {
            var processType = EnumerationType.FromName<BusinessProcessType>(message.ProcessType);
            return processType == BusinessProcessType.ConnectMeteringPoint ? HandleInternalAsync(message) : _next?.HandleAsync(message)!;
        }

        private async Task HandleInternalAsync(MasterDataDocument message)
        {
            var validationResult = await _validator.ValidateAsync(message).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult
                    .Errors
                    .Select(error => (ValidationError)error.CustomState)
                    .ToList()
                    .AsReadOnly();

                _validationContext.Add(validationErrors);
            }

            await _mediator.Send(CreateCommandFrom(message)).ConfigureAwait(false);
        }
    }
}
