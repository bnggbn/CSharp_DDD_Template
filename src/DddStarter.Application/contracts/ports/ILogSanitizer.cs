namespace DddStarter.Application.Contracts.Ports;

public interface ILogSanitizer
{
    string Sanitize(string message);
}