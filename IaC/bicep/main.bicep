targetScope = 'resourceGroup'

// ============================================================================
// Parameters
// ============================================================================

@description('Environment name')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'dev'

@description('Primary location for resources')
param location string = resourceGroup().location

@description('Instance number for resources')
param instance string = '001'

@description('GitHub repository URL for Static Web App')
param repositoryUrl string = ''

@description('GitHub branch for Static Web App')
param branch string = 'master'

// Azure AD Configuration
@description('Azure AD administrator login for SQL Server')
param sqlAzureAdAdminLogin string = ''

@description('Azure AD administrator object ID for SQL Server')
param sqlAzureAdAdminObjectId string = ''

@description('Object ID of the deploying principal (service principal) for Key Vault access')
param deployingPrincipalId string = ''

// Deployment toggles
@description('Deploy Static Web App')
param deployStaticWebApp bool = true

@description('Deploy Alert Management Function App')
param deployAlertManagementFunction bool = true

@description('Deploy User Management Function App')
param deployUserManagementFunction bool = true

@description('Deploy App Configuration')
param deployAppConfiguration bool = true

@description('Deploy SQL Server and Database')
param deploySqlServer bool = false

@description('Deploy Cosmos DB')
param deployCosmosDb bool = true

@description('Deploy Key Vault')
param deployKeyVault bool = true

@description('Deploy Communication Services')
param deployCommunicationServices bool = true

@description('Deploy Notification Hub')
param deployNotificationHub bool = true

@description('App Configuration SKU')
@allowed([
  'free'
  'standard'
])
param appConfigurationSku string = 'free'

@description('App Service Plan SKU for Function Apps')
@allowed([
  'Y1'        // Consumption (Serverless)
  'FC1'       // Flex Consumption
  'EP1'       // Elastic Premium 1
  'EP2'       // Elastic Premium 2
  'EP3'       // Elastic Premium 3
])
param appServicePlanSku string = 'FC1'

@description('SQL Database SKU name')
param sqlDatabaseSkuName string = 'GP_S_Gen5_1'

@description('SQL Database SKU tier')
@allowed([
  'Basic'
  'Standard'
  'GeneralPurpose'
])
param sqlDatabaseSkuTier string = 'GeneralPurpose'

@description('SQL Database SKU family')
param sqlDatabaseSkuFamily string = 'Gen5'

@description('SQL Database SKU capacity')
param sqlDatabaseSkuCapacity int = 1

@description('Static Web App SKU')
@allowed([
  'Free'
  'Standard'
])
param staticWebAppSku string = 'Standard'

@description('Key Vault SKU')
@allowed([
  'standard'
  'premium'
])
param keyVaultSku string = 'standard'

@description('Notification Hub SKU')
@allowed([
  'Free'
  'Basic'
  'Standard'
])
param notificationHubSku string = 'Free'

@description('Storage Account SKU')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
])
param storageAccountSku string = 'Standard_LRS'

// ============================================================================
// Variables
// ============================================================================

var tags = {
  Environment: environment
  Application: 'AlertOne'
  ManagedBy: 'Bicep'
}

// Determine if using Flex Consumption plan
var isFlexConsumption = appServicePlanSku == 'FC1'

// Resource naming - all globally unique names include environment and instance
var sqlServerName = 'alert-one-sql-${environment}-${instance}'
var sqlDatabaseName = 'alert-one-db-${environment}-${instance}'
var cosmosDbAccountName = 'alertone-cosmos-${environment}-${instance}'
var cosmosDbDatabaseName = 'alertone-db'
var appConfigurationName = 'alert-one-appconfig-${environment}-${instance}'
var keyVaultName = 'alertone-kv-${environment}-${instance}'
var communicationServicesName = 'alert-one-comm-${environment}-${instance}'
var emailServicesName = 'alert-one-email-${environment}-${instance}'
var notificationHubNamespaceName = 'alertone-nhub-${environment}-${instance}'
var notificationHubName = 'alertone-hub-${environment}-${instance}'
var staticWebAppName = 'alert-one-swa-${environment}-${instance}'

