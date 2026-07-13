using DddStarter.Infrastructure.Configuration.Abstractions;

namespace DddStarter.Infrastructure.Configuration.Rules;

public sealed class DirectionalConfigRule : IConfigSanitizationRule
{
    public string Replacement => " ";

    public bool IsBlockedRune(int rune)
    {
        return rune is 0x200B or 0x202E;
    }
}