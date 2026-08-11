using FluentValidation;

namespace Core.Application.Users.Commands.SoftDeleteUser;

public sealed class SoftDeleteUserCommandValidator : AbstractValidator<SoftDeleteUserCommand>
{
    public SoftDeleteUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
    }
}
