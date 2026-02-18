# Notification Integration Guide

## Overview

The User Management API now supports device tracking with push notification tokens, enabling a separate notification service (Azure Function App) to query and send notifications to all of a user's registered devices.

## Architecture

```
┌─────────────────────────────┐
│  User Management API        │
│  (This Function App)        │
│                             │
│  - Stores device info       │
│  - Stores push tokens       │
│  - Manages device sessions  │
└──────────────┬──────────────┘
               │
               │ HTTP Request
               │ GET /api/users/{userId}/devices/notifications
               │
               ▼
┌─────────────────────────────┐
│  Notification Service       │
│  (Separate Function App)    │
│                             │
│  - Queries device list      │
│  - Sends push notifications │
│  - Handles delivery status  │
└─────────────────────────────┘
```

## Features

### 1. Push Notification Token Storage

**UserDevice Entity**:
- `PushNotificationToken` (optional) - Stores FCM/APNs/Web Push tokens
- Configured with max length of 512 characters
- Nullable to support devices that don't use push notifications

### 2. Device Registration with Push Tokens

**DeviceInfo Request** (used during login/registration):
```csharp
public sealed record DeviceInfo(
    string DeviceId,
    string DeviceName,
    DeviceType DeviceType,      // Web, Mobile, Desktop, Unknown
    string Platform,              // iOS, Android, Windows, etc.
    string? AppVersion,
    string? UserAgent,
    string? PushNotificationToken // NEW: FCM/APNs token
);
```

**Example Login Request**:
```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "device": {
    "deviceId": "unique-device-identifier",
    "deviceName": "John's iPhone",
    "deviceType": 2,  // Mobile
    "platform": "iOS",
    "appVersion": "1.0.0",
    "userAgent": "MyApp/1.0.0",
    "pushNotificationToken": "fcm-token-here-or-apns-token"
  }
}
```

### 3. Notification Service Endpoint

**Endpoint**: `GET /api/users/{userId}/devices/notifications`

**Purpose**: Returns all active devices with push notification tokens for a specific user

**Response Model** (`DeviceNotificationInfo`):
```csharp
public sealed record DeviceNotificationInfo(
    Guid DeviceId,
    DeviceType DeviceType,    // Web=1, Mobile=2, Desktop=3
    string Platform,           // "iOS", "Android", "Web", etc.
    string? PushNotificationToken,
    bool IsActive
);
```

**Example Response**:
```json
[
  {
    "deviceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "deviceType": 2,
    "platform": "iOS",
    "pushNotificationToken": "apns-device-token-abc123",
    "isActive": true
  },
  {
    "deviceId": "7ba92c84-1234-5678-b3fc-2c963f66afa7",
    "deviceType": 2,
    "platform": "Android",
    "pushNotificationToken": "fcm-device-token-xyz789",
    "isActive": true
  },
  {
    "deviceId": "9cd43a95-8765-4321-b3fc-2c963f66afa8",
    "deviceType": 1,
    "platform": "Web",
    "pushNotificationToken": "web-push-token-def456",
    "isActive": true
  }
]
```

## Integration Steps for Notification Service

### Step 1: Query User Devices

**HTTP Request**:
```http
GET /api/users/{userId}/devices/notifications
Content-Type: application/json
Authorization: Bearer {service-token}  // Optional: Add auth if needed
```

**C# Example**:
```csharp
public async Task<IEnumerable<DeviceNotificationInfo>> GetUserDevices(Guid userId)
{
    using var client = new HttpClient();
    client.BaseAddress = new Uri("https://your-user-management-api.azurewebsites.net");

    var response = await client.GetAsync($"/api/users/{userId}/devices/notifications");
    response.EnsureSuccessStatusCode();

    var devices = await response.Content
        .ReadFromJsonAsync<IEnumerable<DeviceNotificationInfo>>();

    return devices ?? Enumerable.Empty<DeviceNotificationInfo>();
}
```

