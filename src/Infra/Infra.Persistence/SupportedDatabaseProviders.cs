namespace Infra.Persistence;

public static class SupportedDatabaseProviders
{
    public const string SqlServer = "SqlServer";

    public static bool IsSqlServer(string? provider) =>
        string.IsNullOrWhiteSpace(provider)
        || string.Equals(provider, SqlServer, StringComparison.OrdinalIgnoreCase);

    public static void EnsureCurrentlySupported(string? provider)
    {
        if (IsSqlServer(provider))
        {
            return;
        }

        throw new NotSupportedException(
            $"Database provider '{provider}' is not supported yet. Use '{SqlServer}'. PostgreSQL support is planned.");
    }
}
