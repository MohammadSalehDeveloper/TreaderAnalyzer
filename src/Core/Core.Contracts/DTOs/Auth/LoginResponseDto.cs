using Core.Contracts.DTOs.Users;

namespace Core.Contracts.DTOs.Auth;

public sealed record LoginResponseDto(AuthTokensDto Tokens, UserDto User);
