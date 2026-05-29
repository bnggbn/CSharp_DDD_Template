using DddStarter.Application.Contracts.Ports;
using DddStarter.Application.UseCases.Commands;

namespace DddStarter.Application.UseCases.Validators;

public sealed class ExecuteMonitoringCommandValidator : IRequestValidator<ExecuteMonitoringCommand>
{
    public IEnumerable<string> Validate(ExecuteMonitoringCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.TriggeredBy))
        {
            yield return "TriggeredBy is required.";
            yield break;
        }

        if (request.TriggeredBy.Length > 64)
        {
            yield return "TriggeredBy length must be <= 64.";
        }
    }
}
