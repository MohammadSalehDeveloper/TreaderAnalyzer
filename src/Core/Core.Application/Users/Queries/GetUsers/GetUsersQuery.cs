using Core.Contracts.DTOs.Users;
using Core.Domain.Enums;
using MediatR;

namespace Core.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    UserRole? Role = null,
    UserStatus? Status = null) : IRequest<IReadOnlyList<UserDto>>;
