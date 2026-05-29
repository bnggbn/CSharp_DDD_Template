using Dapper;
using DddStarter.Infrastructure.Database.Abstractions.Repository;
using DddStarter.Infrastructure.Database.Core;
using DddStarter.Infrastructure.Database.Tables;

namespace DddStarter.Infrastructure.Database.Repository;

public sealed class ExampleTableRepository : IExampleTableRepository
{
    private readonly IDbContextCore _dbContext;

    public ExampleTableRepository(IDbContextCore dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExampleTable?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = _dbContext.CreateConnection();
        const string sql = "SELECT Id, Name FROM ExampleTable WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<ExampleTable>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}