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
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GenericNotification;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI
{
    public sealed class DocumentType : EnumerationType
    {
        public static readonly DocumentType ConfirmCreateMeteringPoint = new(1, nameof(ConfirmCreateMeteringPoint), typeof(ConfirmMessage));
        public static readonly DocumentType RejectCreateMeteringPoint = new(2, nameof(RejectCreateMeteringPoint), typeof(RejectMessage));
        public static readonly DocumentType ConfirmConnectMeteringPoint = new(3, nameof(ConfirmConnectMeteringPoint), typeof(ConfirmMessage));
        public static readonly DocumentType RejectConnectMeteringPoint = new(4, nameof(RejectConnectMeteringPoint), typeof(RejectMessage));
        public static readonly DocumentType AccountingPointCharacteristicsMessage = new(5, nameof(AccountingPointCharacteristicsMessage), typeof(AccountingPointCharacteristicsMessage));
        public static readonly DocumentType ConfirmChangeMasterData = new(6, nameof(ConfirmChangeMasterData), typeof(ConfirmMessage));
        public static readonly DocumentType RejectChangeMasterData = new(7, nameof(RejectChangeMasterData), typeof(RejectMessage));
        public static readonly DocumentType GenericNotification = new(8, nameof(GenericNotification), typeof(GenericNotificationMessage));
        public static readonly DocumentType ConfirmDisconnectMeteringPoint = new(9, nameof(ConfirmDisconnectMeteringPoint), typeof(ConfirmMessage));
        public static readonly DocumentType RejectDisconnectMeteringPoint = new(10, nameof(RejectDisconnectMeteringPoint), typeof(RejectMessage));

        public DocumentType(int id, string name)
            : base(id, name)
        {
            var documentType = FromName<DocumentType>(name);
            Type = documentType.Type;
        }

        private DocumentType(int id, string name, Type type)
            : base(id, name)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}
