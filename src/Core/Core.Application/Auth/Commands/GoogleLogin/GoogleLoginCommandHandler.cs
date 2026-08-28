using AutoMapper;
using Core.Application.Common.Exceptions;
using Core.Contracts.DTOs.Auth;
using Core.Contracts.Interfaces.Persistence;
using Core.Contracts.Interfaces.Security;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Auth.Commands.GoogleLogin;

public sealed class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, LoginResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IMapper _mapper;

    public GoogleLoginCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IGoogleTokenValidator googleTokenValidator,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _googleTokenValidator = googleTokenValidator;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        GoogleUserInfoDto googleUser;
        try
        {
            googleUser = await _googleTokenValidator.ValidateIdTokenAsync(request.IdToken, cancellationToken);
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("Google ID token"))
        {
            throw new UnauthorizedException("Invalid Google ID token.");
        }

        if (!googleUser.EmailVerified)
            throw new UnauthorizedException("Google account email is not verified.");

        var user = await _unitOfWork.Users.GetByGoogleSubjectIdAsync(googleUser.SubjectId, cancellationToken)
            ?? await _unitOfWork.Users.GetByEmailAsync(googleUser.Email, cancellationToken);

        if (user is null)
        {
            user = await CreateUserFromGoogleAsync(googleUser, cancellationToken);
        }
        else if (user.GoogleSubjectId is null)
        {
            user.LinkGoogleAccount(googleUser.SubjectId);
        }
        else if (!string.Equals(user.GoogleSubjectId, googleUser.SubjectId, StringComparison.Ordinal))
        {
            throw new UnauthorizedException("This email is linked to a different Google account.");
        }

        user.RecordLogin();

        var refreshTokenPlainText = _tokenService.GenerateRefreshTokenPlainText();
        var refreshTokenHash = _tokenService.HashToken(refreshTokenPlainText);
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenHash, TimeSpan.FromDays(7));

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tokens = _tokenService.GenerateTokens(user, refreshTokenPlainText, refreshToken.ExpiresAtUtc);

        return new LoginResponseDto(tokens, _mapper.Map<Contracts.DTOs.Users.UserDto>(user));
    }

    private async Task<User> CreateUserFromGoogleAsync(GoogleUserInfoDto googleUser, CancellationToken cancellationToken)
    {
        var userName = await GenerateUniqueUserNameAsync(googleUser.Email, cancellationToken);
        var passwordPlaceholder = _passwordHasher.Hash(Guid.NewGuid().ToString("N"));

        var user = User.CreateFromGoogle(
            googleUser.Email,
            userName,
            googleUser.SubjectId,
            passwordPlaceholder,
            googleUser.FirstName,
            googleUser.LastName);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

        return user;
    }

    private async Task<string> GenerateUniqueUserNameAsync(string email, CancellationToken cancellationToken)
    {
        var baseName = email.Split('@')[0].Trim();
        if (baseName.Length > 64)
            baseName = baseName[..64];

        var candidate = baseName;
        var suffix = 1;

        while (await _unitOfWork.Users.UserNameExistsAsync(candidate, cancellationToken: cancellationToken))
        {
            var suffixText = suffix.ToString();
            var maxBaseLength = Math.Max(1, 64 - suffixText.Length);
            candidate = $"{baseName[..Math.Min(baseName.Length, maxBaseLength)]}{suffixText}";
            suffix++;
        }

        return candidate;
    }
}
