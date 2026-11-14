# AlertOne.Core

The UI-free foundation of the client. Referenced by every other client project.

| Folder | Contents |
| --- | --- |
| `Abstractions` | `ViewModelBase` (change notification, spinner state, audit fields), `ListViewModelBase`, `DataModelBase`, `ApiOptionsBase`. |
| `Attributes` | Validation attributes such as `NotEmptyGuidAttribute`. |
| `Common` | `Result` / `Result<T>` and `Error` used by API clients instead of exceptions. |
| `DTOs` | `Requests` and `Responses` shared with the APIs. |
| `Enums` | `DeviceType`, `ErrorType`. |
| `Interfaces` | Service contracts: `IApiClient`, `IAuthenticationService`, `IUserManagementService`, `IRoleManagementService`, `IInvitationService`, `IUserSettingsService`, `ITokenStorageService`, `IPushNotificationRegistrationService`, `IAppVersionService`. |
| `Models` | `AppSettings`, `AuthState`, `UserProfile`. |
| `ClaimsPrincipalExtensions.cs` | `GetUserId`, `GetEmail`, `GetFirstName`, `GetLastName` from JWT claims. |

Platform-specific implementations of `ITokenStorageService`, `IPushNotificationRegistrationService` and `IAppVersionService` live in the `Web` and `Maui` projects.
