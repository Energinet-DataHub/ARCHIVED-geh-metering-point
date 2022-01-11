resource "azurerm_subnet" "this_vnet_internal_private_endpoints" {
  name                                          = "snet-internalendpoints-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name                           = azurerm_resource_group.this.name
  virtual_network_name                          = azurerm_virtual_network.this.name
  address_prefixes                              = ["10.0.1.0/24"]
  enforce_private_link_service_network_policies = true

  delegation {
    name = "delegation"
 
    service_delegation {
      name = "Microsoft.Web/serverFarms"
    }
  }

  tags                = azurerm_resource_group.this.tags

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}