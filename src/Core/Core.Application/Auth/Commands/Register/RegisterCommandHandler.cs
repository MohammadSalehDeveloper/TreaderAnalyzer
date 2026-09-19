using AutoMapper;
using Core.Application.Common.Exceptions;
using Core.Contracts.DTOs.Auth;
using Core.Contracts.Interfaces.Persistence;
using Core.Contracts.Interfaces.Security;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, LoginResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
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

    public async Task<LoginResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        if (await _unitOfWork.Users.UserNameExistsAsync(request.UserName, cancellationToken: cancellationToken))
            throw new ConflictException($"User name '{request.UserName}' is already taken.");

        var user = User.CreateTrader(
            request.Email,
            request.UserName,
            _passwordHasher.Hash(request.Password),
            request.FirstName,
            request.LastName,
            request.PreferredCurrency,
            request.PhoneNumber,
            request.DisplayName,
            request.TimeZoneId);

        // Self-serve signup activates immediately (same as Google registration).
        user.Activate();
        user.RecordLogin();

        var refreshTokenPlainText = _tokenService.GenerateRefreshTokenPlainText();
        var refreshTokenHash = _tokenService.HashToken(refreshTokenPlainText);
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenHash, TimeSpan.FromDays(7));

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tokens = _tokenService.GenerateTokens(user, refreshTokenPlainText, refreshToken.ExpiresAtUtc);
        return new LoginResponseDto(tokens, _mapper.Map<Contracts.DTOs.Users.UserDto>(user));
    }
}
