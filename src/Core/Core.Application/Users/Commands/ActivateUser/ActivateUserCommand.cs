using MediatR;

namespace Core.Application.Users.Commands.ActivateUser;

public sealed record ActivateUserCommand(Guid UserId) : IRequest;
