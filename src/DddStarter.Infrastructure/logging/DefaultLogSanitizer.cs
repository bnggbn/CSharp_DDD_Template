using DddStarter.Infrastructure.Logging.Abstractions;

namespace DddStarter.Infrastructure.Logging;

public sealed class DefaultLogSanitizer : ILogSanitizer
{
    private readonly IReadOnlyList<ILogSanitizationRule> _rules;

    public DefaultLogSanitizer(IEnumerable<ILogSanitizationRule> rules)
    {
        _rules = rules.ToList();
    }

    public string Sanitize(string message)
    {
        string current = message ?? string.Empty;
        foreach (ILogSanitizationRule rule in _rules)
        {
            current = rule.Apply(current);
        }

        return current;
    }
}