namespace DddStarter.Application.Contracts.Ports;

public interface IConnectionStringProvider
{
    string GetRequiredConnectionString(string connectionName);
}