// Function app naming
var alertMgmtFunctionName = 'alertone-alrtmg-${environment}-${instance}'
var userMgmtFunctionName = 'alertone-usrm-${environment}-${instance}'
var alertMgmtAppServicePlanName = 'asp-alrtmg-${environment}-${instance}'
var userMgmtAppServicePlanName = 'asp-usrm-${environment}-${instance}'
var alertMgmtStorageAccountName = 'stalrtmg${environment}${take(uniqueString(resourceGroup().id), 4)}'
var userMgmtStorageAccountName = 'stusrm${environment}${take(uniqueString(resourceGroup().id), 4)}'
var alertMgmtAppInsightsName = alertMgmtFunctionName
var userMgmtAppInsightsName = userMgmtFunctionName

// Static Web App requires specific regions
var staticWebAppLocation = 'eastus2'

// CORS origins
#disable-next-line BCP318
var staticWebAppHostname = deployStaticWebApp ? 'https://${staticWebApp.outputs.defaultHostName}' : ''
var corsAllowedOrigins = [
  'https://localhost:7183'
  staticWebAppHostname
]

// App Configuration endpoint for function apps
#disable-next-line BCP318
var appConfigEndpoint = deployAppConfiguration ? appConfiguration.outputs.endpoint : ''

// ============================================================================
// SQL Server and Database
// ============================================================================

