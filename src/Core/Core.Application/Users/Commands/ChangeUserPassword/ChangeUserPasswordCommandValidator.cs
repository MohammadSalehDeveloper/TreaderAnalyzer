using FluentValidation;

namespace Core.Application.Users.Commands.ChangeUserPassword;

public sealed class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);
    }
}
