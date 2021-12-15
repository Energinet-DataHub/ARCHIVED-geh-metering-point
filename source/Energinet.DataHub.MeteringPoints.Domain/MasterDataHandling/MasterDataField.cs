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

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterDataField
    {
        public MasterDataField(string name, Applicability applicability, bool canBeChanged = true, object? defaultValue = null)
        {
            Name = name;
            Applicability = applicability;
            CanBeChanged = canBeChanged;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        public Applicability Applicability { get; }

        public bool CanBeChanged { get; }

        public object? DefaultValue { get; }
    }
}
