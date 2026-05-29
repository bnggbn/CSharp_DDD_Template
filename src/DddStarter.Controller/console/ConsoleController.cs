using DddStarter.Application.UseCases.Commands;
using DddStarter.Controller.Abstractions;
using MediatR;

namespace DddStarter.Controller.Console;

public sealed class ConsoleController : IAppController
{
    private readonly ISender _sender;

    public ConsoleController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        ExecuteMonitoringCommand command = new("ConsoleController");
        await _sender.Send(command, cancellationToken);
        return 0;
    }
}