module sqlServer 'modules/sql-server.bicep' = if (deploySqlServer && !empty(sqlAzureAdAdminLogin) && !empty(sqlAzureAdAdminObjectId)) {
  name: 'sqlServer-${uniqueString(resourceGroup().id)}'
  params: {
    name: sqlServerName
    location: location
    tags: tags
    azureAdAdminLogin: sqlAzureAdAdminLogin
    azureAdAdminObjectId: sqlAzureAdAdminObjectId
    azureAdOnlyAuthentication: true
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

module sqlDatabase 'modules/sql-database.bicep' = if (deploySqlServer && !empty(sqlAzureAdAdminLogin) && !empty(sqlAzureAdAdminObjectId)) {
  name: 'sqlDatabase-${uniqueString(resourceGroup().id)}'
  params: {
    name: sqlDatabaseName
    sqlServerName: sqlServerName
    location: location
    tags: tags
    skuName: sqlDatabaseSkuName
    skuTier: sqlDatabaseSkuTier
    skuFamily: sqlDatabaseSkuFamily
    skuCapacity: sqlDatabaseSkuCapacity
    autoPauseDelay: 60
    minCapacity: '1'
    requestedBackupStorageRedundancy: 'Local'
  }
  dependsOn: [
    sqlServer
  ]
}

// ============================================================================
// Cosmos DB
// ============================================================================

module cosmosDb 'modules/cosmos-db.bicep' = if (deployCosmosDb) {
  name: 'cosmosDb-${uniqueString(resourceGroup().id)}'
  params: {
    name: cosmosDbAccountName
    location: location
    tags: tags
    capacityMode: 'Serverless'
    databaseName: cosmosDbDatabaseName
    defaultConsistencyLevel: 'Session'
  }
}

// ============================================================================
// App Configuration
// ============================================================================

module appConfiguration 'modules/app-configuration.bicep' = if (deployAppConfiguration) {
  name: 'appConfiguration-${uniqueString(resourceGroup().id)}'
  params: {
    name: appConfigurationName
    location: location
    tags: tags
    sku: appConfigurationSku
  }
}

// ============================================================================
// Key Vault
// ============================================================================

module keyVault 'modules/key-vault.bicep' = if (deployKeyVault) {
  name: 'keyVault-${uniqueString(resourceGroup().id)}'
  params: {
    name: keyVaultName
    location: location
    tags: tags
    sku: keyVaultSku
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
  }
}

// ============================================================================
// Communication and Email Services
// ============================================================================

module emailServices 'modules/email-services.bicep' = if (deployCommunicationServices) {
  name: 'emailServices-${uniqueString(resourceGroup().id)}'
  params: {
    name: emailServicesName
    tags: tags
    dataLocation: 'United States'
    createAzureManagedDomain: true
  }
}

module communicationServices 'modules/communication-services.bicep' = if (deployCommunicationServices) {
  name: 'communicationServices-${uniqueString(resourceGroup().id)}'
  params: {
    name: communicationServicesName
    tags: tags
    dataLocation: 'United States'
    enableSystemAssignedIdentity: true
    linkedDomains: deployCommunicationServices ? [
        #disable-next-line BCP318
      emailServices.outputs.azureManagedDomainId
    ] : []
  }
  dependsOn: [
#disable-next-line no-unnecessary-dependson
    emailServices
  ]
}

// ============================================================================
// Notification Hub
// ============================================================================

module notificationHub 'modules/notification-hub.bicep' = if (deployNotificationHub) {
  name: 'notificationHub-${uniqueString(resourceGroup().id)}'
  params: {
    namespaceName: notificationHubNamespaceName
    hubName: notificationHubName
    location: location
    tags: tags
    sku: notificationHubSku
  }
}

// ============================================================================
// Alert Management Function App
// ============================================================================

module alertMgmtStorageAccount 'modules/storage-account.bicep' = if (deployAlertManagementFunction) {
  name: 'alertMgmtStorageAccount-${uniqueString(resourceGroup().id)}'
  params: {
    name: alertMgmtStorageAccountName
    location: location
    tags: union(tags, { Service: 'AlertManagement' })
    sku: storageAccountSku
  }
}

module alertMgmtAppServicePlan 'modules/app-service-plan.bicep' = if (deployAlertManagementFunction) {
  name: 'alertMgmtAppServicePlan-${uniqueString(resourceGroup().id)}'
  params: {
    name: alertMgmtAppServicePlanName
    location: location
    tags: union(tags, { Service: 'AlertManagement' })
    sku: appServicePlanSku
    operatingSystem: 'Linux'
  }
}

module alertMgmtAppInsights 'modules/application-insights.bicep' = if (deployAlertManagementFunction) {
  name: 'alertMgmtAppInsights-${uniqueString(resourceGroup().id)}'
  params: {
    name: alertMgmtAppInsightsName
    location: location
    tags: union(tags, { Service: 'AlertManagement' })
    retentionInDays: environment == 'prod' ? 90 : 90
  }
}

module alertManagementFunction 'modules/function-app.bicep' = if (deployAlertManagementFunction) {
  name: 'alertManagementFunction-${uniqueString(resourceGroup().id)}'
  params: {
    name: alertMgmtFunctionName
    location: location
    tags: union(tags, { Service: 'AlertManagement' })
    #disable-next-line BCP318
    appServicePlanId: alertMgmtAppServicePlan.outputs.id
    #disable-next-line BCP318
    storageAccountConnectionString: alertMgmtStorageAccount.outputs.connectionString
    storageAccountName: alertMgmtStorageAccount.?outputs.name
    deploymentsContainerUrl: alertMgmtStorageAccount.?outputs.deploymentsContainerUrl
    appInsightsConnectionString: alertMgmtAppInsights.?outputs.connectionString
    appInsightsResourceId: alertMgmtAppInsights.?outputs.id
    dotnetVersion: 'v9.0'
    operatingSystem: 'Linux'
    corsAllowedOrigins: corsAllowedOrigins
    corsSupportCredentials: true
    isFlexConsumption: isFlexConsumption
    keyVaultName: deployKeyVault ? keyVault.?outputs.name : ''
    appSettings: {
      AppConfig__BaseUri: appConfigEndpoint
    }
  }
  dependsOn: [
#disable-next-line no-unnecessary-dependson
    alertMgmtStorageAccount
#disable-next-line no-unnecessary-dependson
    alertMgmtAppServicePlan
#disable-next-line no-unnecessary-dependson
    alertMgmtAppInsights
#disable-next-line no-unnecessary-dependson
    keyVault
#disable-next-line no-unnecessary-dependson
    appConfiguration
  ]
}

// ============================================================================
// User Management Function App
// ============================================================================

module userMgmtStorageAccount 'modules/storage-account.bicep' = if (deployUserManagementFunction) {
  name: 'userMgmtStorageAccount-${uniqueString(resourceGroup().id)}'
  params: {
    name: userMgmtStorageAccountName
    location: location
    tags: union(tags, { Service: 'UserManagement' })
    sku: storageAccountSku
  }
}

module userMgmtAppServicePlan 'modules/app-service-plan.bicep' = if (deployUserManagementFunction) {
  name: 'userMgmtAppServicePlan-${uniqueString(resourceGroup().id)}'
  params: {
    name: userMgmtAppServicePlanName
    location: location
    tags: union(tags, { Service: 'UserManagement' })
    sku: appServicePlanSku
    operatingSystem: 'Linux'
  }
}

module userMgmtAppInsights 'modules/application-insights.bicep' = if (deployUserManagementFunction) {
  name: 'userMgmtAppInsights-${uniqueString(resourceGroup().id)}'
  params: {
    name: userMgmtAppInsightsName
    location: location
    tags: union(tags, { Service: 'UserManagement' })
    retentionInDays: environment == 'prod' ? 90 : 90
  }
}

module userManagementFunction 'modules/function-app.bicep' = if (deployUserManagementFunction) {
  name: 'userManagementFunction-${uniqueString(resourceGroup().id)}'
  params: {
    name: userMgmtFunctionName
    location: location
    tags: union(tags, { Service: 'UserManagement' })
    #disable-next-line BCP318
    appServicePlanId: userMgmtAppServicePlan.outputs.id
    #disable-next-line BCP318
    storageAccountConnectionString: userMgmtStorageAccount.outputs.connectionString
    #disable-next-line BCP318
    storageAccountName: userMgmtStorageAccount.outputs.name
    #disable-next-line BCP318
    deploymentsContainerUrl: userMgmtStorageAccount.outputs.deploymentsContainerUrl
    #disable-next-line BCP318
    appInsightsConnectionString: userMgmtAppInsights.outputs.connectionString
    #disable-next-line BCP318
    appInsightsResourceId: userMgmtAppInsights.outputs.id
    dotnetVersion: 'v9.0'
    operatingSystem: 'Linux'
    corsAllowedOrigins: corsAllowedOrigins
    corsSupportCredentials: true
    isFlexConsumption: isFlexConsumption
    #disable-next-line BCP318
    keyVaultName: deployKeyVault ? keyVault.outputs.name : ''
    appSettings: {
      AppConfig__BaseUri: appConfigEndpoint
    }
  }
  dependsOn: [
#disable-next-line no-unnecessary-dependson
    userMgmtStorageAccount
#disable-next-line no-unnecessary-dependson
    userMgmtAppServicePlan
#disable-next-line no-unnecessary-dependson
    userMgmtAppInsights
#disable-next-line no-unnecessary-dependson
    keyVault
#disable-next-line no-unnecessary-dependson
    appConfiguration
  ]
}

// ============================================================================
// Storage Account Role Assignments for Function Apps (Flex Consumption)
// ============================================================================

// Storage Blob Data Owner role ID
var storageBlobDataOwnerRoleId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'

resource alertMgmtStorageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployAlertManagementFunction && isFlexConsumption) {
  name: guid(resourceGroup().id, alertMgmtStorageAccountName, alertMgmtFunctionName, storageBlobDataOwnerRoleId)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwnerRoleId)
    #disable-next-line BCP318
    principalId: alertManagementFunction.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

resource userMgmtStorageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployUserManagementFunction && isFlexConsumption) {
  name: guid(resourceGroup().id, userMgmtStorageAccountName, userMgmtFunctionName, storageBlobDataOwnerRoleId)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwnerRoleId)
    #disable-next-line BCP318
    principalId: userManagementFunction.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

