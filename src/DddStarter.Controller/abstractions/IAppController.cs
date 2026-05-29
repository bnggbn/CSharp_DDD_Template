namespace DddStarter.Controller.Abstractions;

public interface IAppController
{
    Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default);
}