### Step 2: Send Notifications Based on Platform

**Recommended Approach**:
```csharp
public async Task SendNotificationToAllDevices(Guid userId, string title, string message)
{
    var devices = await GetUserDevices(userId);

    var tasks = devices
        .Where(d => !string.IsNullOrEmpty(d.PushNotificationToken))
        .Select(device => device.DeviceType switch
        {
            DeviceType.Mobile when device.Platform == "iOS"
                => SendApnsNotification(device.PushNotificationToken, title, message),

            DeviceType.Mobile when device.Platform == "Android"
                => SendFcmNotification(device.PushNotificationToken, title, message),

            DeviceType.Web
                => SendWebPushNotification(device.PushNotificationToken, title, message),

            _ => Task.CompletedTask
        });

    await Task.WhenAll(tasks);
}
```

### Step 3: Handle Invalid Tokens

When a push notification fails due to an invalid token, you should revoke or update the device:

**Option A - Revoke Device** (if permanently invalid):
```http
DELETE /api/devices/{deviceId}
Authorization: Bearer {user-access-token}
```

**Option B - Update Token** (if token was refreshed):
Client app should send new login request with updated token.

## Device Types

| Value | Name    | Description                           |
|-------|---------|---------------------------------------|
| 0     | Unknown | Unknown or unspecified device type    |
| 1     | Web     | Web browser (Chrome, Firefox, etc.)   |
| 2     | Mobile  | Mobile device (iOS, Android)          |
| 3     | Desktop | Desktop application (Windows, macOS)  |

## Platform Examples

- **iOS**: "iOS 17.0", "iOS", "iPhone"
- **Android**: "Android 14", "Android"
- **Web**: "Chrome", "Firefox", "Safari", "Edge"
- **Windows**: "Windows 11", "Windows"
- **macOS**: "macOS 14", "macOS"

## Security Considerations

### Authentication

The notification endpoint currently has `AuthorizationLevel.Anonymous`. For production:

**Option 1 - Service-to-Service Authentication**:
```csharp
[Function("GetDevicesForNotifications")]
public async Task<HttpResponseData> GetDevicesForNotifications(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = ...)]
    HttpRequestData req, ...)
```

Then use function keys in the notification service:
```csharp
client.DefaultRequestHeaders.Add("x-functions-key", functionKey);
```

**Option 2 - Azure Managed Identity**:
Configure both Function Apps to use Managed Identity and validate tokens.

**Option 3 - API Key Validation**:
Add custom middleware to validate API keys from the notification service.

### Data Protection

- Push notification tokens are sensitive
- Only return tokens to authorized services
- Consider encrypting tokens at rest
- Rotate tokens periodically

## Push Notification Services

### Firebase Cloud Messaging (FCM) - Android

**NuGet Package**: `FirebaseAdmin`

```csharp
private async Task SendFcmNotification(string token, string title, string body)
{
    var message = new Message
    {
        Token = token,
        Notification = new Notification
        {
            Title = title,
            Body = body
        }
    };

    await FirebaseMessaging.DefaultInstance.SendAsync(message);
}
```

### Apple Push Notification Service (APNs) - iOS

**NuGet Package**: `dotAPNS`

```csharp
private async Task SendApnsNotification(string token, string title, string body)
{
    var apns = ApnsClient.CreateUsingJwt(
        new ApnsJwtOptions
        {
            BundleId = "com.yourapp.bundle",
            CertContent = p8Certificate,
            KeyId = apnsKeyId,
            TeamId = apnsTeamId
        });

    var push = new ApplePush(ApplePushType.Alert)
        .AddAlert(title, body)
        .AddToken(token);

    await apns.SendAsync(push);
}
```

### Web Push - Progressive Web Apps

**NuGet Package**: `WebPush`

