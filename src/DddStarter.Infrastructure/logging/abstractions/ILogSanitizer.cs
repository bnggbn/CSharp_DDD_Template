namespace DddStarter.Infrastructure.Logging.Abstractions;

/// <summary>
/// Infrastructure-internal abstraction for sanitizing log messages before they reach a sink.
/// This is not an application contract: only the logging infrastructure consumes it.
/// </summary>
public interface ILogSanitizer
{
    string Sanitize(string message);
}
