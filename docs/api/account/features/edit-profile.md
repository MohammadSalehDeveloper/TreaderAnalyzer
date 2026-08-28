# Feature: Edit Account Profile

| Property | Value |
|----------|-------|
| Story | Authenticated user updates profile information |
| Endpoint | `PUT /api/account/profile` |
| Controller | [AccountController](../controllers/AccountController.md) |
| Command | `UpdateUserProfileCommand` |
| Auth required | Yes — Bearer JWT |

## Request

```http
PUT /api/account/profile
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Trader",
  "phoneNumber": "+1-555-0100",
  "displayName": "JT",
  "timeZoneId": "America/New_York"
}
```

## Editable Fields

| Field | Required | Notes |
|-------|----------|-------|
| `firstName` | Yes | Max 100 chars |
| `lastName` | Yes | Max 100 chars |
| `phoneNumber` | No | Max 32 chars |
| `displayName` | No | Trader only; defaults to full name |
| `timeZoneId` | No | IANA time zone ID |

**Not editable via this endpoint:** email, user name, role, password, preferred currency.

## Response — 200 OK

Returns updated `UserDto`.

## Errors

| Status | Cause |
|--------|-------|
| 400 | Validation or domain rule failure |
| 401 | Missing/invalid JWT |
| 404 | User not found |

## Domain Rules

- Closed or deleted accounts cannot be updated (`User.UpdateProfile`)

## Related

- View profile: `GET /api/account/me`
