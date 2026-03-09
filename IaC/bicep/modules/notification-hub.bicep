@description('Name of the Notification Hub Namespace')
param namespaceName string

@description('Name of the Notification Hub')
param hubName string

@description('Location for the Notification Hub')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('SKU for the Notification Hub Namespace')
@allowed([
  'Free'
  'Basic'
  'Standard'
])
param sku string = 'Free'

@description('Public network access')
@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccess string = 'Enabled'

@description('Zone redundancy')
param zoneRedundancy bool = false

// Notification Hub Namespace
resource notificationHubNamespace 'Microsoft.NotificationHubs/namespaces@2023-10-01-preview' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    namespaceType: 'NotificationHub'
    publicNetworkAccess: publicNetworkAccess
    zoneRedundancy: zoneRedundancy ? 'Enabled' : 'Disabled'
  }
}

// Notification Hub
resource notificationHub 'Microsoft.NotificationHubs/namespaces/notificationHubs@2023-10-01-preview' = {
  parent: notificationHubNamespace
  name: hubName
  location: location
  tags: tags
  properties: {}
}

@description('The resource ID of the Notification Hub Namespace')
output namespaceId string = notificationHubNamespace.id

@description('The name of the Notification Hub Namespace')
output namespaceName string = notificationHubNamespace.name

@description('The resource ID of the Notification Hub')
output hubId string = notificationHub.id

@description('The name of the Notification Hub')
output hubName string = notificationHub.name

@description('The Service Bus endpoint')
output serviceBusEndpoint string = notificationHubNamespace.properties.serviceBusEndpoint
