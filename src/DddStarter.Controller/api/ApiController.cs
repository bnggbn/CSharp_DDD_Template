using DddStarter.Application.Workflows;
using DddStarter.Controller.Abstractions;

namespace DddStarter.Controller.Api;

public sealed class ApiController : IAppController
{
    private readonly MonitoringWorkflow _workflow;

    public ApiController(MonitoringWorkflow workflow)
    {
        _workflow = workflow;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        await _workflow.ExecuteAsync("ApiController", cancellationToken);
        return 0;
    }
}
