using MediatR;

namespace Core.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(Guid UserId, string RefreshToken) : IRequest;
