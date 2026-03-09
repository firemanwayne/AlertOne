@description('Name of the App Configuration store')
param name string

@description('Location for the App Configuration store')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('SKU for the App Configuration store')
@allowed([
  'free'
  'standard'
])
param sku string = 'free'

@description('Enable purge protection')
param enablePurgeProtection bool = false

@description('Soft delete retention in days')
@minValue(1)
@maxValue(7)
param softDeleteRetentionInDays int = 7

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    enablePurgeProtection: sku == 'standard' ? enablePurgeProtection : false
    softDeleteRetentionInDays: sku == 'standard' ? softDeleteRetentionInDays : 0
    disableLocalAuth: false
    publicNetworkAccess: 'Enabled'
  }
}

@description('The resource ID of the App Configuration store')
output id string = appConfiguration.id

@description('The name of the App Configuration store')
output name string = appConfiguration.name

@description('The endpoint of the App Configuration store')
output endpoint string = appConfiguration.properties.endpoint

@description('The primary connection string')
output connectionString string = appConfiguration.listKeys().value[0].connectionString

@description('The primary read-only connection string')
output readOnlyConnectionString string = appConfiguration.listKeys().value[2].connectionString
