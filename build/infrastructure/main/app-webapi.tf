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
module "app_webapi" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=7.0.0"

  name                                      = "webapi"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                = azurerm_key_vault_secret.snet_private_endpoints_id.value
  private_dns_resource_group_name           = data.azurerm_key_vault_secret.pdns_resource_group_name.value
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value

  app_settings = {
    APPINSIGHTS_INSTRUMENTATIONKEY = "${data.azurerm_key_vault_secret.appi_instrumentation_key.value}"
    B2C_TENANT_ID = "${data.azurerm_key_vault_secret.b2c_tenant_id.value}"
    FRONTEND_OPEN_ID_URL = "${data.azurerm_key_vault_secret.frontend_open_id_url.value}"
    FRONTEND_SERVICE_APP_ID = "${data.azurerm_key_vault_secret.frontend_service_app_id.value}"
  }

  connection_strings = [
    {
      name  = "METERINGPOINT_DB_CONNECTION_STRING"
      type  = "SQLServer"
      value = local.MS_METERING_POINT_CONNECTION_STRING
    }
  ]

  tags                                      = azurerm_resource_group.this.tags
}

module "kvs_app_metering_point_webapi_base_url" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=6.0.0"

  name          = "app-metering-point-webapi-base-url"
  value         = "https://${module.app_webapi.default_site_hostname}"
  key_vault_id  = data.azurerm_key_vault.kv_shared_resources.id

  tags          = azurerm_resource_group.this.tags
}
