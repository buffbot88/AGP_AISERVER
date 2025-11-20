# Security Documentation - ASHATAIServer

This document outlines the security features, best practices, and configuration options for ASHATAIServer.

## Table of Contents
- [Security Features](#security-features)
- [Password Security](#password-security)
- [API Key Authentication](#api-key-authentication)
- [Rate Limiting](#rate-limiting)
- [HTTPS Configuration](#https-configuration)
- [Best Practices](#best-practices)
- [Threat Model](#threat-model)
- [Security Updates](#security-updates)

---

## Security Features

ASHATAIServer implements multiple layers of security to protect against common attack vectors:

### ✅ Implemented Security Features

1. **Strong Password Hashing (Argon2id)**
   - Industry-standard password hashing algorithm
   - Per-user cryptographic salts (32 bytes)
   - Memory-hard algorithm resistant to GPU/ASIC attacks
   - Configurable parameters for future-proofing

2. **API Key Authentication**
   - Secure API key generation (256-bit entropy)
   - SHA256 hashing for key storage
   - Scope-based access control
   - Expiration and revocation support
   - Per-key usage tracking

3. **Rate Limiting**
   - Per-IP rate limiting
   - Per-API-key rate limiting
   - Configurable limits (requests per minute/hour)
   - Automatic cleanup of expired entries
   - Standard HTTP 429 responses with Retry-After headers

4. **Session Management**
   - Secure session token generation
   - 7-day expiration by default
   - Automatic cleanup of expired sessions
   - Session validation on each request

5. **Input Validation & Sanitization**
   - Log injection prevention
   - SQL injection prevention via parameterized queries
   - Path traversal protection in file operations
   - Content type validation

6. **HTTPS/TLS Support**
   - Optional HTTPS configuration
   - Certificate-based encryption
   - Configurable ports and certificate paths

---

## Password Security

### Argon2id Configuration

ASHATAIServer uses Argon2id, the winner of the Password Hashing Competition, for password storage.

**Parameters:**
```
Algorithm: Argon2id (hybrid mode)
Salt: 32 bytes (256 bits) - unique per user
Hash Output: 32 bytes (256 bits)
Memory: 64 MB (65536 KB)
Iterations: 4
Parallelism: 8 threads
```

**Why Argon2id?**
- **Memory-hard:** Resistant to GPU and ASIC attacks
- **Side-channel resistant:** Protects against timing attacks
- **Tunable:** Can be adjusted as hardware improves
- **Hybrid mode:** Combines data-dependent and data-independent memory access

### Password Requirements

- Minimum length: 6 characters (recommend 12+ for production)
- No maximum length
- All character types accepted
- Hashed with unique per-user salt

### Upgrading from SHA256

Existing databases with SHA256 hashes are incompatible with the new Argon2id implementation. Users will need to reset their passwords or re-register. The system detects old hash formats and rejects them.

**Migration Note:** If migrating from an existing SHA256-based database, implement a migration script or require users to reset passwords.

---

## API Key Authentication

### Overview

API keys provide an alternative authentication method to session tokens, ideal for:
- Service-to-service communication
- Automated scripts and tools
- Long-running integrations
- Distributed systems

### Key Format

```
agp_live_<64 hexadecimal characters>
```

Example: `agp_live_a1b2c3d4e5f6789012345678901234567890abcdefabcdefabcdefabcdef1234`

### Creating API Keys

**Endpoint:** `POST /api/admin/keys/create`

**Requirements:**
- Admin session token (Bearer authentication)
- Request body with key details

**Example Request:**
```bash
curl -X POST http://localhost:7077/api/admin/keys/create \
  -H "Authorization: Bearer <admin-session-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Production API Key",
    "assignToUserId": 5,
    "expiresAt": "2025-12-31T23:59:59Z",
    "scopes": "ai:process,ai:generate"
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "API key created successfully",
  "apiKey": "agp_live_...",
  "warning": "Save this API key securely. It will not be shown again."
}
```

**⚠️ Important:** The API key is only shown once during creation. Store it securely.

### Using API Keys

**Method 1: Authorization Header (Recommended)**
```bash
curl -H "Authorization: Bearer agp_live_..." http://localhost:7077/api/ai/process
```

**Method 2: X-API-Key Header**
```bash
curl -H "X-API-Key: agp_live_..." http://localhost:7077/api/ai/process
```

### Managing API Keys

**List Keys:**
```bash
GET /api/admin/keys/list
GET /api/admin/keys/list?userId=5
GET /api/admin/keys/list?includeRevoked=true
```

**Revoke Key:**
```bash
POST /api/admin/keys/revoke/{keyId}
```

### Configuring Protected Endpoints

In `appsettings.json`:

```json
{
  "ApiKey": {
    "ProtectedPaths": [
      "/api/ai/process",
      "/api/ai/generate-project"
    ],
    "ExemptPaths": [
      "/api/auth/login",
      "/api/auth/register",
      "/api/ai/health"
    ]
  }
}
```

**Default Behavior:**
- If `ProtectedPaths` is empty: No endpoints require API keys (backward compatible)
- If `ProtectedPaths` is populated: Listed endpoints require API keys
- `ExemptPaths` are always accessible without API keys

### Key Storage

- API keys are hashed using SHA256 before storage
- Only the hash is stored in the database
- Original keys are never stored
- Keys are validated via constant-time comparison to prevent timing attacks

---

## Rate Limiting

### Overview

Rate limiting prevents abuse by limiting the number of requests from a single source within a time window.

### Configuration

**In `appsettings.json`:**
```json
{
  "RateLimit": {
    "Enabled": true,
    "RequestsPerMinute": 60,
    "RequestsPerHour": 1000
  }
}
```

### Default Limits

- **Per Minute:** 60 requests
- **Per Hour:** 1000 requests

### Rate Limit Response

When rate limit is exceeded:

**Status Code:** `429 Too Many Requests`

**Response Body:**
```json
{
  "success": false,
  "message": "Rate limit exceeded. Maximum 60 requests per minute allowed.",
  "retryAfter": "60 seconds"
}
```

**Response Headers:**
```
HTTP/1.1 429 Too Many Requests
Retry-After: 60
X-RateLimit-Limit-Minute: 60
X-RateLimit-Limit-Hour: 1000
X-RateLimit-Remaining-Minute: 0
X-RateLimit-Remaining-Hour: 245
```

### Rate Limit Headers

All responses include rate limit information:

- `X-RateLimit-Limit-Minute`: Maximum requests per minute
- `X-RateLimit-Limit-Hour`: Maximum requests per hour
- `X-RateLimit-Remaining-Minute`: Remaining requests this minute
- `X-RateLimit-Remaining-Hour`: Remaining requests this hour
- `Retry-After`: Seconds to wait before retrying (only on 429)

### Rate Limit Identifiers

Requests are tracked by:
1. **API Key** (if present) - most specific
2. **User ID** (if authenticated via session)
3. **IP Address** (fallback)

### Adjusting Limits

For production deployments, adjust limits based on:
- Expected traffic volume
- Server capacity
- User tier (basic vs. premium)
- Endpoint sensitivity

**Example - Higher Limits for Production:**
```json
{
  "RateLimit": {
    "Enabled": true,
    "RequestsPerMinute": 120,
    "RequestsPerHour": 10000
  }
}
```

---

## HTTPS Configuration

### Enabling HTTPS

**In `appsettings.json`:**
```json
{
  "Https": {
    "Enabled": true,
    "Port": 7443,
    "CertificatePath": "/path/to/certificate.pfx",
    "CertificatePassword": "your-cert-password"
  }
}
```

### Certificate Requirements

- **Format:** PKCS#12 (.pfx or .p12)
- **Contents:** Certificate + Private Key
- **Permissions:** Readable by the application user

### Generating a Self-Signed Certificate (Development)

**Using OpenSSL:**
```bash
# Generate private key
openssl genrsa -out server.key 2048

# Generate certificate signing request
openssl req -new -key server.key -out server.csr

# Generate self-signed certificate
openssl x509 -req -days 365 -in server.csr -signkey server.key -out server.crt

# Convert to PFX format
openssl pkcs12 -export -out server.pfx -inkey server.key -in server.crt
```

**Using .NET:**
```bash
dotnet dev-certs https -ep ./server.pfx -p YourPassword123
```

### Getting a Production Certificate

**Recommended Certificate Authorities:**
1. **Let's Encrypt** (Free, automated)
2. **DigiCert**
3. **GlobalSign**
4. **Sectigo**

**Using Certbot (Let's Encrypt):**
```bash
# Install Certbot
apt-get install certbot

# Get certificate
certbot certonly --standalone -d yourdomain.com

# Convert to PFX
openssl pkcs12 -export \
  -out /etc/letsencrypt/live/yourdomain.com/cert.pfx \
  -inkey /etc/letsencrypt/live/yourdomain.com/privkey.pem \
  -in /etc/letsencrypt/live/yourdomain.com/cert.pem \
  -certfile /etc/letsencrypt/live/yourdomain.com/chain.pem
```

### Environment Variable Configuration

For production, use environment variables instead of `appsettings.json`:

```bash
export Https__Enabled=true
export Https__Port=7443
export Https__CertificatePath=/path/to/cert.pfx
export Https__CertificatePassword=SecurePassword123

dotnet ASHATAIServer.dll
```

### HTTPS Best Practices

1. **Always use HTTPS in production**
2. **Disable HTTP in production** (or redirect to HTTPS)
3. **Use strong cipher suites**
4. **Keep certificates updated**
5. **Use HSTS headers** for additional security
6. **Store certificate passwords securely** (Azure Key Vault, AWS Secrets Manager, etc.)

---

## Best Practices

### General Security

1. **Change Default Ports:** Don't use default ports in production
2. **Use Firewall Rules:** Restrict access to trusted networks
3. **Enable All Security Features:** Don't disable rate limiting or API keys
4. **Regular Updates:** Keep dependencies updated
5. **Monitoring:** Set up logging and alerting
6. **Least Privilege:** Run with minimal required permissions

### Password Policy

**Recommended Settings:**
```json
{
  "PasswordPolicy": {
    "MinimumLength": 12,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigit": true,
    "RequireSpecialChar": true
  }
}
```

**Note:** Current implementation has a 6-character minimum. Update `UserDatabaseService.cs` to enforce stronger policies.

### API Key Management

1. **Rotate Keys Regularly:** Revoke and recreate keys every 90 days
2. **Use Scopes:** Limit key permissions to minimum required
3. **Monitor Usage:** Track `LastUsedAt` for suspicious activity
4. **Revoke Unused Keys:** Clean up keys that haven't been used in 90 days
5. **Separate Keys by Environment:** Different keys for dev/staging/prod

### Rate Limiting Strategy

1. **Tune for Your Use Case:** Monitor actual traffic patterns
2. **Differentiate by Endpoint:** More restrictive on expensive operations
3. **Consider User Tiers:** Different limits for free vs. paid users
4. **Implement Graceful Degradation:** Return cached results when limited
5. **Log Rate Limit Violations:** Detect potential abuse

### Session Management

1. **Short Expiration for Sensitive Operations:** 15-30 minutes
2. **Longer Expiration for General Use:** 7-30 days
3. **Implement "Remember Me" Carefully:** Use separate longer-lived tokens
4. **Invalidate on Password Change:** Expire all sessions when password changes
5. **Track Active Sessions:** Allow users to view and revoke sessions

---

## Threat Model

### Threats Mitigated

| Threat | Mitigation |
|--------|-----------|
| **Brute Force Attacks** | Argon2id (memory-hard), Rate Limiting |
| **Rainbow Tables** | Unique per-user salts (32 bytes) |
| **SQL Injection** | Parameterized queries throughout |
| **Log Injection** | Input sanitization for all logged data |
| **Session Hijacking** | Secure session generation, HTTPS optional |
| **API Abuse** | Rate limiting per IP/key/user |
| **Timing Attacks** | Constant-time hash comparison |
| **Path Traversal** | Path sanitization in file operations |
| **DDoS** | Rate limiting, connection limits |

### Potential Vulnerabilities

⚠️ **Areas Requiring Additional Hardening for High-Security Deployments:**

1. **No Password Complexity Rules:** Only enforces 6-char minimum
2. **No Account Lockout:** Unlimited login attempts (mitigated by rate limiting)
3. **No Multi-Factor Authentication:** Only single-factor auth
4. **No Session IP Binding:** Sessions not tied to IP addresses
5. **No CSRF Protection:** Required for browser-based clients
6. **No Content Security Policy:** Required for web UI
7. **No Certificate Pinning:** For mobile/desktop clients
8. **No Audit Logging:** Limited security event logging

---

## Security Updates

### Reporting Security Issues

**Do not open public issues for security vulnerabilities.**

Contact: security@agpstudios.com (or create a private GitHub security advisory)

### Security Update Process

1. Report received and acknowledged within 24 hours
2. Vulnerability assessed and severity determined
3. Fix developed and tested
4. Security advisory published
5. Patch released
6. Users notified

### Staying Updated

- Watch the GitHub repository for security advisories
- Subscribe to release notifications
- Monitor the CHANGELOG for security fixes
- Follow @AGPStudios on social media

### Dependency Security

ASHATAIServer dependencies are regularly scanned for vulnerabilities:

```bash
# Check for vulnerable packages
dotnet list package --vulnerable

# Update all packages
dotnet add package <PackageName>
```

**Current Dependencies:**
- `Microsoft.AspNetCore.OpenApi` - Web framework (Microsoft)
- `Microsoft.Data.Sqlite` - Database (Microsoft)
- `Konscious.Security.Cryptography.Argon2` - Password hashing
- `AspNetCoreRateLimit` - Rate limiting (deprecated - consider migrating)

⚠️ **Note:** `AspNetCoreRateLimit` is no longer actively maintained. Consider migrating to built-in .NET rate limiting in a future update.

---

## Compliance & Standards

### OWASP Top 10 Coverage

| Risk | Status | Notes |
|------|--------|-------|
| A01 Broken Access Control | ✅ Addressed | API keys, sessions, admin roles |
| A02 Cryptographic Failures | ✅ Addressed | Argon2id, HTTPS, secure storage |
| A03 Injection | ✅ Addressed | Parameterized queries, sanitization |
| A04 Insecure Design | ⚠️ Partial | MFA not implemented |
| A05 Security Misconfiguration | ⚠️ Partial | Requires proper deployment |
| A06 Vulnerable Components | ✅ Addressed | Regular dependency updates |
| A07 Authentication Failures | ✅ Addressed | Strong hashing, rate limiting |
| A08 Data Integrity Failures | ✅ Addressed | Input validation, checksums |
| A09 Logging Failures | ⚠️ Partial | Basic logging, no SIEM integration |
| A10 Server-Side Request Forgery | N/A | No user-controlled URLs |

### NIST Guidelines

Password storage follows NIST SP 800-63B guidelines:
- Memorized secret (password) hashing with salt
- Minimum entropy requirements
- Rate limiting for authentication attempts

---

## FAQ

**Q: Should I disable rate limiting for testing?**
A: No. Test with rate limiting enabled to ensure your application handles 429 responses correctly. Increase limits if needed.

**Q: Can I use API keys instead of session tokens everywhere?**
A: Yes. API keys are suitable for service-to-service communication. Use sessions for user-facing applications.

**Q: How do I create the first admin user?**
A: Register a user normally, then manually update the `Role` field in the database to "Admin".

**Q: Is it safe to run without HTTPS in production?**
A: No. Always use HTTPS in production. HTTP transmits credentials in plain text.

**Q: How do I migrate from SHA256 to Argon2id?**
A: Existing SHA256 hashes are incompatible. Require users to reset passwords or implement a dual-hash verification system that upgrades on next login.

**Q: What's the difference between API key scopes?**
A: Scopes are currently stored as strings for future use. Implement scope validation in middleware for fine-grained access control.

**Q: Can I run multiple instances behind a load balancer?**
A: Yes. Rate limiting is per-instance. For distributed rate limiting, integrate Redis or another shared state store.

---

## Additional Resources

- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [Argon2 RFC](https://tools.ietf.org/html/rfc9106)
- [NIST Password Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)
- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)

---

**Last Updated:** 2025-11-20
**Version:** 1.0.0 (Phase 3 Implementation)
