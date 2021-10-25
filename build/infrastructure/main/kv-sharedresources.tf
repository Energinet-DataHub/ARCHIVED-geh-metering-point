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

data "azurerm_key_vault" "kv_sharedresources" {
  name                = var.sharedresources_keyvault_name
  resource_group_name = var.sharedresources_resource_group_name
}

data "azurerm_key_vault_secret" "SHARED_RESOURCES_DB_ADMIN_NAME" {
  name         = "SHARED-RESOURCES-DB-ADMIN-NAME"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "SHARED_RESOURCES_DB_ADMIN_PASSWORD" {
  name         = "SHARED-RESOURCES-DB-ADMIN-PASSWORD"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}
data "azurerm_key_vault_secret" "SHARED_RESOURCES_DB_URL" {
  name         = "SHARED-RESOURCES-DB-URL"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "SHARED_RESOURCES_EVENT_FORWARDED_QUEUE" {
  name         = "METERING-POINT-FORWARDED-QUEUE-NAME"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "shared_resources_integrationevents_transceiver_connection_string" {
  name         = "SHARED-RESOURCES--SB-INTEGRATIONEVENTS-TRANSCEIVER-CONNECTION-STRING"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "shared_resources_marketoperator_response_connection_string" {
  name         = "SHARED-RESOURCES-MARKETOPERATOR-RESPONSE-CONNECTION-STRING"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "shared_resources_marketoperator_container_reply_name" {
  name         = "SHARED-RESOURCES-MARKETOPERATOR-CONTAINER-REPLY-NAME"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}