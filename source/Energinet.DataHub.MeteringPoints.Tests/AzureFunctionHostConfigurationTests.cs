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
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests
{
    public class AzureFunctionHostConfigurationTests
    {
        private const string ServiceBusConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=sender;SharedAccessKey=0XLJDfVlg+CorvdniMfp5S+SKbAeB9Kkiee6ZVBJJ4c=";
        private const string SomeString = "Foo";

        [Fact]
        public void IngestionHostConfigurationTest()
        {
            Environment.SetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING", SomeString);
            Environment.SetEnvironmentVariable("METERINGPOINT_QUEUE_SEND_CONNECTION_STRING", ServiceBusConnectionString);
            Environment.SetEnvironmentVariable("METERINGPOINT_QUEUE_NAME", SomeString);
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", SomeString);
            Environment.SetEnvironmentVariable("B2C_TENANT_ID", SomeString);
            Environment.SetEnvironmentVariable("BACKEND_SERVICE_APP_ID", SomeString);
            Environment.SetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONTAINER_NAME", SomeString);
            Environment.SetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONNECTION_STRING", SomeString);
            var program = new EntryPoints.Ingestion.Program();

            program.ConfigureApplication();

            program.AssertConfiguration();
        }

        [Fact]
        public void ProcessingHostConfigurationTest()
        {
            Environment.SetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING", SomeString);
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", SomeString);
            Environment.SetEnvironmentVariable("SERVICE_BUS_SEND_CONNECTION_STRING", ServiceBusConnectionString);
            var program = new EntryPoints.Processing.Program();

            program.ConfigureApplication();

            program.AssertConfiguration();
        }

        [Fact]
        public void OutboxHostConfigurationTest()
        {
            Environment.SetEnvironmentVariable("SHARED_SERVICE_BUS_SENDER_CONNECTION_STRING", ServiceBusConnectionString);
            Environment.SetEnvironmentVariable("CHARGES_DEFAULT_LINK_RESPONSE_QUEUE", SomeString);
            Environment.SetEnvironmentVariable("CHARGES_DEFAULT_MESSAGES_RESPONSE_QUEUE", SomeString);
            Environment.SetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING", SomeString);
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", SomeString);
            Environment.SetEnvironmentVariable("METERING_POINT_CREATED_TOPIC", SomeString);
            Environment.SetEnvironmentVariable("CONSUMPTION_METERING_POINT_CREATED_TOPIC", SomeString);
            Environment.SetEnvironmentVariable("MASTER_DATA_UPDATED_TOPIC", SomeString);
            Environment.SetEnvironmentVariable("EXCHANGE_METERING_POINT_CREATED_TOPIC", SomeString);
            Environment.SetEnvironmentVariable("PRODUCTION_METERING_POINT_CREATED_TOPIC", SomeString);
            Environment.SetEnvironmentVariable("METERING_POINT_MESSAGE_DEQUEUED_TOPIC", SomeString);
            Environment.SetEnvironmentVariable("METERING_POINT_CONNECTED_TOPIC", SomeString);
            Environment.SetEnvironmentVariable("METERING_POINT_DISCONNECTED_TOPIC", SomeString);
            Environment.SetEnvironmentVariable("METERING_POINT_RECONNECTED_TOPIC", SomeString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_QUEUE_CONNECTION_STRING", ServiceBusConnectionString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_STORAGE_CONNECTION_STRING", SomeString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_STORAGE_CONTAINER_NAME", SomeString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_DOMAIN_REPLY_QUEUE", SomeString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_DATA_AVAILABLE_QUEUE", SomeString);
            var program = new EntryPoints.Outbox.Program();

            program.ConfigureApplication();

            program.AssertConfiguration();
        }

        [Fact]
        public void LocalMessageHubHostConfigurationTest()
        {
            Environment.SetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING", SomeString);
            Environment.SetEnvironmentVariable("METERINGPOINT_QUEUE_SEND_CONNECTION_STRING", ServiceBusConnectionString);
            Environment.SetEnvironmentVariable("METERINGPOINT_QUEUE_NAME", SomeString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_QUEUE_CONNECTION_STRING", ServiceBusConnectionString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_STORAGE_CONNECTION_STRING", SomeString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_STORAGE_CONTAINER_NAME", SomeString);
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", SomeString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_DOMAIN_REPLY_QUEUE", SomeString);
            Environment.SetEnvironmentVariable("MESSAGEHUB_DATA_AVAILABLE_QUEUE", SomeString);
            var program = new EntryPoints.LocalMessageHub.Program();

            program.ConfigureApplication();

            program.AssertConfiguration();
        }
    }
}
