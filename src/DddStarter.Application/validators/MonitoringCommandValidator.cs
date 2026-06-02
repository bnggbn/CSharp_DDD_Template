using DddStarter.Application.Contracts.Ports;
using DddStarter.Application.UseCases.Commands;

namespace DddStarter.Application.Validators;

public sealed class MonitoringCommandValidator : IRequestValidator<MonitoringBusinessUseCase.ExecuteCommand>
{
    public IEnumerable<string> Validate(MonitoringBusinessUseCase.ExecuteCommand request)
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
