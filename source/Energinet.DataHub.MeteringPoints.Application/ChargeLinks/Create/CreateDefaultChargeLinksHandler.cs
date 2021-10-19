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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Models;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.ChargeLinks.Create
{
    public class CreateDefaultChargeLinksHandler : IRequestHandler<CreateDefaultChargeLinks>
    {
        private readonly DefaultChargeLinkRequestClient _defaultChargeLinkRequestClient;

        public CreateDefaultChargeLinksHandler(DefaultChargeLinkRequestClient defaultChargeLinkRequestClient)
        {
            _defaultChargeLinkRequestClient = defaultChargeLinkRequestClient;
        }

        public async Task<Unit> Handle(CreateDefaultChargeLinks request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            // TODO: On some point add this to some process manager
            await _defaultChargeLinkRequestClient.CreateDefaultChargeLinksRequestAsync(new CreateDefaultChargeLinksDto(request.GsrnNumber), request.CorrelationId).ConfigureAwait(false);

            return default;
        }
    }
}
