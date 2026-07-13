namespace DddStarter.Infrastructure.Logging.Abstractions;

/// <summary>
/// Infrastructure-internal abstraction for a single log-text sanitization step.
/// </summary>
public interface ILogSanitizationRule
{
    string Apply(string input);
}
