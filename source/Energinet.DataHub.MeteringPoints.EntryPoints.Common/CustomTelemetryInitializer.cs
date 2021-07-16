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

using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Common
{
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize([NotNull] ITelemetry telemetry)
        {
            if (string.IsNullOrWhiteSpace(CorrelationIdContext.CorrelationId))
            {
                return;
            }

            if (telemetry is ISupportProperties itemWithProperties && !itemWithProperties.Properties.ContainsKey("CorrelationId"))
            {
                itemWithProperties.Properties["CorrelationId"] = CorrelationIdContext.CorrelationId;
            }

            telemetry.Context.Operation.Id = CorrelationIdContext.CorrelationId;
        }
    }
}
