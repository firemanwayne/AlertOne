@description('Name of the storage account')
param name string

@description('Location for the storage account')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('Storage account SKU')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
])
param sku string = 'Standard_LRS'

@description('Storage account kind')
@allowed([
  'Storage'
  'StorageV2'
  'BlobStorage'
])
param kind string = 'StorageV2'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: kind
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource deploymentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobServices
  name: 'deployments'
  properties: {
    publicAccess: 'None'
  }
}

@description('The resource ID of the storage account')
output id string = storageAccount.id

@description('The name of the storage account')
output name string = storageAccount.name

@description('The primary connection string for the storage account')
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'

@description('The primary blob endpoint')
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob

@description('The deployments container URL for Flex Consumption')
output deploymentsContainerUrl string = '${storageAccount.properties.primaryEndpoints.blob}deployments'
