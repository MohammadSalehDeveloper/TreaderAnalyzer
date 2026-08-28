# AuthController

| Property | Value |
|----------|-------|
| File | `src/API/API.Account/Controllers/AuthController.cs` |
| Route | `/api/auth` |
| Authorization | Mixed — login/forgot/reset/google are anonymous; logout requires JWT |

## Endpoints

| Method | Route | Auth | Feature doc |
|--------|-------|------|-------------|
| `POST` | `/login` | Anonymous | [login.md](../features/login.md) |
| `POST` | `/logout` | Bearer JWT | [logout.md](../features/logout.md) |
| `POST` | `/forgot-password` | Anonymous | [forgot-password.md](../features/forgot-password.md) |
| `POST` | `/reset-password` | Anonymous | [forgot-password.md](../features/forgot-password.md) |
| `POST` | `/google` | Anonymous | [google-login.md](../features/google-login.md) |

## Dependencies

- `IMediator` — dispatches CQRS commands in `Core.Application.Auth.Commands.*`

## Request/Response Models

Defined as nested records on the controller:

- `LoginRequest` — `EmailOrUserName`, `Password`
- `LogoutRequest` — `RefreshToken`
- `ForgotPasswordRequest` — `Email`
- `ResetPasswordRequest` — `Token`, `NewPassword`
- `GoogleLoginRequest` — `IdToken`

Successful login and Google login return `LoginResponseDto`:

```json
{
  "tokens": {
    "accessToken": "...",
    "refreshToken": "...",
    "accessTokenExpiresAtUtc": "2026-08-11T22:00:00Z",
    "refreshTokenExpiresAtUtc": "2026-08-18T21:45:00Z"
  },
  "user": { "...UserDto fields..." }
}
```

## Error Handling

Errors are returned as RFC 7807-style JSON via `ExceptionHandlingMiddleware`:

| Status | When |
|--------|------|
| 400 | Validation failure |
| 401 | Invalid credentials, token, or Google ID token |
| 404 | User not found (internal flows) |
| 409 | Conflict |
