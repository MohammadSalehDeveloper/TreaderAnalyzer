using Core.Contracts.DTOs.Users;
using MediatR;

namespace Core.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;
