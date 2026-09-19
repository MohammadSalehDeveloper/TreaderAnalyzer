namespace Web.Client.Services.Auth;

public sealed record LoginRequest(string EmailOrUserName, string Password);

public sealed record RegisterRequest(
    string Email,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    string PreferredCurrency = "USD",
    string? PhoneNumber = null,
    string? DisplayName = null,
    string? TimeZoneId = null);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Token, string NewPassword);

public sealed record LogoutRequest(string RefreshToken);
