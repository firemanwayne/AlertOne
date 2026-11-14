# Client/src/Domains

Feature modules. Each domain owns its pages, components, view models and the API client for one backend.

| Project | Backend | Contents |
| --- | --- | --- |
| `Domains.AlertManagement` | Alert Management API (`Api:AlertManagement`) | Alert, alert type and status level pages and forms, view models and DTOs, `AlertManagementApiClient` and the alert services. |
| `Domains.UserManagement` | User Management API (`Api:UserManagement`) | Login, registration, users, roles, invitations and settings pages; `UserManagementApiClient`, authentication and user services. |

## Layout of a domain

```
Components/   reusable UI (forms, dialogs)
Pages/        routable pages
Models/       DTOs and view models        (Alert Management)
Options/      API client options (BaseUri, FunctionKey, HTTP client name)
Services/     API client and service implementations (internal)
ServiceRegistryExtensions.cs   AddAlertManagement() / AddUserManagement()
```

## Registering

```csharp
builder.Services
    .AddUserManagement(builder.Configuration)
    .AddAlertManagement(builder.Configuration);
```

Each registration binds `Api:<Domain>` options, adds a typed `HttpClient` with the JWT handler, and throws on startup if `BaseUri` is missing (User Management) so a bad configuration is found immediately. Set the URIs in the host's configuration, for example `Client/src/Web/wwwroot/appsettings.Development.json`:

```json
"Api": {
  "UserManagement":  { "BaseUri": "http://localhost:7071" },
  "AlertManagement": { "BaseUri": "http://localhost:7072" }
}
```

Services are `internal sealed` and consumed through the interfaces in [`Core`](../Core).
