using DddStarter.Application.UseCases.Commands;
using DddStarter.Application.Contracts.Ports;
using DddStarter.Dispatching.Contracts;

namespace DddStarter.Application.Workflows;

public sealed class MonitoringWorkflow
{
    private readonly IDispatcher _dispatcher;

    public MonitoringWorkflow(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public Task ExecuteAsync(string triggeredBy, CancellationToken cancellationToken = default)
    {
        return _dispatcher.Send(new MonitoringBusinessUseCase.ExecuteCommand(triggeredBy), cancellationToken);
    }
}
