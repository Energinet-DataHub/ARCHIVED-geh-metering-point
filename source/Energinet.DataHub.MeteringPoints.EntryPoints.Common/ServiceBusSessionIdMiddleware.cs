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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Newtonsoft.Json;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Common
{
    public class ServiceBusSessionIdMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ISessionContext _sessionContext;

        public ServiceBusSessionIdMiddleware(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.BindingContext.BindingData.TryGetValue("MessageSession", out var value) && value is string session)
            {
                var sessionData = JsonConvert.DeserializeObject<Dictionary<string, object>>(session);

                if (sessionData["SessionId"] is not string sessionId)
                {
                    throw new InvalidOperationException("Session id does not exist in session data");
                }

                _sessionContext.SetId(sessionId);
            }

            await next(context).ConfigureAwait(false);
        }
    }
}
