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
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.Connect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Helpers
{
    public static class OutboxTypeFactory
    {
        private static readonly Dictionary<string, Type> _types = new()
        {
            { typeof(MeteringPointCreatedEventMessage).FullName!, typeof(MeteringPointCreatedEventMessage) },
            { typeof(ConsumptionMeteringPointCreatedIntegrationEvent).FullName!, typeof(ConsumptionMeteringPointCreatedIntegrationEvent) },
            { typeof(MeteringPointConnectedIntegrationEvent).FullName!, typeof(MeteringPointConnectedIntegrationEvent) },
            { typeof(PostOfficeMessageEnvelope).FullName!, typeof(PostOfficeMessageEnvelope) },
        };

        public static Type GetType(string type)
        {
            return _types.TryGetValue(type, out var result)
                ? result
                : throw new ArgumentException("Outbox type is not implemented.");
        }
    }
}
