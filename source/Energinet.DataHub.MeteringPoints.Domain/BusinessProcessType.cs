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

using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain
{
    public class BusinessProcessType : EnumerationType
    {
        public static readonly BusinessProcessType CreateMeteringPoint = new BusinessProcessType(0, nameof(CreateMeteringPoint));
        public static readonly BusinessProcessType ConnectMeteringPoint = new BusinessProcessType(1, nameof(ConnectMeteringPoint));
        public static readonly BusinessProcessType ChangeMasterData = new BusinessProcessType(2, nameof(ChangeMasterData));
        public static readonly BusinessProcessType DisconnectReconnectMeteringPoint = new BusinessProcessType(3, nameof(DisconnectReconnectMeteringPoint));
        public static readonly BusinessProcessType CloseDownMeteringPoint = new BusinessProcessType(4, nameof(CloseDownMeteringPoint));

        public BusinessProcessType(int id, string name)
            : base(id, name)
        {
        }
    }
}
