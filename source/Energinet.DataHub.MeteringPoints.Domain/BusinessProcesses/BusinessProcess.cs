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

using System;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses
{
    public class BusinessProcess : AggregateRootBase
    {
        private readonly string _transactionId;
        private readonly BusinessProcessType _processType;

        public BusinessProcess(BusinessProcessId id, string transactionId, BusinessProcessType processType)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _transactionId = transactionId ?? throw new ArgumentNullException(nameof(transactionId));
            _processType = processType ?? throw new ArgumentNullException(nameof(processType));
        }

        public BusinessProcessId Id { get; init; }

        public static BusinessProcess Create(BusinessProcessId id, string transactionId, BusinessProcessType processType)
        {
            return new BusinessProcess(id, transactionId, processType);
        }
    }
}
