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
  name                = "app-webapi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  app_service_plan_id = module.plan_func_shared.id

  site_config {
    dotnet_framework_version = "v5.0"
    cors {
      allowed_origins = ["*"]
    }
  }

  app_settings = {
    METERINGPOINT_QUEUE_TOPIC_NAME: module.sbq_meteringpoint.name,
    INTEGRATION_EVENT_QUEUE: data.azurerm_key_vault_secret.sb_domain_relay_listen_connection_string.value
  }

  connection_string {
    name  = "METERINGPOINT_DB_CONNECTION_STRING"
    type  = "SQLServer"
    value = local.METERING_POINT_CONNECTION_STRING
  }

  connection_string {
    name  = "INTEGRATION_EVENT_QUEUE_CONNECTION"
    type  = "Custom"
    value = data.azurerm_key_vault_secret.sb_domain_relay_listen_connection_string.value
  }

  connection_string {
    name  = "METERINGPOINT_QUEUE_CONNECTION_STRING"
    type  = "Custom"
    value = module.sb_meteringpoint.primary_connection_strings["listen"]
  }

  tags              = azurerm_resource_group.this.tags

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}