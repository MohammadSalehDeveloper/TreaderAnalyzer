using Core.Contracts.DTOs.Auth;
using MediatR;

namespace Core.Application.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    string PreferredCurrency = "USD",
    string? PhoneNumber = null,
    string? DisplayName = null,
    string? TimeZoneId = null) : IRequest<LoginResponseDto>;
