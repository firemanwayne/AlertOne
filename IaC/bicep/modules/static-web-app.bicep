@description('Name of the Static Web App')
param name string

@description('Location for the Static Web App')
param location string = resourceGroup().location

@description('Tags for the resource')
param tags object = {}

@description('SKU for the Static Web App')
@allowed([
  'Free'
  'Standard'
])
param sku string = 'Free'

@description('GitHub repository URL (optional - for linking)')
param repositoryUrl string = ''

@description('GitHub branch (optional)')
param branch string = 'master'

@description('Build configuration - App location')
param appLocation string = './Client/src/Web'

@description('Build configuration - Output location')
param outputLocation string = 'wwwroot'

@description('Build configuration - API location (optional)')
param apiLocation string = ''

@description('Allow configuration file overrides')
param allowConfigFileUpdates bool = true

@description('Staging environment policy')
@allowed([
  'Enabled'
  'Disabled'
])
param stagingEnvironmentPolicy string = 'Enabled'

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
    tier: sku
  }
  properties: {
    repositoryUrl: !empty(repositoryUrl) ? repositoryUrl : null
    branch: !empty(repositoryUrl) ? branch : null
    buildProperties: {
      appLocation: appLocation
      outputLocation: outputLocation
      apiLocation: !empty(apiLocation) ? apiLocation : null
    }
    stagingEnvironmentPolicy: stagingEnvironmentPolicy
    allowConfigFileUpdates: allowConfigFileUpdates
  }
}

@description('The resource ID of the Static Web App')
output id string = staticWebApp.id

@description('The name of the Static Web App')
output name string = staticWebApp.name

@description('The default hostname of the Static Web App')
output defaultHostName string = staticWebApp.properties.defaultHostname

@description('The API key for deployment (use with caution)')
output apiKey string = staticWebApp.listSecrets().properties.apiKey
