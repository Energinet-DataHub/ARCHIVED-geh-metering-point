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
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Fixtures
{
    public class IngestionFunctionAppFixture : FunctionAppFixture
    {
        public IngestionFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            DatabaseManager = new MeteringPointDatabaseManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            AuthorizationConfiguration = new AuthorizationConfiguration();
            ServiceBusResourceProvider = new ServiceBusResourceProvider(IntegrationTestConfiguration.ServiceBusConnectionString, TestLogger);
        }

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        private AzuriteManager AzuriteManager { get; }

        private MeteringPointDatabaseManager DatabaseManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            if (hostSettings == null)
                return;

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion\\bin\\{buildConfiguration}\\net6.0";
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            Environment.SetEnvironmentVariable("INTERNAL_SERVICEBUS_RETRY_COUNT", "3");
            Environment.SetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONNECTION_STRING", "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONTAINER_NAME", "marketoplogs");
            Environment.SetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING", DatabaseManager.ConnectionString);
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            // => Database
            await DatabaseManager.CreateDatabaseAsync().ConfigureAwait(false);

            // => Service Bus
            // Overwrite service bus related settings, so the function app uses the names we have control of in the test
            Environment.SetEnvironmentVariable("METERINGPOINT_QUEUE_SEND_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

            Environment.SetEnvironmentVariable("B2C_TENANT_ID", AuthorizationConfiguration.B2cTenantId);
            Environment.SetEnvironmentVariable("BACKEND_SERVICE_APP_ID", AuthorizationConfiguration.BackendAppId);

            var meteringPointQueue = await ServiceBusResourceProvider
                .BuildQueue("queue").SetEnvironmentVariableToQueueName("METERINGPOINT_QUEUE_NAME")
                .CreateAsync().ConfigureAwait(false);

            // Shared logging blob storage container
            var storage = new BlobContainerClient("UseDevelopmentStorage=true", "marketoplogs");
            await storage.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            if (Debugger.IsAttached)
                Debugger.Break();

            return base.OnFunctionAppHostFailedAsync(hostLogSnapshot, exception);
        }

        /// <inheritdoc/>
        protected override async Task OnDisposeFunctionAppDependenciesAsync()
        {
            AzuriteManager.Dispose();

            // => Service Bus
            await ServiceBusResourceProvider.DisposeAsync().ConfigureAwait(false);
        }

        private static string GetBuildConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }
}
