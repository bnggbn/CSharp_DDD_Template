using System.Text.RegularExpressions;
using DddStarter.Infrastructure.Logging.Abstractions;

namespace DddStarter.Infrastructure.Logging.Rules;

public sealed class AnsiEscapeRemovalRule : ILogSanitizationRule
{
    public string Apply(string input)
    {
        return Regex.Replace(input ?? string.Empty, @"\x1B\[[0-?]*[ -/]*[@-~]", string.Empty);
    }
}