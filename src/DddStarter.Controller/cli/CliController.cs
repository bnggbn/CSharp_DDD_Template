using DddStarter.Application.UseCases.Commands;
using DddStarter.Controller.Abstractions;
using MediatR;

namespace DddStarter.Controller.Cli;

public sealed class CliController : IAppController
{
    private readonly ISender _sender;

    public CliController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length > 0 && string.Equals(args[0], "run", StringComparison.OrdinalIgnoreCase))
        {
            ExecuteMonitoringCommand command = new("CliController");
            await _sender.Send(command, cancellationToken);
            return 0;
        }

        return 1;
    }
}
