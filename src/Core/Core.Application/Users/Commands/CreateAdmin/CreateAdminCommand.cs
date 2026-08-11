using Core.Contracts.DTOs.Users;
using MediatR;

namespace Core.Application.Users.Commands.CreateAdmin;

public sealed record CreateAdminCommand(
    string Email,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null) : IRequest<UserDto>;
