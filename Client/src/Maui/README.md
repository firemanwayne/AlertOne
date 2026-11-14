# AlertOne.Maui

The .NET MAUI Blazor Hybrid app. `MainPage` hosts a `BlazorWebView` that renders the same Blazor UI as the web app.

## Platform code

| Path | Purpose |
| --- | --- |
| `MauiProgram.cs` | Builds the app: MudBlazor, Blazor web view, shared state, token storage, push registration, deep links and the domain services. |
| `Services/MauiTokenStorageService` | Stores tokens in MAUI secure storage. |
| `Services/PushNotificationRegistrationService` | Push registration and events (supported on Android and iOS). |
| `Services/DeepLinkService` | Maps `alertone://` and `https://` links, for example invitation links, to app routes. |
| `Services/MauiAppVersionService` | Version and build from `AppInfo`. |
| `Platforms/` | Android (Firebase messaging service, deep link intents), iOS and Mac Catalyst (app delegates, deep links), Windows, Tizen. |

## API URLs

The app reads the same `Api:UserManagement:BaseUri` and `Api:AlertManagement:BaseUri` keys as the web app. Aspire injects service addresses for `Aspire/AlertOne.ServiceDefaults.Maui`. See [`MAUI_CONNECTION_GUIDE.md`](../../../Api/MAUI_CONNECTION_GUIDE.md) for connecting a device or emulator to the local APIs (it predates the Aspire setup, so check the ports against [`Aspire`](../../../Aspire)).

## Build

Needs the .NET MAUI workload. Windows: `dotnet build -f net9.0-windows10.0.19041.0`. Android and iOS need their SDKs and (for Android push) Firebase configuration.
