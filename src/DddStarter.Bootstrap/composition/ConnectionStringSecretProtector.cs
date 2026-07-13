using DddStarter.Application.Contracts.Ports;
using DddStarter.Infrastructure.Configuration;

namespace DddStarter.Bootstrap.Composition;

internal sealed class ConnectionStringSecretProtector : IConnectionStringSecretProtector
{
    private readonly AppSettings _appSettings;

    public ConnectionStringSecretProtector(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public ProtectedConnectionStringSecret Protect(string connectionName, string connectionString)
    {
        if (!string.Equals(_appSettings.ConnectionStringProvider.Kind, ConnectionStringProviderKinds.DataProtection, StringComparison.Ordinal))
        {
            throw new NotSupportedException($"The configured connection string provider '{_appSettings.ConnectionStringProvider.Kind}' does not support local secret protection.");
        }

        AppSettingsResolver.ProtectedConnectionStringResult result = AppSettingsResolver.ProtectConnectionString(_appSettings, connectionName, connectionString);
        return new ProtectedConnectionStringSecret(result.SecretPath, result.KeyDirectoryPath, result.ReusedExistingKeyRing);
    }
}