using '../main.bicep'

param environment = 'dev'
param location = 'eastus2'
param instance = '001'
param repositoryUrl = 'https://github.com/firemanwayne/AlertOne'
param branch = 'master'

// Azure AD Configuration for SQL Server
param sqlAzureAdAdminLogin = 'admin@wdavis2002gmailcom.onmicrosoft.com'
param sqlAzureAdAdminObjectId = '532dd4be-5499-4444-a7ea-bee09411b9b6'

// Deploying principal (GitHub Actions service principal)
param deployingPrincipalId = '1f766ee6-03e9-499f-9156-b671c0b3222b'

// Deployment toggles
param deployStaticWebApp = true
param deployAlertManagementFunction = true
param deployUserManagementFunction = true
param deployAppConfiguration = true
param deploySqlServer = false
param deployCosmosDb = true
param deployKeyVault = true
param deployCommunicationServices = true
param deployNotificationHub = true

param appConfigurationSku = 'developer'

// SKU parameters
param appServicePlanSku = 'FC1'
param sqlDatabaseSkuName = 'GP_S_Gen5_1'
param sqlDatabaseSkuTier = 'GeneralPurpose'
param sqlDatabaseSkuFamily = 'Gen5'
param sqlDatabaseSkuCapacity = 1
param staticWebAppSku = 'Standard'
param keyVaultSku = 'standard'
param notificationHubSku = 'Free'
param storageAccountSku = 'Standard_LRS'
