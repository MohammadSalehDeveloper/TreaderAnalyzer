using Core.Application.Common.Validation;
using FluentValidation;

namespace Core.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty();
        RuleFor(command => command.NewPassword).StrongPassword();
    }
}
