# Feature: Google Login

| Property | Value |
|----------|-------|
| Story | Sign in or register using Google OAuth ID token |
| Endpoint | `POST /api/auth/google` |
| Controller | [AuthController](../controllers/AuthController.md) |
| Command | `GoogleLoginCommand` |
| Auth required | No |

## Request

```http
POST /api/auth/google
Content-Type: application/json

{
  "idToken": "{Google ID token from client-side Google Sign-In}"
}
```

The client obtains the ID token using [Google Identity Services](https://developers.google.com/identity/gsi/web) (web) or the platform SDK (mobile).

## Response — 200 OK

Same shape as login: `LoginResponseDto` with tokens + user profile.

## Application Flow

1. Validate ID token with Google (`IGoogleTokenValidator`)
2. Require verified Google email
3. Resolve user:
   - By `GoogleSubjectId`, or
   - By email (link Google account if exists), or
   - Create new trader via `User.CreateFromGoogle`
4. Record login and issue tokens

## New User Defaults

| Field | Value |
|-------|-------|
| Role | `Trader` |
| Status | `Active` (skips pending) |
| Preferred currency | `USD` |
| User name | Email prefix, with numeric suffix if taken |
| Password | Random placeholder hash (not used for login) |

## Errors

| Status | Cause |
|--------|-------|
| 401 | Invalid token, unverified email, or email linked to different Google account |

## Options / Configuration

| Option | Location | Notes |
|--------|----------|-------|
| Google Client ID | `Google:ClientId` | Must match the client that issued the ID token |

## Setup Checklist

1. Create OAuth 2.0 Client ID in [Google Cloud Console](https://console.cloud.google.com/)
2. Set `Google:ClientId` in `appsettings.json` or user secrets
3. Configure authorized JavaScript origins / redirect URIs for your frontend

## Domain Changes

- `User.GoogleSubjectId` — unique when set, links account to Google subject
