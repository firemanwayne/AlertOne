# AlertOne.Web

The Blazor WebAssembly host. It runs entirely in the browser and calls the APIs over HTTP.

| Path | Purpose |
| --- | --- |
| `Program.cs` | Registers MudBlazor, shared state, authorization (every route requires a signed-in user), token storage, and the User and Alert management domains. |
| `Services/BrowserTokenStorageService` | Stores tokens in browser local storage. |
| `Services/WebPushNotificationService` | Stub: push notifications are not supported in the browser. |
| `Services/WebAppVersionService` | Version and build from the assembly's informational version. |
| `wwwroot/appsettings*.json` | Client configuration (API URLs). |

## Configuration (`wwwroot/appsettings.Development.json`)

```json
{
  "Services": { "api": { "http": ["http://localhost:7071"] },
                "alerts-api": { "http": ["http://localhost:7072"] } },
  "Api": { "UserManagement":  { "BaseUri": "http://localhost:7071" },
           "AlertManagement": { "BaseUri": "http://localhost:7072" } }
}
```

These files are downloaded by the browser, so they must never contain secrets (function keys, connection strings). The URLs must match the ports pinned in [`Aspire/AlertOne.AppHost`](../../../Aspire).

## Notes

- Browser code cannot use `DefaultAzureCredential`, and configuration providers that block (such as Azure App Configuration) fail on the single-threaded WebAssembly runtime with `Cannot wait on monitors on this runtime`. Keep Azure SDK dependencies out of this project.
- Run it through the Aspire AppHost; the app is served over HTTPS on a port assigned by Aspire.
