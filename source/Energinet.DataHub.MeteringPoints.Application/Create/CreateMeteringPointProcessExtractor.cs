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
using System.Linq;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.ProcessOverview;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Create
{
    public class CreateMeteringPointProcessExtractor : ProcessExtractor<CreateMeteringPoint>
    {
        private readonly ISystemDateTimeProvider _dateTimeProvider;

        public CreateMeteringPointProcessExtractor(IActorContext actorContext, ISystemDateTimeProvider dateTimeProvider)
            : base(actorContext, dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        protected override string ProcessName => "BRS-004";

        public override ProcessDetail GetProcessDetails(CreateMeteringPoint request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new ProcessDetail(
                "RequestCreateMeteringPoint",
                CurrentActor,
                DataHub,
                _dateTimeProvider.Now().ToDateTimeUtc(),
                GetDateTime(request.EffectiveDate),
                ProcessStatus.Received);
        }

        public override ProcessDetail GetProcessDetails(BusinessProcessResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var name = result.Success
                ? "ConfirmCreateMeteringPoint"
                : "RejectCreateMeteringPoint";

            return new ProcessDetail(
                name,
                DataHub,
                CurrentActor,
                _dateTimeProvider.Now().ToDateTimeUtc(),
                null,
                ProcessStatus.Sent,
                result.ValidationErrors.Select(error => new ProcessDetailError(error.Code, error.Message)).ToArray());
        }

        protected override string GetGsrn(CreateMeteringPoint request) => request?.GsrnNumber
                                                                          ?? throw new InvalidOperationException("GSRN cannot be empty");
    }
}
