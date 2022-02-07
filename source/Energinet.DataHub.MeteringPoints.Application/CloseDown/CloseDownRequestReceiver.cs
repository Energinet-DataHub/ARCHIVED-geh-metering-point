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
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses.CloseDown;

namespace Energinet.DataHub.MeteringPoints.Application.CloseDown
{
    public class CloseDownRequestReceiver
    {
        private readonly IBusinessProcessRepository _businessProcesses;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActorMessageService _actorMessageService;

        public CloseDownRequestReceiver(IBusinessProcessRepository businessProcesses, IUnitOfWork unitOfWork, IActorMessageService actorMessageService)
        {
            _businessProcesses = businessProcesses ?? throw new ArgumentNullException(nameof(businessProcesses));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _actorMessageService = actorMessageService ?? throw new ArgumentNullException(nameof(actorMessageService));
        }
        #pragma warning disable

        public async Task ReceiveRequest(MasterDataDocument request)
        {
            var businessProcess = CloseDownProcess.Create(BusinessProcessId.Create(), request.TransactionId);
            _businessProcesses.Add(businessProcess);

            businessProcess.AcceptRequest();

            await CreateAcceptMessageAsync(request).ConfigureAwait(false);

            await _unitOfWork.CommitAsync();
        }

        private async Task CreateAcceptMessageAsync(MasterDataDocument request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await _actorMessageService
                .SendRequestCloseDownAcceptedAsync(request.TransactionId, request.GsrnNumber)
                .ConfigureAwait(false);
        }
    }
}
