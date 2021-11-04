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
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Connect
{
    public class EnergySupplierChangedHandler : INotificationHandler<EnergySupplierChanged>
    {
        private readonly ICommandScheduler _commandScheduler;

        public EnergySupplierChangedHandler(ICommandScheduler commandScheduler)
        {
            _commandScheduler = commandScheduler ?? throw new ArgumentNullException(nameof(commandScheduler));
        }

        public Task Handle(EnergySupplierChanged notification, CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            var command = new SetEnergySupplierInfo(notification.GsrnNumber, notification.StartOfSupply, notification.GlnNumber);
            return _commandScheduler.EnqueueAsync(command);
        }
    }
}
