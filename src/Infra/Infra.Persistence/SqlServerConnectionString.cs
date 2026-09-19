namespace Infra.Persistence;

public static class SqlServerConnectionString
{
    public const string LocalDockerHost = "localhost";
    public const string ComposeServiceHost = "sqlserver";
    public const int Port = 1433;
    public const int LocalDockerHostPort = 14333;
    public const string Database = "TraderAnalyzerDb";
    public const string UserId = "sa";

    public static string Create(
        string host,
        string database,
        string userId,
        string password,
        int port = Port,
        bool trustServerCertificate = true,
        bool encrypt = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(port);

        return $"Server={host},{port};Database={database};User Id={userId};Password={password};TrustServerCertificate={trustServerCertificate};Encrypt={encrypt};";
    }

    public static string LocalDockerDevelopment(string password) =>
        Create(LocalDockerHost, Database, UserId, password, LocalDockerHostPort);
}
