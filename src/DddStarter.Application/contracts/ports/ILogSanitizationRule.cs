namespace DddStarter.Application.Contracts.Ports;

public interface ILogSanitizationRule
{
    string Apply(string input);
}