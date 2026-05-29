namespace DddStarter.Application.Contracts.Ports;

public interface IConfigSanitizer
{
    string SafeText(string input);
    string SafeUnicode(string input, string? replacement = null);
}