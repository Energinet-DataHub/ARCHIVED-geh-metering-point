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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.MessageHub.IntegrationTesting;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Fixtures
{
    /// <summary>
    /// PoC for integration test starting multiple function apps.
    /// </summary>
    public class MeteringPointFunctionAppFixture : IAsyncLifetime
    {
        public MeteringPointFunctionAppFixture()
        {
            TestLogger = new TestDiagnosticsLogger();

            AzuriteManager = new AzuriteManager();
            DatabaseManager = new MeteringPointDatabaseManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            AuthorizationConfiguration = new AuthorizationConfiguration();
            ServiceBusResourceProvider = new ServiceBusResourceProvider(IntegrationTestConfiguration.ServiceBusConnectionString, TestLogger);

            HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();
        }

        public ITestDiagnosticsLogger TestLogger { get; }

        [NotNull]
        public FunctionAppHostManager? IngestionHostManager { get; private set; }

        [NotNull]
        public FunctionAppHostManager? ProcessingHostManager { get; private set; }

        [NotNull]
        public FunctionAppHostManager? OutboxHostManager { get; private set; }

        [NotNull]
        public FunctionAppHostManager? LocalMessageHubHostManager { get; private set; }

        [NotNull]
        public MessageHubSimulation? MessageHubSimulator { get; private set; }

        public MeteringPointDatabaseManager DatabaseManager { get; }

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        private FunctionAppHostConfigurationBuilder HostConfigurationBuilder { get; }

        public async Task InitializeAsync()
        {
            // => Storage emulator
            AzuriteManager.StartAzurite();

            // => Prepare host settings
            var localSettingsSnapshot = HostConfigurationBuilder.BuildLocalSettingsConfiguration();

            var ingestionHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
            var processingHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
            var outboxHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
            var localMessageHubHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();

            var buildConfiguration = GetBuildConfiguration();
            var port = 8000;
            ingestionHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion\\bin\\{buildConfiguration}\\net6.0";
            ingestionHostSettings.Functions = "MeteringPoint";
            ingestionHostSettings.Port = ++port;

            processingHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\Energinet.DataHub.MeteringPoints.EntryPoints.Processing\\bin\\{buildConfiguration}\\net6.0";
            processingHostSettings.Functions = "QueueSubscriber RaiseTimeHasPassedEvent";
            processingHostSettings.Port = ++port;

            outboxHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\Energinet.DataHub.MeteringPoints.EntryPoints.Outbox\\bin\\{buildConfiguration}\\net6.0";
            outboxHostSettings.Functions = "OutboxWatcher";
            outboxHostSettings.Port = ++port;

            localMessageHubHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\Energinet.DataHub.MeteringPoints.EntryPoints.LocalMessageHub\\bin\\{buildConfiguration}\\net6.0";
            localMessageHubHostSettings.Functions = "BundleDequeuedQueueSubscriber RequestBundleQueueSubscriber";
            localMessageHubHostSettings.Port = ++port;

            ingestionHostSettings.ProcessEnvironmentVariables.Add("INTERNAL_SERVICEBUS_RETRY_COUNT", "3");
            ingestionHostSettings.ProcessEnvironmentVariables.Add("B2C_TENANT_ID", AuthorizationConfiguration.B2cTenantId);
            ingestionHostSettings.ProcessEnvironmentVariables.Add("BACKEND_SERVICE_APP_ID", AuthorizationConfiguration.BackendAppId);

            // Use "0 0 8 1 1 *", to do: 8:00 1. January ~ we must set this setting, but we do not want the trigger to run automatically, we want to trigger it manually
            outboxHostSettings.ProcessEnvironmentVariables.Add("ACTOR_MESSAGE_DISPATCH_TRIGGER_TIMER", "*/1 * * * * *");
            processingHostSettings.ProcessEnvironmentVariables.Add("RAISE_TIME_HAS_PASSED_EVENT_SCHEDULE", "*/1 * * * * *");

            ingestionHostSettings.ProcessEnvironmentVariables.Add("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            processingHostSettings.ProcessEnvironmentVariables.Add("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            outboxHostSettings.ProcessEnvironmentVariables.Add("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("AzureWebJobsStorage", "UseDevelopmentStorage=true");

            ingestionHostSettings.ProcessEnvironmentVariables.Add("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            processingHostSettings.ProcessEnvironmentVariables.Add("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            outboxHostSettings.ProcessEnvironmentVariables.Add("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);

            ingestionHostSettings.ProcessEnvironmentVariables.Add("REQUEST_RESPONSE_LOGGING_CONNECTION_STRING", "UseDevelopmentStorage=true");
            ingestionHostSettings.ProcessEnvironmentVariables.Add("REQUEST_RESPONSE_LOGGING_CONTAINER_NAME", "marketoplogs");

            // => Logging
            var storage = new BlobContainerClient("UseDevelopmentStorage=true", "marketoplogs");
            await storage.CreateIfNotExistsAsync().ConfigureAwait(false);

            // => MeteringPoint
            ingestionHostSettings.ProcessEnvironmentVariables.Add("SHARED_SERVICE_BUS_SENDER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            processingHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_QUEUE_LISTEN_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            processingHostSettings.ProcessEnvironmentVariables.Add("SHARED_SERVICE_BUS_SEND_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("SHARED_SERVICE_BUS_SENDER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

            var meteringPointQueue = await ServiceBusResourceProvider
                .BuildQueue("sbq-meteringpoint")
                .Do(p =>
                {
                    ingestionHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_QUEUE_NAME", p.Name);
                    processingHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_QUEUE_NAME", p.Name);
                    localMessageHubHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_QUEUE_NAME", p.Name);
                })
                .CreateAsync().ConfigureAwait(false);

            // => MessageHub
            outboxHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_QUEUE_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_QUEUE_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

            var dataAvailableQueue = await ServiceBusResourceProvider
                .BuildQueue("dataavailable").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_DATA_AVAILABLE_QUEUE", p.Name))
                .CreateAsync().ConfigureAwait(false);
            var requestBundleQueue = await ServiceBusResourceProvider
                .BuildQueue("meteringpoints", 1, null, true).Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("REQUEST_BUNDLE_QUEUE_SUBSCRIBER_QUEUE", p.Name))
                .CreateAsync().ConfigureAwait(false);
            var replyQueue = await ServiceBusResourceProvider
                .BuildQueue("meteringpoints-reply", 1, null, true).Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_DOMAIN_REPLY_QUEUE", p.Name))
                .CreateAsync().ConfigureAwait(false);
            var dequeuedQueue = await ServiceBusResourceProvider
                .BuildQueue("meteringpoints-dequeue").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("BUNDLE_DEQUEUED_SUBSCRIBER_QUEUE", p.Name))
                .CreateAsync().ConfigureAwait(false);

            outboxHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_STORAGE_CONNECTION_STRING", "UseDevelopmentStorage=true");
            outboxHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_STORAGE_CONTAINER_NAME", "meteringpoint");
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_STORAGE_CONNECTION_STRING", "UseDevelopmentStorage=true");
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_STORAGE_CONTAINER_NAME", "meteringpoint");
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("REQUEST_BUNDLE_QUEUE_SUBSCRIBER_QUEUE", requestBundleQueue.Name);
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("BUNDLE_DEQUEUED_SUBSCRIBER_QUEUE", dequeuedQueue.Name);
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_DATA_AVAILABLE_QUEUE", dataAvailableQueue.Name);
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("MESSAGEHUB_DOMAIN_REPLY_QUEUE", replyQueue.Name);

            var messageHubSimulationConfig = new MessageHubSimulationConfig(
                serviceBusReadWriteConnectionString: ServiceBusResourceProvider.ConnectionString,
                dataAvailableQueueName: dataAvailableQueue.Name,
                domainQueueName: requestBundleQueue.Name,
                domainReplyQueueName: replyQueue.Name,
                domainDequeueQueueName: dequeuedQueue.Name,
                blobStorageConnectionString: "UseDevelopmentStorage=true",
                blobStorageContainerName: "meteringpoint");
            MessageHubSimulator = new MessageHubSimulation(messageHubSimulationConfig);

            // => Integration events
            outboxHostSettings.ProcessEnvironmentVariables.Add("SHARED_SERVICE_BUS_SENDER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

            await ServiceBusResourceProvider
                .BuildQueue("sbq-charges-link").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("CHARGES_DEFAULT_LINK_RESPONSE_QUEUE", p.Name))
                .CreateAsync().ConfigureAwait(false);
            await ServiceBusResourceProvider
                .BuildQueue("sbq-charges-messages").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("CHARGES_DEFAULT_MESSAGES_RESPONSE_QUEUE", p.Name))
                .CreateAsync().ConfigureAwait(false);
            await ServiceBusResourceProvider
                .BuildTopic("sbt-meteringpoint-dequeued").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("METERING_POINT_MESSAGE_DEQUEUED_TOPIC", p.Name))
                .CreateAsync().ConfigureAwait(false);
            await ServiceBusResourceProvider
                .BuildTopic("sbt-meteringpoint-created").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("METERING_POINT_CREATED_TOPIC", p.Name))
                .CreateAsync().ConfigureAwait(false);
            await ServiceBusResourceProvider
                .BuildTopic("sbt-meteringpoint-connected").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("METERING_POINT_CONNECTED_TOPIC", p.Name))
                .CreateAsync().ConfigureAwait(false);
            await ServiceBusResourceProvider
                .BuildTopic("sbt-meteringpoint-disconnected").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("METERING_POINT_DISCONNECTED_TOPIC", p.Name))
                .CreateAsync().ConfigureAwait(false);
            await ServiceBusResourceProvider
                .BuildTopic("sbt-meteringpoint-reconnected").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("METERING_POINT_RECONNECTED_TOPIC", p.Name))
                .CreateAsync().ConfigureAwait(false);
            await ServiceBusResourceProvider
                .BuildTopic("sbt-master-data-updated").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("MASTER_DATA_UPDATED_TOPIC", p.Name))
                .CreateAsync().ConfigureAwait(false);
            await ServiceBusResourceProvider
                .BuildTopic("sbt-sharedres-integrationevent-received").Do(p => outboxHostSettings.ProcessEnvironmentVariables.Add("INTEGRATION_EVENT_TOPIC_NAME", p.Name))
                .CreateAsync().ConfigureAwait(false);

            // => Database
            await DatabaseManager.CreateDatabaseAsync().ConfigureAwait(false);

            ingestionHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_DB_CONNECTION_STRING", DatabaseManager.ConnectionString);
            processingHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_DB_CONNECTION_STRING", DatabaseManager.ConnectionString);
            outboxHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_DB_CONNECTION_STRING", DatabaseManager.ConnectionString);
            localMessageHubHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_DB_CONNECTION_STRING", DatabaseManager.ConnectionString);

            IngestionHostManager = new FunctionAppHostManager(ingestionHostSettings, TestLogger);
            ProcessingHostManager = new FunctionAppHostManager(processingHostSettings, TestLogger);
            OutboxHostManager = new FunctionAppHostManager(outboxHostSettings, TestLogger);
            LocalMessageHubHostManager = new FunctionAppHostManager(localMessageHubHostSettings, TestLogger);

            StartHost(IngestionHostManager);
            StartHost(ProcessingHostManager);
            StartHost(OutboxHostManager);
            StartHost(LocalMessageHubHostManager);
        }

        public async Task DisposeAsync()
        {
            IngestionHostManager.Dispose();
            ProcessingHostManager.Dispose();
            OutboxHostManager.Dispose();
            LocalMessageHubHostManager.Dispose();

            AzuriteManager.Dispose();

            // => Service Bus
            await ServiceBusResourceProvider.DisposeAsync().ConfigureAwait(false);

            // => Database
            // await DatabaseManager.DeleteDatabaseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Use this method to attach <paramref name="testOutputHelper"/> to the host logging pipeline.
        /// While attached, any entries written to host log pipeline will also be logged to xUnit test output.
        /// It is important that it is only attached while a test i active. Hence, it should be attached in
        /// the test class constructor; and detached in the test class Dispose method (using 'null').
        /// </summary>
        /// <param name="testOutputHelper">If a xUnit test is active, this should be the instance of xUnit's <see cref="ITestOutputHelper"/>; otherwise it should be 'null'.</param>
        public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
        {
            TestLogger.TestOutputHelper = testOutputHelper;
        }

        private static void StartHost(FunctionAppHostManager hostManager)
        {
            IEnumerable<string> hostStartupLog;

            try
            {
                hostManager.StartHost();
            }
            catch (Exception)
            {
                // Function App Host failed during startup.
                // Exception has already been logged by host manager.
                hostStartupLog = hostManager.GetHostLogSnapshot();

                if (Debugger.IsAttached)
                    Debugger.Break();

                // Rethrow
                throw;
            }

            // Function App Host started.
            hostStartupLog = hostManager.GetHostLogSnapshot();
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
