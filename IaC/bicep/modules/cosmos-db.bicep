@description('Name of the Cosmos DB account')
param name string

@description('Location for the Cosmos DB account')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('Capacity mode for Cosmos DB')
@allowed([
  'Serverless'
  'Provisioned'
])
param capacityMode string = 'Serverless'

@description('Database name')
param databaseName string = 'alertone-db'

@description('Enable automatic failover')
param enableAutomaticFailover bool = false

@description('Default consistency level')
@allowed([
  'Eventual'
  'ConsistentPrefix'
  'Session'
  'BoundedStaleness'
  'Strong'
])
param defaultConsistencyLevel string = 'Session'

// Cosmos DB Account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: name
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: defaultConsistencyLevel
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: capacityMode == 'Serverless' ? [
      {
        name: 'EnableServerless'
      }
    ] : []
    enableAutomaticFailover: enableAutomaticFailover
    enableMultipleWriteLocations: false
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

// Cosmos DB Database
resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

// Container definitions
var containers = [
  // User Management containers
  {
    name: 'users'
    partitionKeyPath: '/id'
  }
  {
    name: 'user-devices'
    partitionKeyPath: '/userId'
  }
  {
    name: 'user-invitations'
    partitionKeyPath: '/email'
  }
  {
    name: 'user-settings'
    partitionKeyPath: '/userId'
  }
  {
    name: 'roles'
    partitionKeyPath: '/id'
  }
  {
    name: 'user-roles'
    partitionKeyPath: '/userId'
  }
  {
    name: 'user-claims'
    partitionKeyPath: '/userId'
  }
  {
    name: 'user-logins'
    partitionKeyPath: '/userId'
  }
  {
    name: 'user-tokens'
    partitionKeyPath: '/userId'
  }
  {
    name: 'role-claims'
    partitionKeyPath: '/roleId'
  }
  // Alert Management containers
  {
    name: 'alerts'
    partitionKeyPath: '/alertTypeId'
  }
  {
    name: 'alert-types'
    partitionKeyPath: '/id'
  }
  {
    name: 'alert-type-users'
    partitionKeyPath: '/alertTypeId'
  }
  {
    name: 'alert-status-levels'
    partitionKeyPath: '/id'
  }
  {
    name: 'alert-history'
    partitionKeyPath: '/alertId'
  }
]

// Create containers
resource cosmosContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = [for container in containers: {
  parent: cosmosDatabase
  name: container.name
  properties: {
    resource: {
      id: container.name
      partitionKey: {
        paths: [
          container.partitionKeyPath
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
    }
  }
}]

// Outputs
@description('The resource ID of the Cosmos DB account')
output id string = cosmosAccount.id

@description('The name of the Cosmos DB account')
output name string = cosmosAccount.name

@description('The endpoint of the Cosmos DB account')
output endpoint string = cosmosAccount.properties.documentEndpoint

@description('The database name')
output databaseName string = cosmosDatabase.name

@description('The principal ID of the managed identity')
output principalId string = cosmosAccount.identity.principalId
