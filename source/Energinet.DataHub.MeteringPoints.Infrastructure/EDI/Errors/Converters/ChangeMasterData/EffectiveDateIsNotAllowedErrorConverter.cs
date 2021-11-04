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

using Energinet.DataHub.MeteringPoints.Application.ChangeMasterData;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters.ChangeMasterData
{
    public class EffectiveDateIsNotAllowedErrorConverter : ErrorConverter<EffectiveDateNotAllowed>
    {
        protected override ErrorMessage Convert(EffectiveDateNotAllowed validationError)
        {
            return new ErrorMessage("E17", "Effectuation date incorrect: The information is not received within the correct time period (either too soon or too late).");
        }
    }
}
