using FluentValidation;

namespace Core.Application.Users.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(100);

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
