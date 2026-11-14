# Api/lib

Shared libraries used by both APIs ([`Api/src/alerts`](../src/alerts) and [`Api/src/usrm`](../src/usrm)). Nothing here is deployed on its own.

| Folder | Purpose |
| --- | --- |
| [`core`](./core) | `AlertOne.Api.Core`: dependency-free abstractions (`DataModelBase`, `IEmailService`, `IPushNotificationService`). |
| [`infrastructure`](./infrastructure) | Adapters for external services: App Configuration, Communication Services (email), Notification Hubs (push) and Cosmos DB initialization. |

## Dependency direction

```
API Client (Azure Functions) -> API Infrastructure -> lib/infrastructure -> lib/core
```

`core` never references anything above it. The infrastructure libraries implement interfaces that live in `core`, so the APIs depend on the interface and the host wires up the implementation.
