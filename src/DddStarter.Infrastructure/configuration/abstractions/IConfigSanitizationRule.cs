namespace DddStarter.Infrastructure.Configuration.Abstractions;

/// <summary>
/// Infrastructure-internal abstraction for a single configuration-text sanitization rule.
/// </summary>
public interface IConfigSanitizationRule
{
    bool IsBlockedRune(int rune);
    string Replacement { get; }
}
