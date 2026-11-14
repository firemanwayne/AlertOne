# AlertOne.Shared.Blazor

Blazor pieces used by both hosts (Web and MAUI).

The project, assembly and root namespace are all `AlertOne.Shared.Blazor`, so types and Razor components live under `AlertOne.Shared.Blazor.*` (for example `AlertOne.Shared.Blazor.States.AppState`). It lives in `Client/src/Blazor`.

| Folder | Contents |
| --- | --- |
| `Auth` | `JwtAuthenticationStateProvider`: builds the signed-in user from the stored access token (anonymous when missing or expired). |
| `Handlers` | `JwtAuthHeaderHandler`: adds the bearer token to API calls and refreshes it first when it is expired or about to expire; `/auth/` calls are sent unchanged. |
| `States` | `AppState` (signed-in user, token, loading flag) and `NotificationState` (received notifications, newest first, capped). |
| `Layout` | `MainLayout`. |
| `Pages` | Home, Notifications and Profile pages. |
| `ServiceRegistryExtensions.cs` | Shared registrations and constants. |

Every route requires an authenticated user by default (the hosts set a fallback authorization policy), so pages that must be public, such as login, opt out explicitly.
