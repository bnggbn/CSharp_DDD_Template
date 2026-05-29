namespace DddStarter.Application.Contracts.Ports;

public interface IConfigSanitizationRule
{
    bool IsBlockedRune(int rune);
    string Replacement { get; }
}