// ============================================================================
// Key Vault Role Assignments for Function Apps
// ============================================================================

// Key Vault Secrets User role ID
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource alertMgmtKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployAlertManagementFunction && deployKeyVault) {
  name: guid(resourceGroup().id, keyVaultName, alertMgmtFunctionName, keyVaultSecretsUserRoleId)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    #disable-next-line BCP318
    principalId: alertManagementFunction.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

resource userMgmtKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployUserManagementFunction && deployKeyVault) {
  name: guid(resourceGroup().id, keyVaultName, userMgmtFunctionName, keyVaultSecretsUserRoleId)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
#disable-next-line BCP318
    principalId: userManagementFunction.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

// Key Vault Secrets Officer role ID - for deploying principal to write secrets
var keyVaultSecretsOfficerRoleId = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'

resource deployingPrincipalKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployKeyVault && !empty(deployingPrincipalId)) {
  name: guid(resourceGroup().id, keyVaultName, deployingPrincipalId, keyVaultSecretsOfficerRoleId)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsOfficerRoleId)
    principalId: deployingPrincipalId
    principalType: 'ServicePrincipal'
  }
  dependsOn: [
    keyVault
  ]
}

// ============================================================================
// App Configuration Role Assignments for Function Apps
// ============================================================================

