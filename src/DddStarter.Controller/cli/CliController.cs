using DddStarter.Application.Workflows;
using DddStarter.Controller.Abstractions;

namespace DddStarter.Controller.Cli;

public sealed class CliController : IAppController
{
    private readonly MonitoringWorkflow _workflow;

    public CliController(MonitoringWorkflow workflow)
    {
        _workflow = workflow;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length > 0 && string.Equals(args[0], "run", StringComparison.OrdinalIgnoreCase))
        {
            await _workflow.ExecuteAsync("CliController", cancellationToken);
            return 0;
        }

        return 1;
    }
}
