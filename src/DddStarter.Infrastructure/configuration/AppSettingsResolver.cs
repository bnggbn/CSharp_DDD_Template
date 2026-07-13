using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace DddStarter.Infrastructure.Configuration;

/// <summary>
/// Binds application settings and resolves protected configuration values.
/// </summary>
public static class AppSettingsResolver
{
    public sealed record ProtectedConnectionStringResult(string SecretPath, string KeyDirectoryPath, bool ReusedExistingKeyRing);

    /// <summary>
    /// Binds the <c>AppSettings</c> section and validates required database secret metadata.
    /// </summary>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The bound application settings.</returns>
    public static AppSettings Bind(IConfiguration configuration)
    {
        AppSettings appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new();

        if (string.IsNullOrWhiteSpace(appSettings.Database.DefaultConnectionName))
        {
            throw new InvalidOperationException("AppSettings:Database:DefaultConnectionName must be provided.");
        }

        switch (appSettings.ConnectionStringProvider.Kind)
        {
            case ConnectionStringProviderKinds.DataProtection:
                ValidateDataProtectionConnectionStrings(appSettings);
                break;
            case ConnectionStringProviderKinds.Environment:
                if (string.IsNullOrWhiteSpace(appSettings.ConnectionStringProvider.EnvironmentVariablePrefix))
                {
                    throw new InvalidOperationException("AppSettings:ConnectionStringProvider:EnvironmentVariablePrefix must be provided for the Environment provider.");
                }

                break;
            default:
                throw new InvalidOperationException($"Unsupported AppSettings:ConnectionStringProvider:Kind '{appSettings.ConnectionStringProvider.Kind}'.");
        }

        return appSettings;
    }

    private static void ValidateDataProtectionConnectionStrings(AppSettings appSettings)
    {
        if (appSettings.Database.EncryptedConnectionStringPaths.Count == 0)
        {
            throw new InvalidOperationException("AppSettings:Database:EncryptedConnectionStringPaths must define at least one connection.");
        }

        if (!appSettings.Database.EncryptedConnectionStringPaths.ContainsKey(appSettings.Database.DefaultConnectionName))
        {
            throw new InvalidOperationException("AppSettings:Database:DefaultConnectionName must point to an existing encrypted connection string path.");
        }
    }

    /// <summary>
    /// Protects a plaintext connection string and writes it to the configured secret file for the named connection.
    /// </summary>
    /// <param name="appSettings">The application settings containing secret path and Data Protection configuration.</param>
    /// <param name="connectionName">The configured connection name to update.</param>
    /// <param name="connectionString">The plaintext connection string to protect.</param>
    /// <returns>Metadata describing the written secret file and key directory reuse.</returns>
    public static ProtectedConnectionStringResult ProtectConnectionString(AppSettings appSettings, string connectionName, string connectionString)
    {
        if (!appSettings.Database.EncryptedConnectionStringPaths.TryGetValue(connectionName, out string? configuredSecretPath))
        {
            throw new InvalidOperationException($"AppSettings:Database:EncryptedConnectionStringPaths does not define '{connectionName}'.");
        }

        DataProtectionSettings protectionSettings = appSettings.DataProtection;
        string keyDirectory = GetKeyDirectoryPath(protectionSettings);
        bool reusedExistingKeyRing = Directory.Exists(keyDirectory) && Directory.EnumerateFileSystemEntries(keyDirectory).Any();
        Directory.CreateDirectory(keyDirectory);

        IDataProtector protector = CreateConnectionStringProtector(protectionSettings);
        string protectedPayload = protector.Protect(connectionString);

        string secretPath = GetProtectedSecretPath(configuredSecretPath, requireExists: false);
        string? secretDirectory = Path.GetDirectoryName(secretPath);

        if (!string.IsNullOrWhiteSpace(secretDirectory))
        {
            Directory.CreateDirectory(secretDirectory);
        }

        File.WriteAllText(secretPath, protectedPayload);
        return new ProtectedConnectionStringResult(secretPath, keyDirectory, reusedExistingKeyRing);
    }

    public static IDataProtector CreateConnectionStringProtector(DataProtectionSettings protectionSettings)
    {
        string keyDirectory = GetKeyDirectoryPath(protectionSettings);
        Directory.CreateDirectory(keyDirectory);

        IDataProtectionProvider provider = DataProtectionProvider.Create(
            new DirectoryInfo(keyDirectory),
            builder => builder.SetApplicationName(protectionSettings.ApplicationName));

        return provider.CreateProtector(protectionSettings.ConnectionStringPurpose);
    }

    /// <summary>
    /// Resolves the absolute key directory used by Data Protection.
    /// </summary>
    /// <param name="protectionSettings">The Data Protection configuration.</param>
    /// <returns>The absolute key directory path.</returns>
    public static string GetKeyDirectoryPath(DataProtectionSettings protectionSettings)
    {
        return Path.GetFullPath(
            Path.IsPathRooted(protectionSettings.KeyDirectory)
                ? protectionSettings.KeyDirectory
                : Path.Combine(AppContext.BaseDirectory, protectionSettings.KeyDirectory));
    }

    public static string GetProtectedSecretPath(string path, bool requireExists = true)
    {
        string resolved = Path.GetFullPath(
            Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppContext.BaseDirectory, path));

        if (requireExists && !File.Exists(resolved))
        {
            throw new FileNotFoundException("Protected connection string file was not found.", resolved);
        }

        return resolved;
    }
}
