@description('Name of the Function App')
param name string

@description('Location for the Function App')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('Resource ID of the App Service Plan')
param appServicePlanId string

@description('Connection string for the storage account')
@secure()
param storageAccountConnectionString string

@description('Storage account name for Flex Consumption')
param storageAccountName string = ''

@description('Application Insights Instrumentation Key')
@secure()
param appInsightsInstrumentationKey string = ''

@description('Application Insights Connection String')
@secure()
param appInsightsConnectionString string = ''

@description('Application Insights Resource ID (for tagging)')
param appInsightsResourceId string = ''

@description('.NET version for the function app')
@allowed([
  'v8.0'
  'v9.0'
])
param dotnetVersion string = 'v9.0'

@description('Operating system for the function app')
@allowed([
  'Windows'
  'Linux'
])
param operatingSystem string = 'Linux'

@description('CORS allowed origins')
param corsAllowedOrigins array = []

@description('CORS support credentials')
param corsSupportCredentials bool = true

@description('Container size in MB')
param containerSize int = 1536

@description('Function app scale limit')
param functionAppScaleLimit int = 100

@description('Additional app settings')
param appSettings object = {}

@description('Whether this is a Flex Consumption plan')
param isFlexConsumption bool = false

@description('Maximum instance count for Flex Consumption')
param maximumInstanceCount int = 100

@description('Instance memory in MB for Flex Consumption')
@allowed([
  2048
  4096
])
param instanceMemoryMB int = 2048

@description('Azure AD authentication secret')
@secure()
param aadAuthSecret string = ''

@description('Allowed Azure AD tenant IDs')
param aadAllowedTenants string = ''

@description('Key Vault name for secret references')
param keyVaultName string = ''

@description('SQL Server connection string secret name in Key Vault')
param sqlConnectionStringSecretName string = 'AzureSqlServer-ConnectionString'

var isLinux = operatingSystem == 'Linux'
var kind = isFlexConsumption ? 'functionapp,linux' : (isLinux ? 'functionapp,linux' : 'functionapp')

// App settings for non-Flex Consumption plans
var baseAppSettings = {
  AzureWebJobsStorage: storageAccountConnectionString
  WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
  WEBSITE_CONTENTSHARE: toLower(name)
  FUNCTIONS_EXTENSION_VERSION: '~4'
  FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
  WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: '1'
}

// App settings for Flex Consumption plans (minimal - storage is in functionAppConfig)
var flexAppSettings = {
  FUNCTIONS_EXTENSION_VERSION: '~4'
  FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
}

var insightsSettings = !empty(appInsightsConnectionString) ? {
  APPLICATIONINSIGHTS_CONNECTION_STRING: appInsightsConnectionString
} : !empty(appInsightsInstrumentationKey) ? {
  APPINSIGHTS_INSTRUMENTATIONKEY: appInsightsInstrumentationKey
} : {}

var authSettings = union(
  !empty(aadAuthSecret) ? { MICROSOFT_PROVIDER_AUTHENTICATION_SECRET: aadAuthSecret } : {},
  !empty(aadAllowedTenants) ? { WEBSITE_AUTH_AAD_ALLOWED_TENANTS: aadAllowedTenants } : {}
)

// Key Vault reference for SQL connection string
var sqlConnectionSettings = !empty(keyVaultName) ? {
  ConnectionStrings__SqlServer: '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}${environment().suffixes.keyvaultDns}/secrets/${sqlConnectionStringSecretName}/)'
} : {}

var mergedAppSettings = union(isFlexConsumption ? flexAppSettings : baseAppSettings, insightsSettings, authSettings, sqlConnectionSettings, appSettings)

// Convert merged app settings to array format for siteConfig
var appSettingsArray = [for setting in items(mergedAppSettings): {
  name: setting.key
  value: setting.value
}]

// Additional settings for Flex Consumption
var flexExtraSettings = [
  {
    name: 'DEPLOYMENT_STORAGE_CONNECTION_STRING'
    value: storageAccountConnectionString
  }
  {
    name: 'AzureWebJobsStorage__accountName'
    value: storageAccountName
  }
]

// Combined settings for Flex Consumption
var flexAppSettingsArray = concat(flexExtraSettings, appSettingsArray)

var appInsightsTags = !empty(appInsightsResourceId) ? {
  'hidden-link: /app-insights-resource-id': appInsightsResourceId
} : {}

var mergedTags = union(tags, appInsightsTags)

// Flex Consumption Function App
resource functionAppFlex 'Microsoft.Web/sites@2023-12-01' = if (isFlexConsumption) {
  name: name
  location: location
  tags: mergedTags
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    reserved: true
    clientAffinityEnabled: false
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storageAccountConnectionString};ContainerName=deployments'
          authentication: {
            type: 'StorageAccountConnectionString'
            storageAccountConnectionStringName: 'DEPLOYMENT_STORAGE_CONNECTION_STRING'
          }
        }
      }
      runtime: {
        name: 'dotnet-isolated'
        version: dotnetVersion
      }
      scaleAndConcurrency: {
        maximumInstanceCount: maximumInstanceCount
        instanceMemoryMB: instanceMemoryMB
      }
    }
    siteConfig: {
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      cors: {
        allowedOrigins: corsAllowedOrigins
        supportCredentials: corsSupportCredentials
      }
      appSettings: flexAppSettingsArray
    }
  }
}

// Standard Consumption Function App
resource functionApp 'Microsoft.Web/sites@2023-12-01' = if (!isFlexConsumption) {
  name: name
  location: location
  tags: mergedTags
  kind: kind
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    reserved: isLinux
    clientAffinityEnabled: false
    containerSize: containerSize
    siteConfig: {
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      http20Enabled: false
      use32BitWorkerProcess: false
      functionAppScaleLimit: functionAppScaleLimit
      cors: {
        allowedOrigins: corsAllowedOrigins
        supportCredentials: corsSupportCredentials
      }
      appSettings: appSettingsArray
    }
  }
}

@description('The resource ID of the Function App')
output id string = isFlexConsumption ? functionAppFlex.id : functionApp.id

@description('The name of the Function App')
output name string = isFlexConsumption ? functionAppFlex.name : functionApp.name

@description('The default hostname of the Function App')
output defaultHostName string = isFlexConsumption ? functionAppFlex.properties.defaultHostName : functionApp.properties.defaultHostName

@description('The principal ID of the Function App managed identity')
output principalId string = isFlexConsumption ? functionAppFlex.identity.principalId : functionApp.identity.principalId
