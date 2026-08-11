using FluentValidation;

namespace Core.Application.Users.Commands.ActivateUser;

public sealed class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
    }
}
