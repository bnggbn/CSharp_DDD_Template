using System.Text;
using DddStarter.Application.Contracts.Ports;

namespace DddStarter.Infrastructure.Configuration;

public sealed class ConfigSanitizer : IConfigSanitizer
{
    private readonly IReadOnlyList<IConfigSanitizationRule> _rules;

    public ConfigSanitizer(IEnumerable<IConfigSanitizationRule> rules)
    {
        _rules = rules.ToList();
    }

    public string SafeText(string input)
    {
        return SafeUnicode(input, " ");
    }

    public string SafeUnicode(string input, string? replacement = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        StringBuilder sb = new(input.Length);
        foreach (char c in input)
        {
            int rune = c;
            IConfigSanitizationRule? blockedBy = _rules.FirstOrDefault(rule => rule.IsBlockedRune(rune));
            if (blockedBy == null)
            {
                sb.Append(c);
                continue;
            }

            sb.Append(replacement ?? blockedBy.Replacement);
        }

        return sb.ToString();
    }
}