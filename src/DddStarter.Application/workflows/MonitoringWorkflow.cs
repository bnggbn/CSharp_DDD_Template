using DddStarter.Application.UseCases.Commands;
using MediatR;

namespace DddStarter.Application.Workflows;

public sealed class MonitoringWorkflow
{
    private readonly ISender _sender;

    public MonitoringWorkflow(ISender sender)
    {
        _sender = sender;
    }

    public Task ExecuteAsync(string triggeredBy, CancellationToken cancellationToken = default)
    {
        return _sender.Send(new MonitoringBusinessUseCase.ExecuteCommand(triggeredBy), cancellationToken);
    }
}
