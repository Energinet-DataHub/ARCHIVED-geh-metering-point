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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI
{
    public sealed class DocumentType : EnumerationType
    {
        public static readonly DocumentType CreateMeteringPointAccepted = new(1, nameof(CreateMeteringPointAccepted), typeof(ConfirmMessage));
        public static readonly DocumentType CreateMeteringPointRejected = new(2, nameof(CreateMeteringPointRejected), typeof(RejectMessage));
        public static readonly DocumentType ConnectMeteringPointAccepted = new(3, nameof(ConnectMeteringPointAccepted), typeof(ConfirmMessage));
        public static readonly DocumentType ConnectMeteringPointRejected = new(4, nameof(ConnectMeteringPointRejected), typeof(RejectMessage));
        public static readonly DocumentType AccountingPointCharacteristicsMessage = new(5, nameof(AccountingPointCharacteristicsMessage), typeof(AccountingPointCharacteristicsMessage));

        private DocumentType(int id, string name, Type type)
            : base(id, name)
        {
            Type = type;
        }

        public Type Type { get; }
    }

    #pragma warning disable
    public enum DocumentType2
    {
        None = 0,
        CreateMeteringPointAccepted = 1,
        CreateMeteringPointRejected = 2,
        AccountingPointCharacteristicsMessage = 3,
        ConnectMeteringPointAccepted = 4,
        ConnectMeteringPointRejected = 5,
    }
}
