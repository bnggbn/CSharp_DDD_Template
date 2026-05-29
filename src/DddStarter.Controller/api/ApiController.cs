using DddStarter.Application.UseCases.Commands;
using DddStarter.Controller.Abstractions;
using MediatR;

namespace DddStarter.Controller.Api;

public sealed class ApiController : IAppController
{
    private readonly ISender _sender;

    public ApiController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        ExecuteMonitoringCommand command = new("ApiController");
        await _sender.Send(command, cancellationToken);
        return 0;
    }
}
