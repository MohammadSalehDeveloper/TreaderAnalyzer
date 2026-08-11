using Core.Contracts.DTOs.Users;
using MediatR;

namespace Core.Application.Users.Commands.CreateTrader;

public sealed record CreateTraderCommand(
    string Email,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    string PreferredCurrency,
    string? PhoneNumber = null,
    string? DisplayName = null,
    string? TimeZoneId = null) : IRequest<UserDto>;
