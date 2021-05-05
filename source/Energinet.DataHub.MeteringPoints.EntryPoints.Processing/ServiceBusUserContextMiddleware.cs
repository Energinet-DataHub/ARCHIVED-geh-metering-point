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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.UserIdentity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public sealed class ServiceBusUserContextMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger _logger;
        private readonly IUserContext _userContext;
        private readonly UserIdentityFactory _userIdentityFactory;

        public ServiceBusUserContextMiddleware(
            ILogger logger,
            IUserContext userContext,
            UserIdentityFactory userIdentityFactory)
        {
            _logger = logger;
            _userContext = userContext;
            _userIdentityFactory = userIdentityFactory;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: Fetch actual user identity from context
            if (context.BindingContext.BindingData.TryGetValue("ThisKeyDoesntExist", out var userPropertiesObject))
            {
                // TODO: Set via factory
                // _userContext.CurrentUser = _userIdentityFactory.FromString(...);
            }
            else
            {
                _logger.LogWarning("UserIdentity not found for invocation: {invocationId}", context.InvocationId);

                // TODO: Consider throwing if UserIdentity is not found
                // throw new InvalidOperationException();
                var userIdentity = new UserIdentity
                {
                    Id = "Who?",
                };
                _userContext.CurrentUser = userIdentity;
            }

            await next(context).ConfigureAwait(false);
        }
    }
}
