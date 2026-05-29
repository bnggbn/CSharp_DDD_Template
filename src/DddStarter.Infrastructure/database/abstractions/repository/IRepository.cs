namespace DddStarter.Infrastructure.Database.Abstractions.Repository;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}