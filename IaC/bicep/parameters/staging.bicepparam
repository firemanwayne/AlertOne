using '../main.bicep'

param environment = 'staging'
param location = 'eastus2'
param instance = '001'
param repositoryUrl = 'https://github.com/firemanwayne/AlertOne'
param branch = 'master'
param deployStaticWebApp = true
param deployAlertManagementFunction = true
param deployUserManagementFunction = true
param deployAppConfiguration = true
param appConfigurationSku = 'standard'

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
