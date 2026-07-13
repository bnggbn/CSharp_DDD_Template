using System;
using System.Collections.Generic;

namespace DddStarter.Infrastructure.Configuration;

public sealed class AppSettings
{
    public DatabaseConnectionSettings Database { get; set; } = new();
    public ConnectionStringProviderSettings ConnectionStringProvider { get; set; } = new();
    public Dictionary<string, string> MonitorSeverityOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public DataProtectionSettings DataProtection { get; set; } = new();
}

public sealed class ConnectionStringProviderSettings
{
    public string Kind { get; set; } = ConnectionStringProviderKinds.DataProtection;
    public string EnvironmentVariablePrefix { get; set; } = "ConnectionStrings__";
}

public static class ConnectionStringProviderKinds
{
    public const string DataProtection = "DataProtection";
    public const string Environment = "Environment";
}

public sealed class DatabaseConnectionSettings
{
    public string DefaultConnectionName { get; set; } = "Default";
    public Dictionary<string, string> EncryptedConnectionStringPaths { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class DataProtectionSettings
{
    public string ApplicationName { get; set; } = "DddStarter";
    public string KeyDirectory { get; set; } = "keys";
    public string ConnectionStringPurpose { get; set; } = "ConnectionString";
}
