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

using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses.Rules
{
    public class CitySubdivisionRuleError : ValidationError
    {
        public CitySubdivisionRuleError()
        {
            CitySubdivision = string.Empty;
            SetErrorProperties();
        }

        public CitySubdivisionRuleError(string citySubdivision)
        {
            CitySubdivision = citySubdivision;
            SetErrorProperties();
        }

        private string CitySubdivision { get; }

        private void SetErrorProperties()
        {
            Code = "E86";
            Message = $"City sub division name {CitySubdivision} has a length that exceeds 34.";
        }
    }
}
