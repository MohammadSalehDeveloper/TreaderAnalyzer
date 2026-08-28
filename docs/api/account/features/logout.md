# Feature: Logout

| Property | Value |
|----------|-------|
| Story | Sign out and invalidate refresh token |
| Endpoint | `POST /api/auth/logout` |
| Controller | [AuthController](../controllers/AuthController.md) |
| Command | `LogoutCommand` |
| Auth required | Yes — Bearer JWT |

## Request

```http
POST /api/auth/logout
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "refreshToken": "{refreshToken from login response}"
}
```

## Response — 204 No Content

## Errors

| Status | Cause |
|--------|-------|
| 401 | Missing/invalid JWT, or refresh token not found / belongs to another user |

## Application Flow

1. Resolve user ID from JWT
2. Hash submitted refresh token
3. Find active refresh token matching hash and user
4. Call `RefreshToken.Revoke()`

## Options

| Option | Notes |
|--------|-------|
| Access token | Remains valid until expiry (stateless JWT). Client should discard it locally. |
| Refresh token | Revoked server-side immediately |

## Client Guidance

- Clear stored access and refresh tokens after successful logout
- Do not reuse the revoked refresh token
