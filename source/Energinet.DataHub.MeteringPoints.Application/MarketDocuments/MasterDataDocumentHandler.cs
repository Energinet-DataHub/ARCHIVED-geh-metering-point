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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.CloseDown;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.ReceiveBusinessRequests;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.MarketDocuments
{
    public class MasterDataDocumentHandler : IRequestHandler<MasterDataDocument, BusinessProcessResult>
    {
        private readonly IValidator<MasterDataDocument> _validator;
        private readonly IMediator _mediator;
        private readonly IBusinessProcessValidationContext _validationContext;
        private readonly IBusinessProcessCommandFactory _commandFactory;
        private readonly RequestReceiver _requestReceiver;

        public MasterDataDocumentHandler(
            IValidator<MasterDataDocument> validator,
            IMediator mediator,
            IBusinessProcessValidationContext validationContext,
            IBusinessProcessCommandFactory commandFactory,
            RequestReceiver requestReceiver)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _validationContext = validationContext ?? throw new ArgumentNullException(nameof(validationContext));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _requestReceiver = requestReceiver ?? throw new ArgumentNullException(nameof(requestReceiver));
        }

        public async Task<BusinessProcessResult> Handle(MasterDataDocument request, CancellationToken cancellationToken)
        {
            if (_requestReceiver.CanHandleRequest(request))
            {
                await _requestReceiver.ReceiveRequestAsync(request).ConfigureAwait(false);
                return BusinessProcessResult.Ok(request.TransactionId);
            }

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult
                    .Errors
                    .Select(error => (ValidationError)error.CustomState)
                    .ToList()
                    .AsReadOnly();

                _validationContext.Add(validationErrors);
            }

            var businessProcessCommand = _commandFactory.CreateFrom(request);
            var result = await _mediator.Send(businessProcessCommand!, cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}
