using MediatR;

namespace Core.Application.Users.Commands.ChangeUserPassword;

public sealed record ChangeUserPasswordCommand(Guid UserId, string NewPassword) : IRequest;
