# Feature: Delete Account

| Property | Value |
|----------|-------|
| Story | Authenticated user deletes their own account |
| Endpoint | `DELETE /api/account` |
| Controller | [AccountController](../controllers/AccountController.md) |
| Command | `SoftDeleteUserCommand` |
| Auth required | Yes — Bearer JWT |

## Request

```http
DELETE /api/account
Authorization: Bearer {accessToken}
```

No request body.

## Response — 204 No Content

## Application Flow

1. Resolve user ID from JWT
2. Call `User.SoftDelete()` — sets status to `Closed` and marks entity deleted
3. Persist changes

## Effects

| Aspect | Behavior |
|--------|----------|
| User status | Set to `Closed` |
| Soft delete | `IsDeleted = true`, `DeletedAtUtc` set |
| Login | Blocked — inactive users cannot sign in |
| Data | Row retained in database (soft delete query filter hides it) |

## Errors

| Status | Cause |
|--------|-------|
| 401 | Missing/invalid JWT |
| 404 | User not found |

## Future Enhancements

- Optional password confirmation before delete
- Grace period / account recovery window
- Hard delete job for GDPR compliance
