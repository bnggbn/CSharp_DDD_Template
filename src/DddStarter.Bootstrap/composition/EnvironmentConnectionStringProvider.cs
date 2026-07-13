using DddStarter.Application.Contracts.Ports;
using DddStarter.Infrastructure.Configuration;

namespace DddStarter.Bootstrap.Composition;

internal sealed class EnvironmentConnectionStringProvider : IConnectionStringProvider
{
    private readonly AppSettings _appSettings;

    public EnvironmentConnectionStringProvider(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public string GetRequiredConnectionString(string connectionName)
    {
        string variableName = $"{_appSettings.ConnectionStringProvider.EnvironmentVariablePrefix}{connectionName}";
        string? connectionString = Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Environment variable '{variableName}' was not found for connection '{connectionName}'.");
        }

        return connectionString;
    }
}