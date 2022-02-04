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

using System.Runtime.Intrinsics.Arm;

namespace Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses.CloseDown
{
    public class CloseDownProcess : BusinessProcess
    {
        private Status _status;

        public CloseDownProcess(BusinessProcessId id, string transactionId, BusinessProcessType processType)
            : base(id, transactionId, processType)
        {
            _status = Status.NotStarted;
        }

        public enum Status
        {
            NotStarted,
            RequestWasAccepted,
            RequestWasRejected,
            PendingMarketRoles,
        }

        public static CloseDownProcess Create(BusinessProcessId id, string transactionId)
        {
            return new CloseDownProcess(id, transactionId, BusinessProcessType.CloseDownMeteringPoint);
        }

        public void SetPending()
        {
            _status = Status.PendingMarketRoles;
        }

        public void AcceptRequest()
        {
            if (_status != Status.NotStarted)
            {
                throw new InvalidBusinessProcessStateException();
            }

            _status = Status.RequestWasAccepted;
            AddDomainEvent(new RequestWasAccepted(Id.Value, TransactionId, ProcessType.Name));
        }

        public void RejectRequest()
        {
            if (_status != Status.NotStarted)
            {
                throw new InvalidBusinessProcessStateException();
            }

            _status = Status.RequestWasRejected;
            AddDomainEvent(new RequestWasRejected(Id.Value, TransactionId, ProcessType.Name));
        }
    }
}
