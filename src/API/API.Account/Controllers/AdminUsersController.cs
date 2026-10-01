using System.ComponentModel.DataAnnotations;
using Core.Application.Users.Commands.ActivateUser;
using Core.Application.Users.Commands.CreateAdmin;
using Core.Application.Users.Commands.SuspendUser;
using Core.Application.Users.Queries.GetUsers;
using Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Account.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Create an admin. Open only while no admin exists; later calls require an admin token.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdminRequest request,
        CancellationToken cancellationToken)
    {
        var admins = await _mediator.Send(new GetUsersQuery(UserRole.Admin), cancellationToken);
        if (admins.Count > 0)
        {
            if (User.Identity?.IsAuthenticated != true)
                return Unauthorized();

            if (!User.IsInRole(nameof(UserRole.Admin)))
                return Forbid();
        }

        var user = await _mediator.Send(
            new CreateAdminCommand(
                request.Email,
                request.UserName,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PhoneNumber),
            cancellationToken);

        return Created($"/api/admin/users/{user.Id}", user);
    }

    /// <summary>List users, optionally filtered by role and status.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] UserRole? role,
        [FromQuery] UserStatus? status,
        CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(new GetUsersQuery(role, status), cancellationToken);
        return Ok(users);
    }

    /// <summary>Activate a pending or suspended user.</summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Suspend an active user.</summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SuspendUserCommand(id), cancellationToken);
        return NoContent();
    }

    public sealed record CreateAdminRequest(
        [param: Required, EmailAddress] string Email,
        [param: Required] string UserName,
        [param: Required] string Password,
        [param: Required] string FirstName,
        [param: Required] string LastName,
        string? PhoneNumber = null);
}
