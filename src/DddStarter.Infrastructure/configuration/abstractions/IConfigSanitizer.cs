namespace DddStarter.Infrastructure.Configuration.Abstractions;

/// <summary>
/// Infrastructure-internal abstraction for sanitizing configuration text.
/// This is not an application contract: only the configuration infrastructure consumes it.
/// </summary>
public interface IConfigSanitizer
{
    string SafeText(string input);
    string SafeUnicode(string input, string? replacement = null);
}
