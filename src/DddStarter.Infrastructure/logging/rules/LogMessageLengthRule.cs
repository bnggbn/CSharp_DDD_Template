using DddStarter.Infrastructure.Logging.Abstractions;

namespace DddStarter.Infrastructure.Logging.Rules;

public sealed class LogMessageLengthRule : ILogSanitizationRule
{
    private readonly int _maxLength;

    public LogMessageLengthRule(int maxLength = 4000)
    {
        _maxLength = maxLength > 0 ? maxLength : 4000;
    }

    public string Apply(string input)
    {
        string value = input ?? string.Empty;
        if (value.Length <= _maxLength)
        {
            return value;
        }

        return value.Substring(0, _maxLength) + "... [truncated]";
    }
}