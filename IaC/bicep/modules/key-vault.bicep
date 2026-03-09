@description('Name of the Key Vault')
param name string

@description('Location for the Key Vault')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('SKU for the Key Vault')
@allowed([
  'standard'
  'premium'
])
param sku string = 'standard'

@description('Tenant ID for the Key Vault')
param tenantId string = subscription().tenantId

@description('Enable RBAC authorization (recommended)')
param enableRbacAuthorization bool = true

@description('Enable soft delete')
param enableSoftDelete bool = true

@description('Soft delete retention in days')
@minValue(7)
@maxValue(90)
param softDeleteRetentionInDays int = 90

@description('Enable purge protection')
param enablePurgeProtection bool = false

@description('Public network access')
@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccess string = 'Enabled'

@description('Enable for deployment (VMs)')
param enabledForDeployment bool = false

@description('Enable for disk encryption')
param enabledForDiskEncryption bool = false

@description('Enable for template deployment')
param enabledForTemplateDeployment bool = false

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: sku
    }
    tenantId: tenantId
    enableRbacAuthorization: enableRbacAuthorization
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: softDeleteRetentionInDays
    enablePurgeProtection: enablePurgeProtection ? true : null
    enabledForDeployment: enabledForDeployment
    enabledForDiskEncryption: enabledForDiskEncryption
    enabledForTemplateDeployment: enabledForTemplateDeployment
    publicNetworkAccess: publicNetworkAccess
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

@description('The resource ID of the Key Vault')
output id string = keyVault.id

@description('The name of the Key Vault')
output name string = keyVault.name

@description('The URI of the Key Vault')
output vaultUri string = keyVault.properties.vaultUri
