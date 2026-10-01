namespace Web.Client.Services.Account;

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? DisplayName = null,
    string? TimeZoneId = null);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
