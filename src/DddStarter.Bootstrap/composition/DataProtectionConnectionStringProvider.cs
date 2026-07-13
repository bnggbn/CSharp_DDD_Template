using DddStarter.Application.Contracts.Ports;
using DddStarter.Infrastructure.Configuration;
using Microsoft.AspNetCore.DataProtection;

namespace DddStarter.Bootstrap.Composition;

internal sealed class DataProtectionConnectionStringProvider : IConnectionStringProvider
{
    private readonly AppSettings _appSettings;

    public DataProtectionConnectionStringProvider(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public string GetRequiredConnectionString(string connectionName)
    {
        if (!_appSettings.Database.EncryptedConnectionStringPaths.TryGetValue(connectionName, out string? configuredSecretPath))
        {
            throw new InvalidOperationException($"AppSettings:Database:EncryptedConnectionStringPaths does not define '{connectionName}'.");
        }

        string protectedPayload = File.ReadAllText(AppSettingsResolver.GetProtectedSecretPath(configuredSecretPath)).Trim();
        return AppSettingsResolver.CreateConnectionStringProtector(_appSettings.DataProtection).Unprotect(protectedPayload);
    }
}