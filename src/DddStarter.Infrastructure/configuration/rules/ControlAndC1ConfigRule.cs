using DddStarter.Infrastructure.Configuration.Abstractions;

namespace DddStarter.Infrastructure.Configuration.Rules;

public sealed class ControlAndC1ConfigRule : IConfigSanitizationRule
{
    public string Replacement => " ";

    public bool IsBlockedRune(int rune)
    {
        if (rune < 0x20 || rune == 0x7F) return true;
        if (rune >= 0x80 && rune <= 0x9F) return true;
        return false;
    }
}