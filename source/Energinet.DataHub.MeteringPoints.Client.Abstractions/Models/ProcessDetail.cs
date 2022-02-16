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
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums;

namespace Energinet.DataHub.MeteringPoints.Client.Abstractions.Models
{
    public record ProcessDetail
    {
        public ProcessDetail(
            Guid id,
            Guid processId,
            string name,
            string sender,
            string receiver,
            DateTime createdDate,
            DateTime? effectiveDate,
            ProcessStatus status,
            IReadOnlyList<ProcessDetailError> errors)
        {
            Id = id;
            ProcessId = processId;
            Name = name;
            Sender = sender;
            Receiver = receiver;
            CreatedDate = createdDate;
            EffectiveDate = effectiveDate;
            Status = status;
            Errors = errors;
        }

        public ProcessDetail(
            string name,
            string sender,
            string receiver,
            DateTime createdDate,
            DateTime? effectiveDate,
            ProcessStatus status,
            IReadOnlyList<ProcessDetailError> errors)
        {
            Id = Guid.NewGuid();
            Name = name;
            Sender = sender;
            Receiver = receiver;
            CreatedDate = createdDate;
            EffectiveDate = effectiveDate;
            Status = status;
            Errors = errors;
        }

        public ProcessDetail(
            string name,
            string sender,
            string receiver,
            DateTime createdDate,
            DateTime? effectiveDate,
            ProcessStatus status)
        {
            Id = Guid.NewGuid();
            Name = name;
            Sender = sender;
            Receiver = receiver;
            CreatedDate = createdDate;
            EffectiveDate = effectiveDate;
            Status = status;
            Errors = new List<ProcessDetailError>();
        }

        public ProcessDetail()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            Sender = string.Empty;
            Receiver = string.Empty;
            CreatedDate = DateTime.MinValue;
            EffectiveDate = null;
            Status = ProcessStatus.None;
            Errors = new List<ProcessDetailError>();
        }

        public Guid Id { get; init; }

        public Guid ProcessId { get; init; }

        public string Name { get; init; }

        public string Sender { get; init; }

        public string Receiver { get; init; }

        public DateTime CreatedDate { get; init; }

        public DateTime? EffectiveDate { get; init; }

        public ProcessStatus Status { get; init; }

        public IReadOnlyList<ProcessDetailError> Errors { get; init; }
    }
}