// App Configuration Data Reader role ID
var appConfigDataReaderRoleId = '516239f1-63e1-4d78-a4de-a74fb236a071'

resource alertMgmtAppConfigAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployAlertManagementFunction && deployAppConfiguration) {
  name: guid(resourceGroup().id, appConfigurationName, alertMgmtFunctionName, appConfigDataReaderRoleId)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', appConfigDataReaderRoleId)
    #disable-next-line BCP318
    principalId: alertManagementFunction.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

resource userMgmtAppConfigAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployUserManagementFunction && deployAppConfiguration) {
  name: guid(resourceGroup().id, appConfigurationName, userMgmtFunctionName, appConfigDataReaderRoleId)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', appConfigDataReaderRoleId)
    #disable-next-line BCP318
    principalId: userManagementFunction.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

// ============================================================================
// Cosmos DB Role Assignments for Function Apps
// ============================================================================

// Cosmos DB Built-in Data Contributor role ID
var cosmosDbDataContributorRoleId = '00000000-0000-0000-0000-000000000002'

resource cosmosDbAccountRef 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = if (deployCosmosDb) {
  name: cosmosDbAccountName
}

resource alertMgmtCosmosDbAccess 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = if (deployAlertManagementFunction && deployCosmosDb) {
  name: guid(cosmosDbAccountName, alertMgmtFunctionName, cosmosDbDataContributorRoleId)
  parent: cosmosDbAccountRef
  properties: {
    roleDefinitionId: '${cosmosDbAccountRef.id}/sqlRoleDefinitions/${cosmosDbDataContributorRoleId}'
    #disable-next-line BCP318
    principalId: alertManagementFunction.outputs.principalId
    scope: cosmosDbAccountRef.id
  }
  dependsOn: [
    cosmosDb
  ]
}

resource userMgmtCosmosDbAccess 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = if (deployUserManagementFunction && deployCosmosDb) {
  name: guid(cosmosDbAccountName, userMgmtFunctionName, cosmosDbDataContributorRoleId)
  parent: cosmosDbAccountRef
  properties: {
    roleDefinitionId: '${cosmosDbAccountRef.id}/sqlRoleDefinitions/${cosmosDbDataContributorRoleId}'
    #disable-next-line BCP318
    principalId: userManagementFunction.outputs.principalId
    scope: cosmosDbAccountRef.id
  }
  dependsOn: [
    cosmosDb
  ]
}

// ============================================================================
// Static Web App
// ============================================================================

module staticWebApp 'modules/static-web-app.bicep' = if (deployStaticWebApp) {
  name: 'staticWebApp-${uniqueString(resourceGroup().id)}'
  params: {
    name: staticWebAppName
    location: staticWebAppLocation
    tags: union(tags, { Service: 'WebApp' })
    sku: staticWebAppSku
    repositoryUrl: repositoryUrl
    branch: branch
    appLocation: './Client/src/Web'
    outputLocation: 'wwwroot'
  }
}

// ============================================================================
// Outputs
// ============================================================================

// SQL Server
@description('SQL Server name')
#disable-next-line BCP318
output sqlServerName string = deploySqlServer && !empty(sqlAzureAdAdminLogin) ? sqlServer.outputs.name : ''

@description('SQL Server FQDN')
#disable-next-line BCP318
output sqlServerFqdn string = deploySqlServer && !empty(sqlAzureAdAdminLogin) ? sqlServer.outputs.fullyQualifiedDomainName : ''

@description('SQL Database name')
#disable-next-line BCP318
output sqlDatabaseName string = deploySqlServer && !empty(sqlAzureAdAdminLogin) ? sqlDatabase.outputs.name : ''

// App Configuration
@description('App Configuration name')
#disable-next-line BCP318
output appConfigurationName string = deployAppConfiguration ? appConfiguration.outputs.name : ''

@description('App Configuration endpoint')
#disable-next-line BCP318
output appConfigurationEndpoint string = deployAppConfiguration ? appConfiguration.outputs.endpoint : ''

// Key Vault
@description('Key Vault name')
#disable-next-line BCP318
output keyVaultName string = deployKeyVault ? keyVault.outputs.name : ''

@description('Key Vault URI')
#disable-next-line BCP318
output keyVaultUri string = deployKeyVault ? keyVault.outputs.vaultUri : ''

// Communication Services
@description('Communication Services name')
#disable-next-line BCP318
output communicationServicesName string = deployCommunicationServices ? communicationServices.outputs.name : ''

@description('Communication Services hostname')
#disable-next-line BCP318
output communicationServicesHostName string = deployCommunicationServices ? communicationServices.outputs.hostName : ''

// Notification Hub
@description('Notification Hub Namespace name')
#disable-next-line BCP318
output notificationHubNamespaceName string = deployNotificationHub ? notificationHub.outputs.namespaceName : ''

@description('Notification Hub name')
#disable-next-line BCP318
output notificationHubName string = deployNotificationHub ? notificationHub.outputs.hubName : ''

// Alert Management Function App
@description('Alert Management Function App name')
#disable-next-line BCP318
output alertManagementFunctionName string = deployAlertManagementFunction ? alertManagementFunction.outputs.name : ''

@description('Alert Management Function App hostname')
#disable-next-line BCP318
output alertManagementFunctionHostName string = deployAlertManagementFunction ? alertManagementFunction.outputs.defaultHostName : ''

@description('Alert Management Function App principal ID')
#disable-next-line BCP318
output alertManagementFunctionPrincipalId string = deployAlertManagementFunction ? alertManagementFunction.outputs.principalId : ''

// User Management Function App
@description('User Management Function App name')
#disable-next-line BCP318
output userManagementFunctionName string = deployUserManagementFunction ? userManagementFunction.outputs.name : ''

@description('User Management Function App hostname')
#disable-next-line BCP318
output userManagementFunctionHostName string = deployUserManagementFunction ? userManagementFunction.outputs.defaultHostName : ''

@description('User Management Function App principal ID')
#disable-next-line BCP318
output userManagementFunctionPrincipalId string = deployUserManagementFunction ? userManagementFunction.outputs.principalId : ''

// Static Web App
@description('Static Web App name')
#disable-next-line BCP318
output staticWebAppName string = deployStaticWebApp ? staticWebApp.outputs.name : ''

@description('Static Web App hostname')
#disable-next-line BCP318
output staticWebAppHostName string = deployStaticWebApp ? staticWebApp.outputs.defaultHostName : ''

// Cosmos DB
@description('Cosmos DB account name')
#disable-next-line BCP318
output cosmosDbAccountName string = deployCosmosDb ? cosmosDb.outputs.name : ''

@description('Cosmos DB endpoint')
#disable-next-line BCP318
output cosmosDbEndpoint string = deployCosmosDb ? cosmosDb.outputs.endpoint : ''

@description('Cosmos DB database name')
#disable-next-line BCP318
output cosmosDbDatabaseName string = deployCosmosDb ? cosmosDb.outputs.databaseName : ''
