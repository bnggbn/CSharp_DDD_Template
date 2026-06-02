using DddStarter.Application.Workflows;
using DddStarter.Controller.Abstractions;

namespace DddStarter.Controller.Console;

public sealed class ConsoleController : IAppController
{
    private readonly MonitoringWorkflow _workflow;

    public ConsoleController(MonitoringWorkflow workflow)
    {
        _workflow = workflow;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        await _workflow.ExecuteAsync("ConsoleController", cancellationToken);
        return 0;
    }
}
