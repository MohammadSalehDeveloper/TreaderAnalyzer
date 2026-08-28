using Core.Application.Common.Exceptions;
using Core.Contracts.Interfaces.Persistence;
using Core.Contracts.Interfaces.Security;
using MediatR;

namespace Core.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashToken(request.Token);
        var resetToken = await _unitOfWork.PasswordResetTokens.GetValidByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired password reset token.");

        var user = await _unitOfWork.Users.GetByIdAsync(resetToken.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), resetToken.UserId);

        user.ChangePassword(_passwordHasher.Hash(request.NewPassword));
        resetToken.MarkUsed();

        await _unitOfWork.RefreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
