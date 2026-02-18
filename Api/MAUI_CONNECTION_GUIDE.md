# MAUI App Connection Guide

This guide explains how to connect your .NET MAUI application to the Azure Functions API running on your development machine.

## The Problem

When running the Azure Function locally on `http://localhost:7071`, mobile emulators and physical devices cannot connect because:
- `localhost` refers to the device itself, not your host machine
- The network configuration differs by platform (Android, iOS, Windows)

## Solution by Platform

### Android Emulator

**Use the special loopback address**: `10.0.2.2`

```csharp
// In your MAUI app configuration
#if ANDROID
    private const string BaseUrl = "http://10.0.2.2:7071/api";
#endif
```

**Why?** The Android emulator maps `10.0.2.2` to the host machine's `127.0.0.1`.

### iOS Simulator

**Use**: `localhost` or `127.0.0.1`

```csharp
#if IOS
    private const string BaseUrl = "http://localhost:7071/api";
#endif
```

**Why?** The iOS simulator shares the network stack with the host machine, so `localhost` works directly.

### Windows (WinUI)

**Use**: `localhost` or `127.0.0.1`

```csharp
#if WINDOWS
    private const string BaseUrl = "http://localhost:7071/api";
#endif
```

### Physical Devices (Android/iOS)

**Use your host machine's IP address**: `http://192.168.1.84:7071/api`

```csharp
// For physical devices on the same network
private const string BaseUrl = "http://192.168.1.84:7071/api";
```

**How to find your IP address:**

**Windows:**
```bash
ipconfig
# Look for "IPv4 Address" under your active network adapter
```

**macOS/Linux:**
```bash
ifconfig
# Look for inet address under your active network interface
```

## Complete MAUI Configuration Example

```csharp
public static class ApiConfig
{
    public static string BaseUrl
    {
        get
        {
#if DEBUG
    #if ANDROID
            // Android emulator
            return "http://10.0.2.2:7071/api";
    #elif IOS
            // iOS simulator
            return "http://localhost:7071/api";
    #elif WINDOWS
            // Windows
            return "http://localhost:7071/api";
    #else
            // Physical devices - replace with your machine's IP
            return "http://192.168.1.84:7071/api";
    #endif
#else
            // Production
            return "https://your-production-api.azurewebsites.net/api";
#endif
        }
    }
}
```

## Azure Function Configuration

The Azure Function must be configured to listen on all network interfaces (not just localhost).

### 1. Update `launchSettings.json`

```json
{
  "profiles": {
    "UserManagement.Client": {
      "commandName": "Project",
      "commandLineArgs": "--port 7071 --host 0.0.0.0",
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**The `--host 0.0.0.0` flag makes the function listen on all network interfaces.**

### 2. Start the Azure Function

```bash
cd src/UserManagement.Client
func start --port 7071 --host 0.0.0.0
```

Or with .NET CLI:
```bash
dotnet run
```

### 3. Verify the Function is Running

You should see output like:
```
Functions:
        AuthFunctions:
                [POST] http://0.0.0.0:7071/api/auth/register
```

## Testing the Connection

### From Windows Host

```bash
curl -X POST http://localhost:7071/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!","confirmPassword":"Password123!","firstName":"John","lastName":"Doe","device":{"deviceId":"test","deviceName":"Test","deviceType":1,"platform":"Windows"}}'
```

### From Android Emulator

```bash
# Using adb shell
adb shell
curl -X POST http://10.0.2.2:7071/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com",...}'
```

### From Physical Device on Same Network

```bash
curl -X POST http://192.168.1.84:7071/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com",...}'
```

## Firewall Configuration

If physical devices still can't connect, check Windows Firewall:

### Allow Inbound Port 7071

**Windows PowerShell (Run as Administrator):**
```powershell
New-NetFirewallRule -DisplayName "Azure Functions Local Dev" `
  -Direction Inbound `
  -LocalPort 7071 `
  -Protocol TCP `
  -Action Allow
```

### Or Use Windows Defender Firewall UI

1. Open "Windows Defender Firewall with Advanced Security"
2. Click "Inbound Rules" → "New Rule"
3. Select "Port" → Next
4. Select "TCP" → Specific local ports: `7071` → Next
5. Select "Allow the connection" → Next
6. Check all profiles → Next
7. Name: "Azure Functions Local Dev" → Finish

## Troubleshooting

### Issue: "Connection Refused"

**Cause:** Azure Function not listening on the right interface

**Solution:**
1. Stop the Azure Function
2. Ensure `launchSettings.json` has `--host 0.0.0.0`
3. Restart the function
4. Verify it shows `http://0.0.0.0:7071` (not `http://localhost:7071`)

### Issue: "No route to host" (Physical Device)

**Cause:** Device not on the same network or firewall blocking

**Solution:**
1. Ensure device is connected to the same Wi-Fi network
2. Check firewall rules (see above)
3. Ping the host from the device to verify connectivity

### Issue: "Network timeout" (Android Emulator)

**Cause:** Wrong address or DNS issues

**Solution:**
1. Use `10.0.2.2` (not `localhost`)
2. Restart the emulator
3. Check emulator network settings

### Issue: CORS Errors

**Cause:** CORS not configured for requests from different origins

**Solution:** The `local.settings.json` already has CORS enabled for all origins:
```json
"Host": {
  "LocalHttpPort": 7071,
  "CORS": "*",
  "CORSCredentials": false
}
```

## Security Note

**Important:** The `--host 0.0.0.0` configuration and `CORS: "*"` settings are for **local development only**.

For production:
- Azure Functions automatically handles proper binding
- Configure CORS to allow only your app's domain
- Use HTTPS with valid certificates
- Never expose development APIs to the public internet

## Example MAUI HttpClient Setup

```csharp
public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        var handler = new HttpClientHandler();

#if DEBUG && ANDROID
        // Allow cleartext (HTTP) traffic on Android
        handler.ServerCertificateCustomValidationCallback =
            (message, cert, chain, errors) => true;
#endif

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/auth/register", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }
}
```

## Android Network Security Configuration

For Android, you need to allow cleartext (HTTP) traffic in debug mode.

**Add to `Platforms/Android/Resources/xml/network_security_config.xml`:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
  <base-config cleartextTrafficPermitted="false" />
  <debug-overrides>
    <trust-anchors>
      <certificates src="system" />
      <certificates src="user" />
    </trust-anchors>
  </debug-overrides>
  <domain-config cleartextTrafficPermitted="true">
    <domain includeSubdomains="true">10.0.2.2</domain>
    <domain includeSubdomains="true">192.168.1.84</domain>
  </domain-config>
</network-security-config>
```

**Reference in `AndroidManifest.xml`:**

```xml
<application
    android:networkSecurityConfig="@xml/network_security_config"
    ...>
</application>
```

## Quick Reference Table

| Platform | URL | Notes |
|----------|-----|-------|
| Android Emulator | `http://10.0.2.2:7071/api` | Special loopback address |
| iOS Simulator | `http://localhost:7071/api` | Shares host network |
| Windows | `http://localhost:7071/api` | Local machine |
| Physical Device | `http://192.168.1.84:7071/api` | Your host IP address |
| Production | `https://your-api.azurewebsites.net/api` | Azure deployment |

---

**Your Host Machine IP Addresses:**
- Primary Network: `192.168.1.84`
- Alternative: `10.11.139.135`

Use the IP address that corresponds to the network your mobile device is connected to.
