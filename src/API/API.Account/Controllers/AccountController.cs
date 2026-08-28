using System.ComponentModel.DataAnnotations;
using API.Account.Extensions;
using Core.Application.Users.Commands.ChangeUserPassword;
using Core.Application.Users.Commands.SoftDeleteUser;
using Core.Application.Users.Commands.UpdateUserProfile;
using Core.Application.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Account.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public sealed class AccountController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get the authenticated user's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetUserByIdQuery(userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Update the authenticated user's profile.</summary>
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(
            new UpdateUserProfileCommand(
                userId,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.DisplayName,
                request.TimeZoneId),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Change the authenticated user's password.</summary>
    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _mediator.Send(
            new ChangeUserPasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Soft-delete the authenticated user's account.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new SoftDeleteUserCommand(userId), cancellationToken);
        return NoContent();
    }

    public sealed record UpdateProfileRequest(
        [property: Required] string FirstName,
        [property: Required] string LastName,
        string? PhoneNumber = null,
        string? DisplayName = null,
        string? TimeZoneId = null);

    public sealed record ChangePasswordRequest(
        [property: Required] string CurrentPassword,
        [property: Required] string NewPassword);
}
