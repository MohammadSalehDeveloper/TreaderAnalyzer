using Core.Contracts.DTOs.Auth;
using MediatR;

namespace Core.Application.Auth.Commands.Login;

public sealed record LoginCommand(string EmailOrUserName, string Password) : IRequest<LoginResponseDto>;
