# ASHATOS Authentication Bridge Extension for phpBB3

This extension provides REST API endpoints for authentication, enabling the ASHATOS AI Game Server to integrate with phpBB3 for user management and authentication.

## Features

- **RESTful API Endpoints**: JSON-based authentication API
- **User Registration**: Create new user accounts via API
- **User Login**: Authenticate users and receive session tokens
- **Session Validation**: Validate existing session tokens
- **User Logout**: End user sessions
- **CORS Support**: Cross-Origin Resource Sharing for external integrations
- **Secure Authentication**: Uses phpBB's native password hashing and session management

## Requirements

- phpBB 3.2.0 or higher
- PHP 7.4 or higher

## Installation

1. Download the extension
2. Copy the `ashatos` folder to `phpBB/ext/` directory
3. Navigate to "ACP" > "Customise" > "Extensions" in your phpBB admin panel
4. Enable the "ASHATOS Authentication Bridge" extension

## API Endpoints

### Base URL
All endpoints are relative to your phpBB installation URL.

### 1. Login
**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
    "username": "john_doe",
    "password": "secure_password"
}
```

**Success Response** (200 OK):
```json
{
    "success": true,
    "message": "Login successful",
    "sessionId": "abc123...",
    "user": {
        "id": 1,
        "username": "john_doe",
        "email": "john@example.com",
        "role": "User"
    }
}
```

**Error Response** (401 Unauthorized):
```json
{
    "success": false,
    "message": "Invalid username or password"
}
```

### 2. Register
**Endpoint**: `POST /api/auth/register`

**Request Body**:
```json
{
    "username": "new_user",
    "email": "user@example.com",
    "password": "secure_password"
}
```

**Success Response** (201 Created):
```json
{
    "success": true,
    "message": "Registration successful",
    "sessionId": "xyz789...",
    "user": {
        "id": 2,
        "username": "new_user",
        "email": "user@example.com",
        "role": "User"
    }
}
```

**Error Responses**:
- 400 Bad Request: Missing or invalid fields
- 409 Conflict: Username or email already exists

### 3. Validate Session
**Endpoint**: `POST /api/auth/validate`

**Request Body**:
```json
{
    "sessionId": "abc123..."
}
```

**Success Response** (200 OK):
```json
{
    "success": true,
    "user": {
        "id": 1,
        "username": "john_doe",
        "email": "john@example.com",
        "role": "User"
    }
}
```

**Error Response** (401 Unauthorized):
```json
{
    "success": false,
    "message": "Invalid or expired session"
}
```

### 4. Logout
**Endpoint**: `POST /api/auth/logout`

**Request Body**:
```json
{
    "sessionId": "abc123..."
}
```

**Success Response** (200 OK):
```json
{
    "success": true,
    "message": "Logout successful"
}
```

## Integration with ASHATOS

### Configuration
Update your ASHATAIServer `appsettings.json`:

```json
{
  "Authentication": {
    "PhpBBBaseUrl": "http://your-phpbb-url.com",
    "RequireAuthentication": true
  }
}
```

### Example Usage in C#

```csharp
// Login
var loginResponse = await httpClient.PostAsJsonAsync(
    "http://your-phpbb-url.com/api/auth/login",
    new { username = "john", password = "pass123" }
);

if (loginResponse.IsSuccessStatusCode)
{
    var result = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
    string sessionId = result.SessionId;
    // Use sessionId for subsequent requests
}
```

## Security Considerations

1. **HTTPS**: Always use HTTPS in production to protect credentials
2. **Rate Limiting**: Consider implementing rate limiting to prevent brute force attacks
3. **CORS**: The extension allows all origins by default; restrict this in production
4. **Session Expiry**: Sessions use phpBB's configured session length
5. **Password Requirements**: Minimum 6 characters; consider stronger requirements

## Customization

### Adjusting Password Requirements
Edit `api_controller.php`, method `register()`:

```php
if (strlen($password) < 8) { // Change from 6 to 8
    return $this->json_error('Password must be at least 8 characters', Response::HTTP_BAD_REQUEST);
}
```

### Modifying User Roles
Edit `api_controller.php`, method `get_user_role()`:

```php
protected function get_user_role($user_row)
{
    // Add custom role logic here
    if ($user_row['user_type'] == USER_FOUNDER) {
        return 'Admin';
    }
    // Add moderator check, etc.
    return 'User';
}
```

### Restricting CORS
Edit `api_controller.php`, method `json_response()`:

```php
return new JsonResponse($data, $status, [
    'Content-Type' => 'application/json',
    'Access-Control-Allow-Origin' => 'https://your-ashat-server.com', // Restrict to specific domain
    'Access-Control-Allow-Methods' => 'POST',
    'Access-Control-Allow-Headers' => 'Content-Type',
]);
```

## Troubleshooting

### Extension Not Appearing in ACP
- Verify the folder structure: `phpBB/ext/ashatos/authbridge/`
- Clear phpBB cache: `php bin/phpbbcli.php cache:purge`

### API Returns 404
- Ensure the extension is enabled in ACP
- Check that routing is properly configured
- Verify phpBB's URL rewriting is working

### Authentication Fails
- Check phpBB error logs
- Verify database connection
- Ensure user exists and is active in phpBB

### CORS Issues
- Verify browser console for CORS errors
- Adjust CORS headers in `json_response()` method
- Consider adding OPTIONS method handler for preflight requests

## Development

### Running Tests
```bash
# Navigate to phpBB root
cd /path/to/phpbb

# Run phpBB tests (if available)
php bin/phpunit.phar
```

### Debugging
Enable debug mode in phpBB's config.php:
```php
@define('DEBUG', true);
@define('DEBUG_CONTAINER', true);
```

## License

GNU General Public License v2.0 - see LICENSE file

## Support

For issues and feature requests, please visit:
https://github.com/buffbot88/ASHATOS/issues

## Changelog

### Version 1.0.0 (2024-11-12)
- Initial release
- Login, register, validate, and logout endpoints
- Session management
- Role-based user information
- CORS support
