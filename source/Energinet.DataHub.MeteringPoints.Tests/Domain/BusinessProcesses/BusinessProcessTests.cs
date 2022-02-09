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
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses.CloseDown;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.BusinessProcesses
{
    public class BusinessProcessTests
    {
        [Fact]
        public void Request_is_accepted()
        {
            var businessProcess = CreateProcess();

            businessProcess.AcceptRequest();

            var requestWasAcceptedEvent = businessProcess.DomainEvents.FirstOrDefault(e => e is CloseDownRequestWasAccepted) as CloseDownRequestWasAccepted;
            Assert.NotNull(requestWasAcceptedEvent);
        }

        [Fact]
        public void Request_is_rejected()
        {
            var businessProcess = CreateProcess();

            businessProcess.RejectRequest();

            var domainEvent = businessProcess.DomainEvents.FirstOrDefault(e => e is CloseDownRequestWasRejected) as CloseDownRequestWasRejected;
            Assert.NotNull(domainEvent);
        }

        [Fact]
        public void Can_not_reject_request_when_is_other_than_not_started()
        {
            var businessProcess = CreateProcess();
            businessProcess.AcceptRequest();

            Assert.Throws<InvalidBusinessProcessStateException>(() => businessProcess.RejectRequest());
        }

        [Fact]
        public void Can_not_accept_request_when_state_is_other_than_not_started()
        {
            var businessProcess = CreateProcess();
            businessProcess.AcceptRequest();

            Assert.Throws<InvalidBusinessProcessStateException>(() => businessProcess.AcceptRequest());
        }

        private static CloseDownProcess CreateProcess()
        {
            return CloseDownProcess.Create(BusinessProcessId.Create(), Guid.NewGuid().ToString());
        }
    }
}
