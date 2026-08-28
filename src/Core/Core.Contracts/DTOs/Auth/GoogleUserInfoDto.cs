namespace Core.Contracts.DTOs.Auth;

public sealed record GoogleUserInfoDto(
    string SubjectId,
    string Email,
    string FirstName,
    string LastName,
    bool EmailVerified);
