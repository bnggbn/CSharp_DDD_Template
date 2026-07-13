using System;
using DddStarter.Domain.Enums;

namespace DddStarter.Domain.ValueObjects;

/// <summary>
/// Immutable outcome produced by a domain monitoring execution.
/// The domain service never logs or persists; it returns this result so the
/// application layer can orchestrate logging and the persist/skip decision.
/// </summary>
public sealed class MonitoringResultVo
{
    public MonitoringResultVo(string triggeredBy, SeverityLevel severity, string summary)
    {
        if (string.IsNullOrWhiteSpace(triggeredBy))
        {
            throw new ArgumentException("TriggeredBy is required.", nameof(triggeredBy));
        }

        TriggeredBy = triggeredBy.Trim();
        Severity = severity;
        Summary = summary ?? string.Empty;
    }

    public string TriggeredBy { get; }

    public SeverityLevel Severity { get; }

    public string Summary { get; }
}
