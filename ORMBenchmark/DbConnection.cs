using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Benchmarks;

public static class DbConnection
{
    private static readonly string connectionString;

    static DbConnection()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

        var config = builder.Build();
        connectionString = config.GetSection("ConnectionStrings")["Blogging"];
    }

    public static string ConnectionString =>
        new NpgsqlConnectionStringBuilder(connectionString).ConnectionString;
}