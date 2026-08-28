# AccountController

| Property | Value |
|----------|-------|
| File | `src/API/API.Account/Controllers/AccountController.cs` |
| Route | `/api/account` |
| Authorization | **All endpoints require Bearer JWT** |

## Endpoints

| Method | Route | Feature doc |
|--------|-------|-------------|
| `GET` | `/me` | Get current user profile |
| `PUT` | `/profile` | [edit-profile.md](../features/edit-profile.md) |
| `PUT` | `/change-password` | [change-password.md](../features/change-password.md) |
| `DELETE` | `/` | [delete-account.md](../features/delete-account.md) |

## User Identity

The authenticated user ID is resolved from JWT claims (`sub` / `NameIdentifier`) via `ClaimsPrincipal.GetUserId()` in `Extensions/AuthenticationExtensions.cs`.

## Request Models

- `UpdateProfileRequest` — `FirstName`, `LastName`, optional `PhoneNumber`, `DisplayName`, `TimeZoneId`
- `ChangePasswordRequest` — `CurrentPassword`, `NewPassword`

## CQRS Commands Used

| Endpoint | Command |
|----------|---------|
| `GET /me` | `GetUserByIdQuery` |
| `PUT /profile` | `UpdateUserProfileCommand` |
| `PUT /change-password` | `ChangeUserPasswordCommand` |
| `DELETE /` | `SoftDeleteUserCommand` |

## Responses

| Endpoint | Success response |
|----------|------------------|
| `GET /me` | `200` + `UserDto` |
| `PUT /profile` | `200` + updated `UserDto` |
| `PUT /change-password` | `204 No Content` |
| `DELETE /` | `204 No Content` |
