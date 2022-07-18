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
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  log_analytics_workspace_id                = data.azurerm_key_vault_secret.log_shared_id.value
  always_on                                 = true
  dotnet_framework_version                  = "6"
  use_dotnet_isolated_runtime               = true
  health_check_path                         = "/api/monitor/ready"

  app_settings                              = {
    METERINGPOINT_QUEUE_URL                                     = "${module.sb_meteringpoint.name}.servicebus.windows.net:9093"
    METERINGPOINT_SERVICE_BUS_LISTEN_CONNECTION_STRING          = module.sb_meteringpoint.primary_connection_strings["listen"]
    METERINGPOINT_DB_CONNECTION_STRING                          = local.MS_METERING_POINT_CONNECTION_STRING
    METERINGPOINT_QUEUE_NAME                                    = module.sbq_meteringpoint.name
    INTEGRATION_EVENT_QUEUE                                     = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-metering-point-forward-name)"
    INTEGRATION_EVENT_QUEUE_CONNECTION                          = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-listen-connection-string)"
    CHARGES_DEFAULT_LINK_RESPONSE_QUEUE                         = "create-link-reply"
    RAISE_TIME_HAS_PASSED_EVENT_SCHEDULE                        = "*/10 * * * * *"
    SERVICE_BUS_LISTEN_CONNECTION_STRING                        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-listen-connection-string)"
    SERVICE_BUS_SEND_CONNECTION_STRING                          = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"        
    MASTER_DATA_REQUEST_QUEUE_NAME                              = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-metering-point-master-data-request-name)"    
    SERVICE_BUS_CONNECTION_STRING_MANAGE_FOR_INTEGRATION_EVENTS = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"
    METERINGPOINT_QUEUE_MANAGE_CONNECTION_STRING          = module.sb_meteringpoint.primary_connection_strings["manage"]
  }

  tags                                      = azurerm_resource_group.this.tags
}