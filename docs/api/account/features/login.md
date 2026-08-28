# Feature: Login

| Property | Value |
|----------|-------|
| Story | Sign in with email/user name and password |
| Endpoint | `POST /api/auth/login` |
| Controller | [AuthController](../controllers/AuthController.md) |
| Command | `LoginCommand` |
| Auth required | No |

## Request

```http
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUserName": "trader@example.com",
  "password": "SecurePass123"
}
```

`emailOrUserName` accepts either email (contains `@`) or user name.

## Response — 200 OK

Returns access + refresh tokens and the user profile (`LoginResponseDto`).

## Errors

| Status | Cause |
|--------|-------|
| 400 | Validation failure (empty fields) |
| 401 | Invalid credentials or inactive account |

## Application Flow

1. Look up user by email or user name
2. Verify password with `IPasswordHasher.Verify`
3. Call `User.RecordLogin()` (requires active status)
4. Create refresh token (7-day lifetime, stored hashed)
5. Issue JWT access token (default 15 minutes)

## Options / Configuration

| Option | Location | Default |
|--------|----------|---------|
| Access token lifetime | `Jwt:AccessTokenLifetimeMinutes` | 15 |
| Refresh token lifetime | Hard-coded in handler | 7 days |
| JWT signing key | `Jwt:SigningKey` | Dev key in appsettings |

## Security Notes

- Password is never logged or returned
- Same error message for unknown user vs wrong password (prevents enumeration)
- Refresh token plain text is returned once; only SHA-256 hash is stored
