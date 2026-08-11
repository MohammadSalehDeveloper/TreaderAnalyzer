using MediatR;

namespace Core.Application.Users.Commands.SuspendUser;

public sealed record SuspendUserCommand(Guid UserId) : IRequest;
