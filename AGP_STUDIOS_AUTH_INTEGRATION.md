# AGP Studios Authentication Integration

## Overview

This document describes the authentication integration between AGP Studios IDE and the ASHAT AI Server through the phpBB3 authentication bridge.

## Problem Statement

AGP Studios IDE was unable to authenticate with the AI Server because of a response format mismatch:
- AGP Studios IDE expected a `token` field in the login response
- The AI Server was only returning a `sessionId` field

## Solution

The authentication endpoints have been updated to support both formats:

### 1. Updated Login/Register Responses

Both `/api/auth/login` and `/api/auth/register` now return:

```json
{
    "success": true,
    "message": "Login successful",
    "token": "<session-token>",       // NEW: AGP Studios IDE compatibility
    "sessionId": "<session-token>",   // Backward compatibility
    "user": {
        "id": 123,
        "username": "testuser",
        "email": "user@example.com",
        "role": "User"
    }
}
```

### 2. New User Info Endpoint

Created `/api/user/me` endpoint to retrieve authenticated user information:

**Request:**
```http
GET /api/user/me
Authorization: Bearer <session-token>
```

**Response:**
```json
{
    "userId": 123,
    "username": "testuser",
    "email": "user@example.com",
    "isAdmin": false
}
```

## Authentication Flow

### AGP Studios IDE → AI Server → phpBB3

1. **User Login in AGP Studios IDE:**
   ```
   AGP Studios IDE → POST /api/auth/login
   {
       "username": "user",
       "password": "pass"
   }
   ```

2. **AI Server forwards to phpBB3:**
   ```
   AI Server → POST http://phpbb-url/api/auth/login
   ```

3. **phpBB3 validates and returns session:**
   ```
   phpBB3 → Response with sessionId
   ```

4. **AI Server returns to AGP Studios IDE:**
   ```
   AI Server → Response with both token and sessionId
   {
       "success": true,
       "token": "<session-id>",
       "sessionId": "<session-id>",
       "user": { ... }
   }
   ```

5. **AGP Studios IDE gets user info:**
   ```
   AGP Studios IDE → GET /api/user/me
   Authorization: Bearer <token>
   ```

6. **AI Server validates token with phpBB3:**
   ```
   AI Server → POST http://phpbb-url/api/auth/validate
   {
       "sessionId": "<token>"
   }
   ```

7. **Route user to appropriate screen:**
   - If `isAdmin: true` → Admin Console
   - If `isAdmin: false` → Game Library

## Configuration

### AI Server Configuration

In `ASHATAIServer/appsettings.json`:

```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://your-phpbb-url.com",
    "RequireAuthentication": true
  }
}
```

### AGP Studios IDE Configuration

The IDE reads server URL from `config.json`:

```json
{
  "ServerUrl": "http://localhost:8088"
}
```

## API Endpoints

### Authentication Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/login` | POST | User login with username/password |
| `/api/auth/register` | POST | New user registration |
| `/api/auth/validate` | POST | Validate session token |

### User Endpoints

| Endpoint | Method | Description | Auth Required |
|----------|--------|-------------|---------------|
| `/api/user/me` | GET | Get current user info | Yes (Bearer token) |

## Testing

Run the AGP Studios authentication test:

```bash
./test_agp_studios_auth.sh
```

This test verifies:
- ✅ ASHATAIServer builds successfully
- ✅ Server starts and listens on port 8088
- ✅ Login endpoint responds with correct structure
- ✅ Register endpoint responds with correct structure
- ✅ `/api/user/me` endpoint exists and validates tokens
- ✅ Response formats match AGP Studios IDE expectations

## Backward Compatibility

The changes maintain full backward compatibility:

- The `sessionId` field is still included in all responses
- Existing phpBB3 extension endpoints remain unchanged
- Other services using `sessionId` will continue to work

## Security Considerations

1. **Token Validation:** All tokens are validated through phpBB3's session management
2. **HTTPS Required:** Use HTTPS in production to protect credentials
3. **Session Expiry:** Sessions expire based on phpBB3 configuration
4. **Bearer Token:** Standard OAuth2 Bearer token pattern for API authentication

## Code Changes

### Files Modified

1. **ASHATAIServer/Controllers/AuthController.cs**
   - Added `token` field to login response
   - Added `token` field to register response
   - Maintained `sessionId` for backward compatibility

2. **ASHATAIServer/Controllers/UserController.cs** (NEW)
   - Created `/api/user/me` endpoint
   - Bearer token validation
   - User info retrieval

3. **test_agp_studios_auth.sh** (NEW)
   - Integration test for authentication flow

## Troubleshooting

### AGP Studios Cannot Connect

1. **Check Server is Running:**
   ```bash
   curl http://localhost:8088/api/auth/login
   ```

2. **Verify Configuration:**
   - Check `appsettings.json` has correct PhpBBBaseUrl
   - Verify phpBB3 extension is installed and enabled

3. **Check Logs:**
   - AI Server logs are in console output
   - phpBB3 logs are in phpBB error log

### Authentication Fails

1. **Verify phpBB3 is Running:**
   ```bash
   curl http://your-phpbb-url.com/api/auth/login
   ```

2. **Check Credentials:**
   - Username must exist in phpBB3
   - Password must be correct
   - User account must be active

3. **Check phpBB3 Extension:**
   - Extension must be enabled in ACP
   - Extension routes must be configured
   - Database connection must be working

### Common Error Messages

**"Authentication service is temporarily unavailable. Please try again later."**
- Cause: phpBB3 backend is returning HTTP 500+ errors
- Solution: Check phpBB3 server logs, ensure phpBB3 is running and accessible
- Check: `appsettings.json` has correct `PhpBBBaseUrl`

**"Unable to connect to authentication service. Please check that phpBB is running and accessible."**
- Cause: Network connection to phpBB3 failed
- Solution: Verify phpBB3 URL is correct and reachable from AI Server
- Test: `curl http://your-phpbb-url/api/auth/login` from AI Server host

**"Authentication service configuration error. The service may not be properly configured."**
- Cause: phpBB3 is returning HTML (404/403 pages) instead of JSON API responses
- Solution: Verify phpBB3 authentication bridge extension is installed and enabled
- Check: Extension routes are properly registered in phpBB3

**"Invalid username or password"**
- Cause: Login credentials are incorrect
- Solution: Verify username and password in phpBB3 database

## Improvements (Latest Release)

### Enhanced Error Handling (v1.1)

The authentication service now provides clear, user-friendly error messages when phpBB3 is unavailable or misconfigured:

1. **Intelligent Error Detection:**
   - Checks Content-Type header before parsing responses
   - Detects HTML error pages vs JSON responses
   - Distinguishes between connection errors and server errors

2. **Security Improvements:**
   - Sanitizes user input in log messages to prevent log injection attacks
   - Removes control characters from logged usernames

3. **Better Logging:**
   - Logs phpBB connection attempts with full URL
   - Logs HTTP status codes for troubleshooting
   - Categorizes errors (connection, parsing, server error)

## Next Steps

With authentication now working, AGP Studios IDE users can:

1. **Login** with their phpBB3 credentials
2. **Access** the appropriate interface based on their role:
   - Admins → AI Console for code creation
   - Members → Game Library for downloading/playing games
3. **Publish** games (admin only)
4. **Download** and play published games

## Additional Resources

- [phpBB3 Extension README](../phpbb3_extension/ashatos/authbridge/README.md)
- [Integration Guide](../INTEGRATION_GUIDE.md)
- [AGP Studios IDE README](../AGP_Studios/README.md)
