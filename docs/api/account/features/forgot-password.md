# Feature: Forgot Password

| Property | Value |
|----------|-------|
| Story | Request password reset via email |
| Endpoints | `POST /api/auth/forgot-password`, `POST /api/auth/reset-password` |
| Controller | [AuthController](../controllers/AuthController.md) |
| Commands | `ForgotPasswordCommand`, `ResetPasswordCommand` |
| Auth required | No |

## Step 1 — Request Reset

```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "trader@example.com"
}
```

### Response — 204 No Content

Always returns 204, even if the email is not registered (prevents account enumeration).

### Flow

1. Look up user by email; exit silently if not found or inactive
2. Invalidate previous reset tokens for the user
3. Create new reset token (1-hour lifetime, stored hashed)
4. Send email via `IEmailService` (currently `LoggingEmailService` logs the URL)

## Step 2 — Reset Password

```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "{token from email link}",
  "newPassword": "NewSecurePass456"
}
```

### Response — 204 No Content

### Flow

1. Hash submitted token and find valid, unused reset token
2. Update user password
3. Mark reset token as used
4. Revoke all refresh tokens

## Errors (reset step)

| Status | Cause |
|--------|-------|
| 400 | Validation failure |
| 401 | Invalid or expired reset token |

## Options / Configuration

| Option | Location | Default |
|--------|----------|---------|
| Reset token lifetime | Hard-coded in handler | 1 hour |
| Reset URL base | `Email:PasswordResetUrlBase` | `https://localhost/reset-password` |
| Email provider | `IEmailService` | Logs to console in dev |

## Production TODO

Replace `LoggingEmailService` with a real email provider (SendGrid, SES, etc.).
