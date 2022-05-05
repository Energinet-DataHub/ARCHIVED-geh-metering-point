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
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands
{
    public class QueuedInternalCommand
    {
        public QueuedInternalCommand(Guid id, string type, string data, Instant creationDate, Instant? scheduleDate, string correlationId)
        {
            Id = id;
            Type = type;
            Data = data;
            CreationDate = creationDate;
            ScheduleDate = scheduleDate;
            CorrelationId = correlationId;
        }

        public Guid Id { get; }

        public string Type { get;  }

        public string Data { get; }

        public Instant CreationDate { get; }

        public Instant? ScheduleDate { get; }

        public Instant? ProcessedDate { get; set; }

        public string CorrelationId { get; }

        public string? Error { get; private set; }

        public void SetProcessed(Instant now)
        {
            ProcessedDate = now;
        }

        public InternalCommand ToCommand(IJsonSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            var storedCommandType = System.Type.GetType(Type, true);
            return (InternalCommand)serializer.Deserialize(Data, storedCommandType!);
        }
    }
}
