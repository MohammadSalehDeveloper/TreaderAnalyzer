using FluentValidation;

namespace Core.Application.Users.Commands.CreateAdmin;

public sealed class CreateAdminCommandValidator : AbstractValidator<CreateAdminCommand>
{
    public CreateAdminCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(command => command.UserName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(64);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.PhoneNumber)
            .MaximumLength(32)
            .When(command => command.PhoneNumber is not null);
    }
}
