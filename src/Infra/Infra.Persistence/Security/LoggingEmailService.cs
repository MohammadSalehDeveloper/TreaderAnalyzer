using Core.Contracts.Interfaces.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infra.Persistence.Security;

public sealed class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;
    private readonly IConfiguration _configuration;

    public LoggingEmailService(ILogger<LoggingEmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task SendPasswordResetEmailAsync(
        string email,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        var resetUrlBase = _configuration["Email:PasswordResetUrlBase"] ?? "https://localhost/reset-password";
        var resetUrl = $"{resetUrlBase}?token={Uri.EscapeDataString(resetToken)}";

        _logger.LogInformation(
            "Password reset email for {Email}. Reset URL: {ResetUrl}",
            email,
            resetUrl);

        return Task.CompletedTask;
    }
}
