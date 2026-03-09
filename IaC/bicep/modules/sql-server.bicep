@description('Name of the SQL Server')
param name string

@description('Location for the SQL Server')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('Azure AD administrator login')
param azureAdAdminLogin string

@description('Azure AD administrator object ID (SID)')
param azureAdAdminObjectId string

@description('Azure AD administrator principal type')
@allowed([
  'User'
  'Group'
  'Application'
])
param azureAdAdminPrincipalType string = 'User'

@description('Azure AD tenant ID')
param azureAdTenantId string = subscription().tenantId

@description('Enable Azure AD only authentication')
param azureAdOnlyAuthentication bool = true

@description('Minimum TLS version')
@allowed([
  '1.0'
  '1.1'
  '1.2'
])
param minimalTlsVersion string = '1.2'

@description('Public network access')
@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccess string = 'Enabled'

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: name
  location: location
  tags: tags
  properties: {
    minimalTlsVersion: minimalTlsVersion
    publicNetworkAccess: publicNetworkAccess
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: azureAdOnlyAuthentication
      login: azureAdAdminLogin
      principalType: azureAdAdminPrincipalType
      sid: azureAdAdminObjectId
      tenantId: azureAdTenantId
    }
  }
}

// Allow Azure services to access the server
resource firewallRuleAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

@description('The resource ID of the SQL Server')
output id string = sqlServer.id

@description('The name of the SQL Server')
output name string = sqlServer.name

@description('The fully qualified domain name of the SQL Server')
output fullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
