using MediatR;

namespace Core.Application.Users.Commands.SoftDeleteUser;

public sealed record SoftDeleteUserCommand(Guid UserId) : IRequest;
