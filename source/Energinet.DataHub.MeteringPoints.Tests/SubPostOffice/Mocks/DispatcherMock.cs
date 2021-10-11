﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common.Transport;
using Energinet.DataHub.MeteringPoints.Application.PostOffice;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;

namespace Energinet.DataHub.MeteringPoints.Tests.SubPostOffice.Mocks
{
    public class DispatcherMock : IMessageDispatcher
    {
        private readonly List<MessageReceived> _dispatchedCommands = new();

        public Task DispatchAsync(IOutboundMessage message, CancellationToken cancellationToken = default)
        {
            _dispatchedCommands.Add((MessageReceived)message);
            return Task.CompletedTask;
        }

        public bool IsDispatched(string correlation)
        {
            return _dispatchedCommands.Any(x => x.Correlation == correlation);
        }
    }
}
