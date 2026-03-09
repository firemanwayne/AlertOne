@description('Name of the App Service Plan')
param name string

@description('Location for the App Service Plan')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('The SKU of the App Service Plan')
@allowed([
  'Y1'        // Consumption (Serverless)
  'FC1'       // Flex Consumption
  'EP1'       // Elastic Premium 1
  'EP2'       // Elastic Premium 2
  'EP3'       // Elastic Premium 3
])
param sku string = 'FC1'

@description('Operating system for the plan')
@allowed([
  'Windows'
  'Linux'
])
param operatingSystem string = 'Linux'

var isConsumption = sku == 'Y1'
var isFlexConsumption = sku == 'FC1'
var isLinux = operatingSystem == 'Linux'

var skuTier = isConsumption ? 'Dynamic' : (isFlexConsumption ? 'FlexConsumption' : 'ElasticPremium')

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: name
  location: location
  tags: tags
  kind: isLinux ? 'linux' : 'functionapp'
  sku: {
    name: sku
    tier: skuTier
  }
  properties: {
    reserved: isLinux
    maximumElasticWorkerCount: (isConsumption || isFlexConsumption) ? null : 20
  }
}

@description('The resource ID of the App Service Plan')
output id string = appServicePlan.id

@description('The name of the App Service Plan')
output name string = appServicePlan.name
