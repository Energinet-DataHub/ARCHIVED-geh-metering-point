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
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using NodaTime.Text;

namespace Energinet.DataHub.MeteringPoints.Application.ProcessOverview
{
    public abstract class ProcessExtractor<TRequest>
    {
        private readonly IActorContext _actorContext;

        protected ProcessExtractor(IActorContext actorContext)
        {
            _actorContext = actorContext;
        }

        public virtual bool IsProcessOverviewEnabled => true;

        protected string CurrentActor => _actorContext.CurrentActor?.Identifier ??
                                         throw new InvalidOperationException("Current actor cannot be unknown");

        protected string DataHub => _actorContext.DataHub.Identifier;

        protected abstract string ProcessName { get; }

        public Process GetProcess(TRequest request, BusinessProcessResult result)
        {
            var gsrn = GetGsrn(request);
            var requestDetails = GetProcessDetails(request);
            var resultDetails = GetProcessDetails(result);
            return GetProcess(gsrn, requestDetails, resultDetails);
        }

        protected static DateTime? GetDateTime(string? dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return null;
            }

            var parseResult = InstantPattern.General.Parse(dateTimeString);
            if (!parseResult.Success)
            {
                return null;
            }

            return parseResult.Value.ToDateTimeUtc();
        }

        protected abstract string GetGsrn(TRequest request);

        protected abstract ProcessDetail GetProcessDetails(TRequest request);

        protected abstract ProcessDetail GetProcessDetails(BusinessProcessResult result);

        private Process GetProcess(string gsrn, params ProcessDetail[] details)
        {
            return new Process(
                id: Guid.NewGuid(),
                meteringPointGsrn: gsrn,
                name: ProcessName,
                createdDate: details.First().CreatedDate,
                effectiveDate: details.FirstOrDefault(d => d.EffectiveDate.HasValue)?.EffectiveDate,
                status: ProcessStatus.Completed,
                details: details);
        }
    }
}
