# Api/lib/core

## AlertOne.Api.Core

The contract layer shared by both APIs. It has **no external package dependencies**, so it can be referenced from anywhere without pulling in Azure or EF Core.

| Type | Purpose |
| --- | --- |
| `Abstractions/DataModelBase` | Base class for stored records: `Id`, `CreatedDate`, `CreatedBy`, `UpdatedDate`, `UpdatedBy` with change notification through `SetProperty`. |
| `Interfaces/IEmailService` | Sends an email (`SendEmailAsync`) and the invitation email with its activation code (`SendInvitationEmailAsync`). |
| `Interfaces/IPushNotificationService` | Registers and unregisters devices and sends push notifications to a user, to tags, or to everyone. |

## Implementations

The interfaces are implemented in [`../infrastructure`](../infrastructure):

- `IEmailService` -> `Azure.Communications` (or a logging-only stand-in when no connection string is configured).
- `IPushNotificationService` -> `Azure.NotificationHubs` (or a logging-only stand-in when no connection string is configured).

## Rules for this project

- Keep it free of package references. Anything that needs Azure, EF Core or ASP.NET Core belongs in `infrastructure` or in an API project.
- One type per file; interfaces go in `Interfaces/`.
