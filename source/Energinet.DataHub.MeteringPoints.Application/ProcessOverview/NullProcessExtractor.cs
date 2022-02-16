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
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ProcessOverview
{
    public class NullProcessExtractor<TRequest> : ProcessExtractor<TRequest>
    {
        public NullProcessExtractor(IActorContext actorContext, ISystemDateTimeProvider dateTimeProvider)
            : base(actorContext, dateTimeProvider)
        {
        }

        public override bool IsProcessOverviewEnabled => false;

        protected override string ProcessName => "Null";

        public override ProcessDetail GetProcessDetails(TRequest request)
        {
            throw new NotImplementedException();
        }

        public override ProcessDetail GetProcessDetails(BusinessProcessResult result)
        {
            throw new NotImplementedException();
        }

        protected override string GetGsrn(TRequest request) => "GSRN";
    }
}
