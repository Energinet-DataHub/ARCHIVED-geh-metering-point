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
module "func_processing" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=7.0.0"

  name                                      = "processing"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = module.vnet_integrations_functions.id
  private_endpoint_subnet_id                = module.snet_internal_private_endpoints.id
  external_private_endpoint_subnet_id       = module.snet_external_private_endpoints.id
  private_dns_resource_group_name           = data.azurerm_key_vault_secret.pdns_resource_group_name.value
  app_service_plan_id                       = module.plan_shared.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  always_on                                 = true
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE       = true
    WEBSITE_RUN_FROM_PACKAGE              = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE   = true
    FUNCTIONS_WORKER_RUNTIME              = "dotnet-isolated"
    # Endregion: Default Values
    METERINGPOINT_QUEUE_URL                 = "${module.sb_meteringpoint.name}.servicebus.windows.net:9093"
    METERINGPOINT_QUEUE_CONNECTION_STRING   = module.sb_meteringpoint.primary_connection_strings["listen"]
    METERINGPOINT_DB_CONNECTION_STRING      = local.MS_METERING_POINT_CONNECTION_STRING
    METERINGPOINT_QUEUE_TOPIC_NAME          = module.sbq_meteringpoint.name
    INTEGRATION_EVENT_QUEUE                 = data.azurerm_key_vault_secret.metering_point_forwarded_name.value
    INTEGRATION_EVENT_QUEUE_CONNECTION      = data.azurerm_key_vault_secret.sb_domain_relay_listen_connection_string.value
    CHARGES_DEFAULT_LINK_RESPONSE_QUEUE     = "create-link-reply"
  }

  tags                                      = azurerm_resource_group.this.tags
}