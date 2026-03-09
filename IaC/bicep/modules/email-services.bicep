@description('Name of the Email Services resource')
param name string

@description('Tags for the resource')
param tags object = {}

@description('Data location for the Email Services')
@allowed([
  'United States'
  'Europe'
  'UK'
  'Australia'
])
param dataLocation string = 'United States'

@description('Create an Azure Managed Domain')
param createAzureManagedDomain bool = true

// Email Services is always global
var location = 'global'

resource emailServices 'Microsoft.Communication/emailServices@2023-04-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    dataLocation: dataLocation
  }
}

// Azure Managed Domain (optional)
resource azureManagedDomain 'Microsoft.Communication/emailServices/domains@2023-04-01' = if (createAzureManagedDomain) {
  parent: emailServices
  name: 'AzureManagedDomain'
  location: location
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

@description('The resource ID of the Email Services')
output id string = emailServices.id

@description('The name of the Email Services')
output name string = emailServices.name

@description('The resource ID of the Azure Managed Domain')
output azureManagedDomainId string = createAzureManagedDomain ? azureManagedDomain.id : ''

@description('The from sender domain for the Azure Managed Domain')
output azureManagedDomainFromSenderDomain string = createAzureManagedDomain ? azureManagedDomain.properties.fromSenderDomain : ''
