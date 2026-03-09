@description('Name of the Communication Services resource')
param name string

@description('Tags for the resource')
param tags object = {}

@description('Data location for the Communication Services')
@allowed([
  'United States'
  'Europe'
  'UK'
  'Australia'
  'Japan'
  'Brazil'
])
param dataLocation string = 'United States'

@description('Enable system-assigned managed identity')
param enableSystemAssignedIdentity bool = true

@description('Resource IDs of linked email domains')
param linkedDomains array = []

// Communication Services is always global
var location = 'global'

resource communicationServices 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: name
  location: location
  tags: tags
  identity: enableSystemAssignedIdentity ? {
    type: 'SystemAssigned'
  } : null
  properties: {
    dataLocation: dataLocation
    linkedDomains: linkedDomains
  }
}

@description('The resource ID of the Communication Services')
output id string = communicationServices.id

@description('The name of the Communication Services')
output name string = communicationServices.name

@description('The hostname of the Communication Services')
output hostName string = communicationServices.properties.hostName

@description('The principal ID of the managed identity')
output principalId string = enableSystemAssignedIdentity ? communicationServices.identity.principalId : ''
