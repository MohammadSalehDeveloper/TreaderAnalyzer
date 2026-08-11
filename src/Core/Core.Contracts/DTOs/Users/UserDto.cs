using Core.Domain.Enums;

namespace Core.Contracts.DTOs.Users;

public sealed record UserDto(
    Guid Id,
    string Email,
    string UserName,
    UserRole Role,
    UserStatus Status,
    string FirstName,
    string LastName,
    string FullName,
    string? PhoneNumber,
    string? TimeZoneId,
    string? DisplayName,
    string? PreferredCurrency,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? LastLoginAtUtc,
    BalanceDto? Balance);
