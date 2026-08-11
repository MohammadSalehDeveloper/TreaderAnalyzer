using Core.Contracts.DTOs.Users;
using MediatR;

namespace Core.Application.Users.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? DisplayName = null,
    string? TimeZoneId = null) : IRequest<UserDto>;
