# Client/src

The frontends. The web app and the mobile app share the same Blazor UI and the same service layer.

| Folder | Project | Purpose |
| --- | --- | --- |
| [`Core`](./Core) | `AlertOne.Core` | Models, DTOs, interfaces, view-model bases, `Result`/`Error`. No UI. |
| [`Blazor`](./Blazor) | `AlertOne.Shared.Blazor` | Shared Blazor pages, layout, global state, JWT authentication and the HTTP auth handler. |
| [`Domains`](./Domains) | `Domains.AlertManagement`, `Domains.UserManagement` | Feature modules: pages, components, API clients and view models per domain. |
| [`Web`](./Web) | `AlertOne.Web` | Blazor WebAssembly host. |
| [`Maui`](./Maui) | `AlertOne.Maui` | .NET MAUI Blazor Hybrid host (Android, iOS, Mac Catalyst, Windows). |

## Dependency direction

```
Web / Maui -> Domains -> Shared -> Core
```

## Rules

- Hosts (`Web`, `Maui`) differ only in platform services: token storage, push registration, app version, deep links. Everything else is shared.
- The browser cannot use Azure SDK credentials. Do not add Azure App Configuration or Key Vault to the Web project; put API URLs in `Web/wwwroot/appsettings*.json`.
- API base URLs are read from the `Api:UserManagement:BaseUri` and `Api:AlertManagement:BaseUri` configuration keys.
