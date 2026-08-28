# Feature: Change Password

| Property | Value |
|----------|-------|
| Story | Authenticated user changes their password |
| Endpoint | `PUT /api/account/change-password` |
| Controller | [AccountController](../controllers/AccountController.md) |
| Command | `ChangeUserPasswordCommand` |
| Auth required | Yes — Bearer JWT |

## Request

```http
PUT /api/account/change-password
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "currentPassword": "OldSecurePass123",
  "newPassword": "NewSecurePass456"
}
```

## Validation

| Field | Rules |
|-------|-------|
| `currentPassword` | Required, max 128 chars |
| `newPassword` | Required, 8–128 chars |

## Response — 204 No Content

## Errors

| Status | Cause |
|--------|-------|
| 400 | Validation failure or domain rule violation |
| 401 | Wrong current password or missing JWT |
| 404 | User not found |

## Application Flow

1. Load user by ID from JWT
2. Verify `currentPassword` against stored hash
3. Hash and save new password via `User.ChangePassword`
4. Revoke all refresh tokens for the user (forces re-login on other devices)

## Security Notes

- Current password must be verified before change
- All active sessions are invalidated after password change
