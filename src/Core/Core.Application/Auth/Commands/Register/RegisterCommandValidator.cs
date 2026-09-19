using Core.Application.Common.Validation;
using FluentValidation;

namespace Core.Application.Auth.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(command => command.UserName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(64);

        RuleFor(command => command.Password).StrongPassword();

        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.PreferredCurrency)
            .NotEmpty()
            .Length(3);

        RuleFor(command => command.PhoneNumber)
            .MaximumLength(32)
            .When(command => command.PhoneNumber is not null);

        RuleFor(command => command.DisplayName)
            .MaximumLength(150)
            .When(command => command.DisplayName is not null);

        RuleFor(command => command.TimeZoneId)
            .MaximumLength(64)
            .When(command => command.TimeZoneId is not null);
    }
}
