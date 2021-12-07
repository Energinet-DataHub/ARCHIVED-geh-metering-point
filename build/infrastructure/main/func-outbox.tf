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
module "func_outbox" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=5.1.0"

  name                                      = "outbox"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = module.plan_shared.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  always_on                                 = true
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                               = true
    WEBSITE_RUN_FROM_PACKAGE                                      = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                           = true
    FUNCTIONS_WORKER_RUNTIME                                      = "dotnet-isolated"
    # Endregion: Default Values
    METERINGPOINT_DB_CONNECTION_STRING                            = local.METERING_POINT_CONNECTION_STRING
    METERINGPOINT_QUEUE_TOPIC_NAME                                = module.sbq_meteringpoint.name
    SHARED_INTEGRATION_EVENT_SERVICE_BUS_SENDER_CONNECTION_STRING = data.azurerm_key_vault_secret.sb_domain_relay_send_connection_string.value
    METERING_POINT_CREATED_TOPIC                                  = "metering-point-created"
    CONSUMPTION_METERING_POINT_CREATED_TOPIC                      = "consumption-metering-point-created"
    PRODUCTION_METERING_POINT_CREATED_TOPIC                       = "production-metering-point-created"
    EXCHANGE_METERING_POINT_CREATED_TOPIC                         = "exchange-metering-point-created"
    SPECIAL_METERING_POINT_CREATED_TOPIC                          = "special-metering-point-created"
    METERING_POINT_CONNECTED_TOPIC                                = "metering-point-connected"
    METERING_POINT_MESSAGE_DEQUEUED_TOPIC                         = "metering-point-message-dequeued"
    ACTOR_MESSAGE_DISPATCH_TRIGGER_TIMER                          = "*/10 * * * * *"
    EVENT_MESSAGE_DISPATCH_TRIGGER_TIMER                          = "*/10 * * * * *"
    MESSAGEHUB_STORAGE_CONNECTION_STRING                          = data.azurerm_key_vault_secret.st_market_operator_response_primary_connection_string.value
    MESSAGEHUB_QUEUE_CONNECTION_STRING                            = data.azurerm_key_vault_secret.sb_domain_relay_transceiver_connection_string.value
    MESSAGEHUB_STORAGE_CONTAINER_NAME                             = data.azurerm_key_vault_secret.st_market_operator_response_postofficereply_container_name.value
    CHARGES_DEFAULT_LINK_RESPONSE_QUEUE                           = data.azurerm_key_vault_secret.sbq_create_link_reply_name.value
    MESSAGEHUB_DATA_AVAILABLE_QUEUE                               = data.azurerm_key_vault_secret.sbq_data_available_name.value
    MESSAGEHUB_DOMAIN_REPLY_QUEUE                                 = data.azurerm_key_vault_secret.sbq_metering_points_reply_name.value
  }

  tags                                      = azurerm_resource_group.this.tags
}