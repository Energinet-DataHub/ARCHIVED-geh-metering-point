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
module "sbs_metering_point_actor_created" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "actor-created"
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_name_id.value
  project_name        = var.domain_name_short
  max_delivery_count  = 10 
  correlation_filter  = {
    properties     = {
      "MessageType" = "ActorCreated",
    }  
  }
}

module "sbs_metering_point_actor_role_added" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "actor-role-added"
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_name_id.value
  project_name        = var.domain_name_short
  max_delivery_count  = 10 
  correlation_filter  = {
    properties     = {
      "MessageType" = "ActorRoleAdded",
    }  
  }
}

module "sbs_metering_point_actor_role_removed" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "actor-role-removed"
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_name_id.value
  project_name        = var.domain_name_short
  max_delivery_count  = 10 
  correlation_filter  = {
    properties     = {
      "MessageType" = "ActorRoleRemoved",
    }  
  }
}

module "sbs_metering_point_actor_grid_area_added" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "actor-grid-area-added"
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_name_id.value
  project_name        = var.domain_name_short
  max_delivery_count  = 10 
  correlation_filter  = {
    properties     = {
      "MessageType" = "ActorGridAreaAdded",
    }  
  }
}

module "sbs_metering_point_actor_grid_area_removed" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "actor-grid-area-removed"
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_name_id.value
  project_name        = var.domain_name_short
  max_delivery_count  = 10 
  correlation_filter  = {
    properties     = {
      "MessageType" = "ActorGridAreaRemoved",
    }  
  }
}

module "sbs_metering_point_grid_area_created" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "grid-area-created"
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_name_id.value
  project_name        = var.domain_name_short
  max_delivery_count  = 10 
  correlation_filter  = {
    properties     = {
      "MessageType" = "GridAreaCreated",
    }  
  }
}

module "sbs_metering_point_grid_area_name_changed" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v9"
  name                = "grid-area-name-changed"
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_name_id.value
  project_name        = var.domain_name_short
  max_delivery_count  = 10 
  correlation_filter  = {
    properties     = {
      "MessageType" = "GridAreaNameChanged",      
    }  
  }
}