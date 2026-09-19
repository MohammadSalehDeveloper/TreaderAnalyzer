using Core.Contracts.Interfaces.Persistence;
using Core.Contracts.Interfaces.Security;
using Infra.Persistence.Context;
using Infra.Persistence.Repositories;
using Infra.Persistence.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string connectionString,
        string? provider = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        SupportedDatabaseProviders.EnsureCurrentlySupported(provider);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IEmailService, LoggingEmailService>();
        services.AddSingleton<IGoogleTokenValidator, GoogleTokenValidator>();

        return services;
    }
}
