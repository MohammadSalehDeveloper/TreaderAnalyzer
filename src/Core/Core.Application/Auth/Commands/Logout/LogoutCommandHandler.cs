using Core.Application.Common.Exceptions;
using Core.Contracts.Interfaces.Persistence;
using Core.Contracts.Interfaces.Security;
using MediatR;

namespace Core.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var refreshToken = await _unitOfWork.RefreshTokens.GetActiveByTokenHashAsync(tokenHash, cancellationToken);

        if (refreshToken is null || refreshToken.UserId != request.UserId)
            throw new UnauthorizedException("Invalid refresh token.");

        refreshToken.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
