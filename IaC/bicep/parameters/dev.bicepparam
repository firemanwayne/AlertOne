using '../main.bicep'

param environment = 'dev'
param location = 'eastus2'
param locationAbbreviation = 'eastus2'
param instance = '001'
param repositoryUrl = 'https://github.com/firemanwayne/AlertOne'
param branch = 'master'

// Azure AD Configuration for SQL Server
param sqlAzureAdAdminLogin = 'admin@wdavis2002gmailcom.onmicrosoft.com'
param sqlAzureAdAdminObjectId = '532dd4be-5499-4444-a7ea-bee09411b9b6'

// Deployment toggles
param deployStaticWebApp = true
param deployAlertManagementFunction = true
param deployUserManagementFunction = true
param deployAppConfiguration = true
param deploySqlServer = true
param deployKeyVault = true
param deployCommunicationServices = true
param deployNotificationHub = true

param appConfigurationSku = 'free'

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
