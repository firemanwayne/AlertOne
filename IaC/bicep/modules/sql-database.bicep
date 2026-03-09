@description('Name of the SQL Database')
param name string

@description('Name of the SQL Server')
param sqlServerName string

@description('Location for the SQL Database')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('SKU name for the database')
@allowed([
  'Basic'
  'S0'
  'S1'
  'S2'
  'GP_S_Gen5_1'
  'GP_S_Gen5_2'
  'GP_Gen5_2'
  'GP_Gen5_4'
])
param skuName string = 'GP_S_Gen5_1'

@description('SKU tier for the database')
@allowed([
  'Basic'
  'Standard'
  'GeneralPurpose'
])
param skuTier string = 'GeneralPurpose'

@description('SKU family (for vCore-based)')
param skuFamily string = 'Gen5'

@description('SKU capacity (vCores or DTUs)')
param skuCapacity int = 1

@description('Max size in bytes (default 36GB)')
param maxSizeBytes int = 38654705664

@description('Auto-pause delay in minutes (-1 to disable)')
param autoPauseDelay int = 60

@description('Minimum capacity for serverless')
param minCapacity string = '1'

@description('Collation for the database')
param collation string = 'SQL_Latin1_General_CP1_CI_AS'

@description('Backup storage redundancy')
@allowed([
  'Local'
  'Zone'
  'Geo'
  'GeoZone'
])
param requestedBackupStorageRedundancy string = 'Local'

@description('Zone redundancy')
param zoneRedundant bool = false

var isServerless = contains(skuName, '_S_')

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
  name: sqlServerName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: name
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuTier
    family: skuFamily
    capacity: skuCapacity
  }
  properties: {
    collation: collation
    maxSizeBytes: maxSizeBytes
    autoPauseDelay: isServerless ? autoPauseDelay : null
    minCapacity: isServerless ? json(minCapacity) : null
    requestedBackupStorageRedundancy: requestedBackupStorageRedundancy
    zoneRedundant: zoneRedundant
    readScale: 'Disabled'
  }
}

@description('The resource ID of the SQL Database')
output id string = sqlDatabase.id

@description('The name of the SQL Database')
output name string = sqlDatabase.name

@description('Connection string template (requires authentication)')
output connectionStringTemplate string = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${name};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
