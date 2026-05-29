using DddStarter.Application.Contracts.Ports;
using NLog;

namespace DddStarter.Infrastructure.Logging;

public sealed class NLogAppLogger : IAppLogger
{
    private readonly Logger _logger;
    private readonly ILogSanitizer _sanitizer;

    public NLogAppLogger(ILogSanitizer sanitizer)
    {
        _sanitizer = sanitizer;
        _logger = LogManager.GetLogger("DddStarter");
    }

    public void Info(string message) => _logger.Info(_sanitizer.Sanitize(message));
    public void Warn(string message) => _logger.Warn(_sanitizer.Sanitize(message));

    public void Error(string message, Exception? exception = null)
    {
        string sanitized = _sanitizer.Sanitize(message);
        if (exception == null)
        {
            _logger.Error(sanitized);
            return;
        }

        _logger.Error(exception, sanitized);
    }
}