```csharp
private async Task SendWebPushNotification(string subscription, string title, string body)
{
    var webPushClient = new WebPushClient();

    var payload = JsonSerializer.Serialize(new
    {
        notification = new
        {
            title = title,
            body = body,
            icon = "/icon.png"
        }
    });

    await webPushClient.SendNotificationAsync(
        PushSubscription.Parse(subscription),
        payload,
        vapidDetails);
}
```

## Example Notification Service Implementation

```csharp
[Function("SendUserNotification")]
public async Task<HttpResponseData> SendUserNotification(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/send")]
    HttpRequestData req)
{
    var request = await req.ReadFromJsonAsync<SendNotificationRequest>();

    // Get all devices for the user
    var devices = await GetUserDevices(request.UserId);

    // Send to each device
    var results = new List<NotificationResult>();

    foreach (var device in devices)
    {
        try
        {
            await SendNotificationToDevice(device, request.Title, request.Message);
            results.Add(new NotificationResult(device.DeviceId, true, null));
        }
        catch (Exception ex)
        {
            results.Add(new NotificationResult(device.DeviceId, false, ex.Message));

            // If token is invalid, consider revoking the device
            if (IsTokenInvalidException(ex))
            {
                await RevokeDevice(device.DeviceId);
            }
        }
    }

    var response = req.CreateResponse(HttpStatusCode.OK);
    await response.WriteAsJsonAsync(results);
    return response;
}
```

## Token Update Flow

### Client App Flow

1. **App Startup**: Client app gets push notification token from OS
2. **Login/Register**: Include token in DeviceInfo
3. **Token Refresh**: When OS refreshes token, login again to update

### Example (iOS):
```swift
func application(_ application: UIApplication,
                 didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
    let token = deviceToken.map { String(format: "%02.2hhx", $0) }.joined()

    // Store token and include in next login
    UserDefaults.standard.set(token, forKey: "pushToken")
}
```

### Example (Android):
```kotlin
class MyFirebaseMessagingService : FirebaseMessagingService() {
    override fun onNewToken(token: String) {
        // Store token and include in next login
        sharedPreferences.edit()
            .putString("pushToken", token)
            .apply()
    }
}
```

## Monitoring and Analytics

Consider tracking:
- Notification delivery success rate per platform
- Average time to deliver notifications
- Failed token counts (for cleanup)
- Active devices per user
- Notification engagement rates

## Migration Notes

**Database Migration**: `AddPushNotificationToken`
- Adds `PushNotificationToken` column to `UserDevices` table
- Column is nullable, existing devices will have NULL
- Max length: 512 characters

**Backwards Compatibility**:
- Existing devices without tokens continue to work
- Push token is optional in DeviceInfo
- Notification endpoint only returns devices with valid tokens

## Testing

### Test Endpoint
```bash
curl -X GET "http://localhost:7071/api/users/{userId}/devices/notifications" \
  -H "Content-Type: application/json"
```

### Test Response
```json
[
  {
    "deviceId": "test-device-id",
    "deviceType": 2,
    "platform": "iOS",
    "pushNotificationToken": "test-token-123",
    "isActive": true
  }
]
```

## Troubleshooting

### No devices returned
- Verify user has logged in from at least one device
- Check that devices provided push tokens during login
- Ensure devices are active (not revoked)

### Notifications not delivered
- Verify push token is valid and current
- Check platform-specific console (FCM, APNs) for errors
- Ensure notification service has correct credentials
- Verify network connectivity between services

### Token errors
- Tokens can expire or become invalid
- Apps must refresh tokens periodically
- Implement retry logic with exponential backoff

## Summary

✅ **Push notification tokens stored** with device information
✅ **Dedicated endpoint** for notification services to query devices
✅ **Platform-aware** responses for routing to correct push service
✅ **Active device filtering** to avoid sending to revoked devices
✅ **Token update support** through re-authentication
✅ **Multi-device support** for users with multiple devices

The notification service can now query this API to get all active devices for a user and send targeted push notifications to each platform!
