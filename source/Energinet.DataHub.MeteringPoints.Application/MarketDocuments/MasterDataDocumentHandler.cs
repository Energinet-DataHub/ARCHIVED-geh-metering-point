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
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Messages;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.MarketDocuments
{
    public class MasterDataDocumentHandler : IRequestHandler<MasterDataDocument>
    {
        private readonly IValidator<MasterDataDocument> _validator;
        private readonly IMediator _mediator;
        private readonly IBusinessProcessValidationContext _validationContext;
        private readonly IBusinessProcessCommandFactory _commandFactory;
        private readonly IMessageReceiver _messageReceiver;

        public MasterDataDocumentHandler(IValidator<MasterDataDocument> validator, IMediator mediator, IBusinessProcessValidationContext validationContext, IBusinessProcessCommandFactory commandFactory, IMessageReceiver messageReceiver)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _validationContext = validationContext ?? throw new ArgumentNullException(nameof(validationContext));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _messageReceiver = messageReceiver ?? throw new ArgumentNullException(nameof(messageReceiver));
        }

        public async Task<Unit> Handle(MasterDataDocument request, CancellationToken cancellationToken)
        {
            await _messageReceiver.HandleAsync(request).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
