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
using Energinet.DataHub.MeteringPoints.Application.Common.NotificationEvents;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Events.Incoming
{
    public class UnitOfWorkHandlerDecorator<T> : INotificationHandler<T>
        where T : INotificationEvent
    {
        private readonly INotificationHandler<T> _decoratedHandler;
        private readonly IUnitOfWork _unitOfWork;

        public UnitOfWorkHandlerDecorator(INotificationHandler<T> decoratedHandler, IUnitOfWork unitOfWork)
        {
            _decoratedHandler = decoratedHandler ?? throw new ArgumentNullException(nameof(decoratedHandler));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task Handle(T notification, CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            await _decoratedHandler.Handle(notification, cancellationToken);
            await _unitOfWork.CommitAsync();
        }
    }
}
