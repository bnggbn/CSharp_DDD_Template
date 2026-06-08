using DddStarter.Application.Contracts.UseCases;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DddStarter.Application.UseCases.Commands;

public partial class MonitoringBusinessUseCase
{
    public sealed class ExecuteCommandHandler : IRequestHandler<ExecuteCommand, Unit>
    {
        private readonly ILogger<ExecuteCommandHandler> _logger;
        public ExecuteCommandHandler(ILogger<ExecuteCommandHandler> logger)
        {
            _logger = logger;
        }

        public Task<Unit> Handle(ExecuteCommand request, CancellationToken cancellationToken)
        {
            return HandleExecuteAsync(request, cancellationToken);
        }

        public Task<Unit> HandleExecuteAsync(ExecuteCommand request, CancellationToken cancellationToken)
        {
            request.TriggeredBy.Length.ToString();
            return Task.FromResult(Unit.Value);
        }
    }
}
