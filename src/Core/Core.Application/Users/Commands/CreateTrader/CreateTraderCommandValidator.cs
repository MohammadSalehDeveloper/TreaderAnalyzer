using FluentValidation;

namespace Core.Application.Users.Commands.CreateTrader;

public sealed class CreateTraderCommandValidator : AbstractValidator<CreateTraderCommand>
{
    public CreateTraderCommandValidator()
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
