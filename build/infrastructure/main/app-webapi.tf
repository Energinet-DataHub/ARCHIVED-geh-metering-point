# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
resource "azurerm_app_service" "webapi" {
  name                = "app-webapi-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  app_service_plan_id = azurerm_app_service_plan.meteringpoint.id

  site_config {
    dotnet_framework_version = "v5.0"
    cors {
      allowed_origins = ["*"]
    }
  }

  app_settings = {
      METERINGPOINT_QUEUE_TOPIC_NAME: module.sbq_meteringpoint.name,
      INTEGRATION_EVENT_QUEUE: data.azurerm_key_vault_secret.INTEGRATION_EVENTS_LISTENER_CONNECTION_STRING.value
      }

  connection_string {
    name  = "METERINGPOINT_DB_CONNECTION_STRING"
    type  = "SQLServer"
    value = local.METERING_POINT_CONNECTION_STRING
  }

  connection_string {
    name  = "INTEGRATION_EVENT_QUEUE_CONNECTION"
    type  = "Custom"
    value = data.azurerm_key_vault_secret.INTEGRATION_EVENTS_LISTENER_CONNECTION_STRING.value
  }

  connection_string {
    name  = "METERINGPOINT_QUEUE_CONNECTION_STRING"
    type  = "Custom"
    value = module.sbnar_meteringpoint_listener.primary_connection_string
  }

}