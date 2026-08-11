using FluentValidation;

namespace Core.Application.Users.Commands.SuspendUser;

public sealed class SuspendUserCommandValidator : AbstractValidator<SuspendUserCommand>
{
    public SuspendUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
    }
}
