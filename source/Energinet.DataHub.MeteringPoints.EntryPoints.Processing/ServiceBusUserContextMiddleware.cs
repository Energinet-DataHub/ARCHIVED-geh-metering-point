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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.MeteringPoints.Infrastructure.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public sealed class ServiceBusUserContextMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger _logger;
        private readonly IUserContext _userContext;

        public ServiceBusUserContextMiddleware(
            ILogger logger,
            IUserContext userContext)
        {
            _logger = logger;
            _userContext = userContext;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.BindingContext.BindingData.TryGetValue("UserProperties", out var userPropertiesObject) && userPropertiesObject != null)
            {
                _userContext.CurrentUser = ServiceBusUserIdentityFactory.FromDictionaryString(userPropertiesObject as string ?? string.Empty, Constants.ServiceBusIdentityKey);
            }
            else
            {
                _logger.LogWarning("UserIdentity not found for invocation: {invocationId}", context.InvocationId);
                throw new InvalidOperationException();
            }

            await next(context).ConfigureAwait(false);
        }
    }
}