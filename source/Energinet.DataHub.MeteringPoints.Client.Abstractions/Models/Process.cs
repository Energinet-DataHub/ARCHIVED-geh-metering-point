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
    public record Process
    {
        public Process(
            Guid id,
            string name,
            DateTime createdDate,
            DateTime? effectiveDate,
            ProcessStatus status,
            IReadOnlyList<ProcessDetail> details)
        {
            Id = id;
            Name = name;
            CreatedDate = createdDate;
            EffectiveDate = effectiveDate;
            Status = status;
            Details = details;
        }

        public Process(
            string name,
            DateTime createdDate,
            DateTime? effectiveDate,
            ProcessStatus status)
        {
            Id = Guid.NewGuid();
            Name = name;
            CreatedDate = createdDate;
            EffectiveDate = effectiveDate;
            Status = status;
            Details = new List<ProcessDetail>();
        }

        public Process()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            CreatedDate = DateTime.MinValue;
            EffectiveDate = null;
            Status = ProcessStatus.None;
            Details = new List<ProcessDetail>();
        }

        public Guid Id { get; init; }

        public string Name { get; init; }

        public DateTime CreatedDate { get; init; }

        public DateTime? EffectiveDate { get; init; }

        public ProcessStatus Status { get; init; }

        public IReadOnlyList<ProcessDetail> Details { get; init; }
    }
}
