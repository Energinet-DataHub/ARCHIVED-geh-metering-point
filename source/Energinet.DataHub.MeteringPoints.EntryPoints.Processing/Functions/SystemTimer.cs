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

using System;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common.SystemTime;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions
{
    public class SystemTimer
    {
        private readonly IMediator _mediator;

        public SystemTimer(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("RaiseTimeHasPassedEvent")]
        public Task RunAsync([TimerTrigger("%RAISE_TIME_HAS_PASSED_EVENT_SCHEDULE%")] TimerInfo timerTimerInfo, FunctionContext context)
        {
            var logger = context.GetLogger("System timer");
            logger.LogInformation($"System timer trigger at: {DateTime.Now}");
            logger.LogInformation($"Next timer schedule at: {timerTimerInfo?.ScheduleStatus?.Next}");

            return _mediator.Publish(new TimeHasPassed(Instant.FromDateTimeUtc(DateTime.UtcNow)));
        }
    }
}
