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
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;
using MeteringPointCreated = Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events.MeteringPointCreated;

namespace Energinet.DataHub.MeteringPoints.Application.Create
{
    // TODO: This is a hack used during the test period and should be removed as soon as the business processes from Market roles are included
    public class SetEnergySupplierHACK : INotificationHandler<MeteringPointCreated>
    {
        private readonly ICommandScheduler _commandScheduler;
        private readonly IActorMessageService _actorMessageService;
        private readonly IActorContext _actorContext;

        public SetEnergySupplierHACK(
            ICommandScheduler commandScheduler,
            IActorMessageService actorMessageService,
            IActorContext actorContext)
        {
            _commandScheduler = commandScheduler ?? throw new ArgumentNullException(nameof(commandScheduler));
            _actorMessageService = actorMessageService;
            _actorContext = actorContext;
        }

        public async Task Handle(MeteringPointCreated notification, CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            if (EnumerationType.FromName<MeteringPointType>(notification.MeteringPointType).IsAccountingPoint)
            {
                await _commandScheduler.EnqueueAsync(new SetEnergySupplierInfo(
                    notification.GsrnNumber,
                    notification.EffectiveDate)).ConfigureAwait(false);

                await _actorMessageService
                    .SendGenericNotificationMessageAsync(
                        Guid.NewGuid().ToString(),
                        notification.GsrnNumber,
                        notification.EffectiveDate,
                        _actorContext.CurrentActor!.Identifier)
                    .ConfigureAwait(false);
            }
        }
    }
}
