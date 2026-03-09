@description('Name of the Application Insights resource')
param name string

@description('Location for the Application Insights resource')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('Resource ID of the Log Analytics Workspace (optional)')
param workspaceResourceId string = ''

@description('Application type')
@allowed([
  'web'
  'other'
])
param applicationType string = 'web'

@description('Retention period in days')
@minValue(30)
@maxValue(730)
param retentionInDays int = 90

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: applicationType
    RetentionInDays: retentionInDays
    WorkspaceResourceId: !empty(workspaceResourceId) ? workspaceResourceId : null
    IngestionMode: !empty(workspaceResourceId) ? 'LogAnalytics' : 'ApplicationInsights'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('The resource ID of the Application Insights')
output id string = applicationInsights.id

@description('The name of the Application Insights')
output name string = applicationInsights.name

@description('The instrumentation key')
output instrumentationKey string = applicationInsights.properties.InstrumentationKey

@description('The connection string')
output connectionString string = applicationInsights.properties.ConnectionString
