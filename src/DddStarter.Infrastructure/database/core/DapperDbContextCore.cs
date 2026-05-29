using System.Data;
using Microsoft.Data.SqlClient;

namespace DddStarter.Infrastructure.Database.Core;

public sealed class DapperDbContextCore : IDbContextCore
{
    private readonly string _connectionString;

    public DapperDbContextCore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}