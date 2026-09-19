using System.ComponentModel.DataAnnotations;
using API.Account.Extensions;
using Core.Application.Auth.Commands.ForgotPassword;
using Core.Application.Auth.Commands.GoogleLogin;
using Core.Application.Auth.Commands.Login;
using Core.Application.Auth.Commands.Logout;
using Core.Application.Auth.Commands.Register;
using Core.Application.Auth.Commands.ResetPassword;
using Core.Contracts.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Account.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Sign in with email/user name and password.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.EmailOrUserName, request.Password),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Register a new trader account and return auth tokens.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegisterCommand(
                request.Email,
                request.UserName,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PreferredCurrency ?? "USD",
                request.PhoneNumber,
                request.DisplayName,
                request.TimeZoneId),
            cancellationToken);

        return Created(string.Empty, result);
    }

    /// <summary>Sign out by revoking the refresh token.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new LogoutCommand(User.GetUserId(), request.RefreshToken),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Request a password reset email.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return NoContent();
    }

    /// <summary>Reset password using a token from the reset email.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ResetPasswordCommand(request.Token, request.NewPassword),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Sign in or register using a Google ID token.</summary>
    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GoogleLoginCommand(request.IdToken), cancellationToken);
        return Ok(result);
    }

    public sealed record LoginRequest(
        [property: Required] string EmailOrUserName,
        [property: Required] string Password);

    public sealed record RegisterRequest(
        [property: Required, EmailAddress] string Email,
        [property: Required] string UserName,
        [property: Required] string Password,
        [property: Required] string FirstName,
        [property: Required] string LastName,
        string? PreferredCurrency = "USD",
        string? PhoneNumber = null,
        string? DisplayName = null,
        string? TimeZoneId = null);

    public sealed record LogoutRequest([property: Required] string RefreshToken);

    public sealed record ForgotPasswordRequest([property: Required, EmailAddress] string Email);

    public sealed record ResetPasswordRequest(
        [property: Required] string Token,
        [property: Required] string NewPassword);

    public sealed record GoogleLoginRequest([property: Required] string IdToken);
}
