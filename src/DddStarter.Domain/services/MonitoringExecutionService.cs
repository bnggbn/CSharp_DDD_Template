using DddStarter.Domain.Enums;
using DddStarter.Domain.ValueObjects;

namespace DddStarter.Domain.Services;

/// <summary>
/// Pure domain service for monitoring execution.
/// It computes a business result only. It must not log, decide persistence,
/// or touch infrastructure; those concerns are orchestrated by the application layer
/// based on the returned <see cref="MonitoringResultVo"/>.
/// </summary>
public sealed class MonitoringExecutionService
{
    public MonitoringResultVo Execute(string triggeredBy)
    {
        SeverityLevel severity = ResolveSeverity(triggeredBy);
        return new MonitoringResultVo(triggeredBy, severity, "Monitoring executed.");
    }

    private static SeverityLevel ResolveSeverity(string triggeredBy)
    {
        // Placeholder domain decision. Replace with real monitoring rules.
        return triggeredBy.Length > 32 ? SeverityLevel.High : SeverityLevel.Low;
    }
}
