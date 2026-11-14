# IaC

Infrastructure as code (Bicep) for the Azure deployment. **Not needed for local development**: running through Aspire uses emulators only (see [`../Aspire`](../Aspire)).

## Layout

| Path | Contents |
| --- | --- |
| `bicep/main.bicep` | Entry template (resource group scope). Each resource group of components has a `deploy*` switch parameter. |
| `bicep/main.json` | Compiled ARM template generated from `main.bicep`. |
| `bicep/modules/` | One module per resource type: App Configuration (and values), App Service plan, Application Insights, Communication Services, Email Services, Cosmos DB, Function App, Key Vault, Notification Hub, SQL server and database, Static Web App, Storage account. |
| `bicep/parameters/` | Parameter files for `dev`, `staging` and `prod` (`*.bicepparam`) plus per-API parameter JSON files. |

## Switches in `main.bicep`

`deployStaticWebApp`, `deployAlertManagementFunction`, `deployUserManagementFunction`, `deployAppConfiguration`, `deploySqlServer` (default off), `deployCosmosDb`, `deployKeyVault`, `deployCommunicationServices`, `deployNotificationHub`.

## Deploy

```powershell
az deployment group create `
  --resource-group <resource-group> `
  --template-file IaC/bicep/main.bicep `
  --parameters IaC/bicep/parameters/dev.bicepparam
```

Use `staging.bicepparam` or `prod.bicepparam` for the other environments. Preview a change first with `az deployment group what-if` and the same arguments.

## Notes

- App Configuration holds the settings the APIs read in Azure, including the API base URIs and function keys used by the web client.
- The Function Apps are granted Cosmos DB data access by role assignment, so Azure deployments use managed identity (no account key).
