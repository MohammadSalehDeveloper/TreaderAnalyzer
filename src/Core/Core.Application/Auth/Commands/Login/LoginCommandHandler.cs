using AutoMapper;
using Core.Application.Common.Exceptions;
using Core.Contracts.DTOs.Auth;
using Core.Contracts.Interfaces.Persistence;
using Core.Contracts.Interfaces.Security;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await FindUserAsync(request.EmailOrUserName, cancellationToken)
            ?? throw new UnauthorizedException("Invalid email/user name or password.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email/user name or password.");

        user.RecordLogin();

        var refreshTokenPlainText = _tokenService.GenerateRefreshTokenPlainText();
        var refreshTokenHash = _tokenService.HashToken(refreshTokenPlainText);
        var refreshTokenLifetime = TimeSpan.FromDays(7);
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenHash, refreshTokenLifetime);

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tokens = _tokenService.GenerateTokens(user, refreshTokenPlainText, refreshToken.ExpiresAtUtc);

        return new LoginResponseDto(tokens, _mapper.Map<Contracts.DTOs.Users.UserDto>(user));
    }

    private async Task<User?> FindUserAsync(string emailOrUserName, CancellationToken cancellationToken)
    {
        if (emailOrUserName.Contains('@'))
            return await _unitOfWork.Users.GetByEmailAsync(emailOrUserName, cancellationToken);

        return await _unitOfWork.Users.GetByUserNameAsync(emailOrUserName, cancellationToken);
    }
}
