using FluentValidation;

namespace Core.Application.Common.Validation;

public static class PasswordRules
{
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Must(HasUpper).WithMessage("Password must contain at least one uppercase letter.")
            .Must(HasLower).WithMessage("Password must contain at least one lowercase letter.")
            .Must(HasDigit).WithMessage("Password must contain at least one number.")
            .Must(HasSymbol).WithMessage("Password must contain at least one symbol.");
    }

    private static bool HasUpper(string? password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);

    private static bool HasLower(string? password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsLower);

    private static bool HasDigit(string? password) =>
        !string.IsNullOrEmpty(password) && password.Any(ch => ch is >= '0' and <= '9' || char.IsDigit(ch));

    private static bool HasSymbol(string? password) =>
        !string.IsNullOrEmpty(password) && password.Any(ch => !char.IsLetterOrDigit(ch));
}
