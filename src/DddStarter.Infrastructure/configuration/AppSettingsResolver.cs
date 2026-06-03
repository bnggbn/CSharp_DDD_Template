using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace DddStarter.Infrastructure.Configuration;

/// <summary>
/// Binds application settings and resolves protected configuration values.
/// </summary>
public static class AppSettingsResolver
{
    /// <summary>
    /// Binds the <c>AppSettings</c> section and validates required database secret metadata.
    /// </summary>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The bound application settings.</returns>
    public static AppSettings Bind(IConfiguration configuration)
    {
        AppSettings appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new();

        if (appSettings.Database.EncryptedConnectionStringPaths.Count == 0)
        {
            throw new InvalidOperationException("AppSettings:Database:EncryptedConnectionStringPaths must define at least one connection.");
        }

        if (!appSettings.Database.EncryptedConnectionStringPaths.ContainsKey(appSettings.Database.DefaultConnectionName))
        {
            throw new InvalidOperationException("AppSettings:Database:DefaultConnectionName must point to an existing encrypted connection string path.");
        }

        return appSettings;
    }

    /// <summary>
    /// Resolves and decrypts every configured connection string secret.
    /// </summary>
    /// <param name="appSettings">The application settings containing protected secret metadata.</param>
    /// <returns>A dictionary of decrypted connection strings keyed by connection name.</returns>
    public static IReadOnlyDictionary<string, string> ResolveConnectionStrings(AppSettings appSettings)
    {
        DataProtectionSettings protectionSettings = appSettings.DataProtection;
        string keyDirectory = GetKeyDirectoryPath(protectionSettings);
        Directory.CreateDirectory(keyDirectory);

        IDataProtectionProvider provider = DataProtectionProvider.Create(
            new DirectoryInfo(keyDirectory),
            builder => builder.SetApplicationName(protectionSettings.ApplicationName));

        IDataProtector protector = provider.CreateProtector(protectionSettings.ConnectionStringPurpose);
        Dictionary<string, string> resolved = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string connectionName, string protectedSecretPath) in appSettings.Database.EncryptedConnectionStringPaths)
        {
            string protectedPayload = File.ReadAllText(GetProtectedSecretPath(protectedSecretPath)).Trim();
            resolved[connectionName] = protector.Unprotect(protectedPayload);
        }

        return resolved;
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

    private static string GetProtectedSecretPath(string path)
    {
        string resolved = Path.GetFullPath(
            Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppContext.BaseDirectory, path));

        if (!File.Exists(resolved))
        {
            throw new FileNotFoundException("Protected connection string file was not found.", resolved);
        }

        return resolved;
    }
}
