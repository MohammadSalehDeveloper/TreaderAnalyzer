# Feature: Admin users

| Property | Value |
|----------|-------|
| Story | An admin lists users and activates or suspends them |
| Endpoints | `GET /api/admin/users`, `POST /api/admin/users/{id}/activate`, `POST /api/admin/users/{id}/suspend` |
| Controller | [AdminUsersController](../controllers/AdminUsersController.md) |
| Query / commands | `GetUsersQuery`, `ActivateUserCommand`, `SuspendUserCommand` |
| Auth required | Yes — Bearer JWT with role `Admin` |

The trader panel does not call these routes. It uses the existing account endpoints (`GET /api/account/me`, `PUT /api/account/profile`, `PUT /api/account/change-password`).

## List users

```http
GET /api/admin/users?role=Trader&status=Pending
Authorization: Bearer {accessToken}
```

`role` and `status` are optional. Omit both to return every user the repository still exposes (soft-deleted users stay hidden).

### Response — 200 OK

`UserDto` array. Enum values are JSON numbers (`Admin` = 1, `Trader` = 2, `Pending` = 0, `Active` = 1, `Suspended` = 2, `Closed` = 3).

## Activate

```http
POST /api/admin/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/activate
Authorization: Bearer {accessToken}
```

No body. `204 No Content` when the user is pending or suspended.

## Suspend

```http
POST /api/admin/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/suspend
Authorization: Bearer {accessToken}
```

No body. `204 No Content` when the user is active.

## Errors

| Status | Cause |
|--------|-------|
| 400 | Closed user cannot be activated, or a user who is not active cannot be suspended |
| 401 | Missing or invalid JWT |
| 403 | Signed-in user is not an admin |
| 404 | User id does not exist |

## Application flow

1. JWT role claim must be `Admin`
2. List loads users through `IUserRepository.ListAsync` and maps them to `UserDto`
3. Activate loads the user and calls `User.Activate()`
4. Suspend loads the user and calls `User.Suspend()`
5. Status changes are saved through `IUnitOfWork`

## Security notes

- Traders cannot call these routes
- The admin panel hides Suspend on the signed-in admin's own row so they do not lock themselves out from the screen. The API still accepts that call
- Status rules stay on `User`; the controller only dispatches the existing commands
