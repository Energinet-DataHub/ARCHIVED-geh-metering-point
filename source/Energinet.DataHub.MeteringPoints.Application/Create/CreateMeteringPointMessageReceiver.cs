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
using Energinet.DataHub.MeteringPoints.Application.Common.Messages;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Create
{
    public class CreateMeteringPointMessageReceiver : MessageReceiver<MasterDataDocument>
    {
        private readonly ICreateMeteringPointInitiator _processInitiator;

        public CreateMeteringPointMessageReceiver(IMessageReceiver<MasterDataDocument> next, ICreateMeteringPointInitiator processInitiator)
            : base(next)
        {
            _processInitiator = processInitiator ?? throw new ArgumentNullException(nameof(processInitiator));
        }

        protected override bool ShouldHandle(MasterDataDocument message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var processType = EnumerationType.FromName<BusinessProcessType>(message.ProcessType);
            return processType == BusinessProcessType.CreateMeteringPoint;
        }

        protected override Task ProcessAsync(MasterDataDocument message)
        {
            return _processInitiator.HandleAsync(message);
        }
    }
}
