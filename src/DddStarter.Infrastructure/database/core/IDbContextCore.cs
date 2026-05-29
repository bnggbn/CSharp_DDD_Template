using System.Data;

namespace DddStarter.Infrastructure.Database.Core;

public interface IDbContextCore
{
    IDbConnection CreateConnection();
}