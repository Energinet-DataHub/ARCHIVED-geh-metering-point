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

data "azurerm_sql_server" "sqlsrv" {
  name                = "sqlsrv-sharedres-${var.organisation}-${var.environment}"
  resource_group_name = var.sharedresources_resource_group_name
}

module "sqldb_meteringpoint" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//sql-database?ref=1.2.0"
  name                      = "sqldb-meteringpoint"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  server_name               = var.sharedresources_sql_server_name
}
