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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
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
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
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

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        private FunctionAppHostConfigurationBuilder HostConfigurationBuilder { get; }

        public async Task InitializeAsync()
        {
            var localSettingsSnapshot = HostConfigurationBuilder.BuildLocalSettingsConfiguration();

            var ingestionHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
            var processingHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
            var outboxHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();

#if DEBUG
            var configuration = "Debug";
#else
            var configuration = "Release";
#endif
            var port = 8000;
            ingestionHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion\\bin\\{configuration}\\net5.0";
            ingestionHostSettings.Functions = "MeteringPoint";
            ingestionHostSettings.Port = ++port;

            processingHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\Energinet.DataHub.MeteringPoints.EntryPoints.Processing\\bin\\{configuration}\\net5.0";
            processingHostSettings.Functions = "QueueSubscriber";
            processingHostSettings.Port = ++port;

            ////outboxHostSettings.Functions = "OutboxWatcher";
            ////outboxHostSettings.Port = ++port;

            ingestionHostSettings.ProcessEnvironmentVariables.Add("INTERNAL_SERVICEBUS_RETRY_COUNT", "3");

            ingestionHostSettings.ProcessEnvironmentVariables.Add("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            processingHostSettings.ProcessEnvironmentVariables.Add("AzureWebJobsStorage", "UseDevelopmentStorage=true");

            ingestionHostSettings.ProcessEnvironmentVariables.Add("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            processingHostSettings.ProcessEnvironmentVariables.Add("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);

            ingestionHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_QUEUE_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            processingHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_QUEUE_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

            await ServiceBusResourceProvider
                .BuildQueue("sbq-meteringpoint")
                .Do(p =>
                {
                    ingestionHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_QUEUE_TOPIC_NAME", p.Name);
                    processingHostSettings.ProcessEnvironmentVariables.Add("METERINGPOINT_QUEUE_TOPIC_NAME", p.Name);
                })
                .CreateAsync().ConfigureAwait(false);

            IngestionHostManager = new FunctionAppHostManager(ingestionHostSettings, TestLogger);
            ProcessingHostManager = new FunctionAppHostManager(processingHostSettings, TestLogger);
            ////OutboxHostManager = new FunctionAppHostManager(outboxHostSettings, TestLogger);

            StartHost(IngestionHostManager);
            StartHost(ProcessingHostManager);
            ////StartHost(OutboxHostManager);
        }

        public async Task DisposeAsync()
        {
            IngestionHostManager.Dispose();
            ProcessingHostManager.Dispose();
            ////OutboxHostManager.Dispose();

            AzuriteManager.Dispose();

            // => Service Bus
            await ServiceBusResourceProvider.DisposeAsync().ConfigureAwait(false);
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
    }
}
