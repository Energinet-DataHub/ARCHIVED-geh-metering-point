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

using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.BusinessProcesses
{
    public class BusinessProcessTests
    {
        [Fact]
        public void Request_is_accepted()
        {
            var businessProcess = BusinessProcess.Create(BusinessProcessId.Create(), "fakeid", BusinessProcessType.CloseDownMeteringPoint);

            businessProcess.AcceptRequest();

            var requestWasAcceptedEvent = businessProcess.DomainEvents.FirstOrDefault(e => e is RequestWasAccepted) as RequestWasAccepted;
            Assert.NotNull(requestWasAcceptedEvent);
            Assert.Equal("RequestWasAccepted", requestWasAcceptedEvent?.Status);
        }
    }
}
