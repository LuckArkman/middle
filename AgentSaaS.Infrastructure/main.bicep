param location string = resourceGroup().location
param appName string = 'agentsaas'

// 1. Container Registry (Para guardar as imagens Docker)
resource acr 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: '${appName}acr'
  location: location
  sku: { name: 'Basic' }
  properties: { adminUserEnabled: true }
}

// 2. Azure Container Apps Environment (Onde roda o Web, Orchestrator e Agentes)
resource env 'Microsoft.App/managedEnvironments@2022-03-01' = {
  name: '${appName}-env'
  location: location
  properties: {
    appLogsConfiguration: { destination: 'log-analytics' }
  }
}

// 3. PostgreSQL Flexible Server
resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' = {
  name: '${appName}-db'
  location: location
  sku: { name: 'Standard_B1ms', tier: 'Burstable' }
  properties: {
    version: '15'
    storage: { storageSizeGB: 32 }
    administratorLogin: 'saas_admin'
    administratorLoginPassword: 'ChangeMe123!' 
  }
}

// 4. Redis Cache
resource redis 'Microsoft.Cache/Redis@2023-04-01' = {
  name: '${appName}-redis'
  location: location
  properties: { sku: { name: 'Basic', family: 'C', capacity: 0 } }
}