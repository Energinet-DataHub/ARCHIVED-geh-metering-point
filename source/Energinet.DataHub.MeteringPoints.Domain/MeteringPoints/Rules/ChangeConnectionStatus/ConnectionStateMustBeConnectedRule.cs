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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules.ChangeConnectionStatus
{
    public class ConnectionStateMustBeConnectedRule : IBusinessRule
    {
        private readonly ConnectionState _connectionState;
        private readonly string _gsrnNumber;

        public ConnectionStateMustBeConnectedRule(ConnectionState connectionState, string gsrnNumber)
        {
            _connectionState = connectionState ?? throw new ArgumentNullException(nameof(connectionState));
            _gsrnNumber = gsrnNumber;
            IsBroken = !(connectionState.PhysicalState == PhysicalState.Connected);
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError => new ConnectionStateMustBeConnectedOrDisconnectedError(_gsrnNumber, _connectionState.PhysicalState);
    }
}
