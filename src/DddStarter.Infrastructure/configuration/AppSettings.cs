using System;
using System.Collections.Generic;

namespace DddStarter.Infrastructure.Configuration;

public sealed class AppSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public Dictionary<string, string> MonitorSeverityOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}