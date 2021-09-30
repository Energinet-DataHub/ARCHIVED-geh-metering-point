resource "azurerm_app_service_plan" "meteringpoint" {
  name                = "plan-${var.project}-${var.organisation}-${var.environment}"
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  kind                = "Linux"
  reserved            = true

  sku {
    tier = "Basic"
    size = "B1"
  }
}