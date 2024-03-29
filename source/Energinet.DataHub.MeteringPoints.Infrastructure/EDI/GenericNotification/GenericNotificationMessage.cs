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

using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GenericNotification
{
    // TODO: This is a hack used during the test period and should be removed as soon as the business processes from Market roles are included
    public record GenericNotificationMessage(
        string DocumentName,
        string Id,
        string Type,
        string ProcessType,
        string BusinessSectorType,
        MarketRoleParticipant Sender,
        MarketRoleParticipant Receiver,
        Instant CreatedDateTime,
        MarketActivityRecord MarketActivityRecord);
}
