@description('Name of the App Configuration store')
param appConfigurationName string

@description('Key name for the configuration value')
param key string

@description('Value for the configuration key')
param value string

@description('Label for the configuration value (optional)')
param label string = ''

@description('Content type for the configuration value')
param contentType string = 'text/plain'

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2025-06-01-preview' existing = {
  name: appConfigurationName
}

resource configValue 'Microsoft.AppConfiguration/configurationStores/keyValues@2025-06-01-preview' = {
  parent: appConfiguration
  name: empty(label) ? key : '${key}$${label}'
  properties: {
    value: value
    contentType: contentType
  }
}

@description('The key of the configuration value')
output configKey string = key

@description('The value stored')
output configValue string = value
