# Feature: Create admin

| Property | Value |
|----------|-------|
| Story | Create an admin account by calling the API directly |
| Endpoint | `POST /api/admin/users` |
| Controller | [AdminUsersController](../controllers/AdminUsersController.md) |
| Command | `CreateAdminCommand` |
| Auth required | No, until the first admin exists. After that, Bearer JWT with role `Admin` |

The Blazor client does not call this endpoint.

## Request

```http
POST /api/admin/users
Content-Type: application/json

{
  "email": "ann@example.com",
  "userName": "ann",
  "password": "Secret123",
  "firstName": "Ann",
  "lastName": "Admin",
  "phoneNumber": null
}
```

`phoneNumber` is optional. Password must be 8–128 characters. The response does not include the password.

## Response — 201 Created

`UserDto` for an `Admin` in `Active` status. Sign in afterward with `POST /api/auth/login` using `userName` and `password`.

## Errors

| Status | Cause |
|--------|-------|
| 400 | Validation failure |
| 401 | An admin already exists and the request has no token |
| 403 | The caller is signed in but is not an admin |
| 409 | Email or user name is already registered |

## Application flow

1. If any admin already exists, the caller must be an admin
2. Reject a duplicate email or user name
3. Hash the password, then `User.CreateAdmin` (starts `Active`)
4. Save and return `UserDto`

## Security notes

- The first call can be anonymous so a database with no admin can be bootstrapped
- After that, only an admin can create another admin
- The plain password is hashed before it is stored and is never returned
