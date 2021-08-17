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
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;

namespace Energinet.DataHub.MeteringPoints.Application.Validation
{
    public static class CreateMeteringPointRulesHelper
    {
        public static bool MeteringPointTypeIsProductionOrConsumption(CreateMeteringPoint createMeteringPoint)
        {
            if (createMeteringPoint is null) throw new ArgumentNullException(nameof(createMeteringPoint));

            return createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Consumption.Name, StringComparison.Ordinal)
                   || createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Production.Name, StringComparison.Ordinal);
        }
    }
}
