# AdminUsersController

| Property | Value |
|----------|-------|
| File | `src/API/API.Account/Controllers/AdminUsersController.cs` |
| Route | `/api/admin/users` |
| Authorization | Bearer JWT with role `Admin`, except `POST /` while no admin exists |

## Endpoints

| Method | Route | Feature doc |
|--------|-------|-------------|
| `POST` | `/` | [create-admin.md](../features/create-admin.md) |
| `GET` | `/` | [admin-users.md](../features/admin-users.md) |
| `POST` | `/{id}/activate` | [admin-users.md](../features/admin-users.md) |
| `POST` | `/{id}/suspend` | [admin-users.md](../features/admin-users.md) |

## Query

`GET /` accepts optional `role` (`Admin` or `Trader`) and `status` (`Pending`, `Active`, `Suspended`, `Closed`).

## CQRS

| Endpoint | Request |
|----------|---------|
| `POST /` | `CreateAdminCommand` |
| `GET /` | `GetUsersQuery` |
| `POST /{id}/activate` | `ActivateUserCommand` |
| `POST /{id}/suspend` | `SuspendUserCommand` |

## Responses

| Endpoint | Success |
|----------|---------|
| `POST /` | `201` + `UserDto` |
| `GET /` | `200` + `UserDto` array |
| `POST /{id}/activate` | `204 No Content` |
| `POST /{id}/suspend` | `204 No Content` |
