namespace Core.Contracts.DTOs.Users;

public sealed record BalanceDto(
    Guid Id,
    Guid UserId,
    string Currency,
    decimal Available,
    decimal Reserved,
    decimal Total,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
