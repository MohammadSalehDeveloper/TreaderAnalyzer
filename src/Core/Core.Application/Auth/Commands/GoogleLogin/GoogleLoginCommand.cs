using Core.Contracts.DTOs.Auth;
using MediatR;

namespace Core.Application.Auth.Commands.GoogleLogin;

public sealed record GoogleLoginCommand(string IdToken) : IRequest<LoginResponseDto>;
