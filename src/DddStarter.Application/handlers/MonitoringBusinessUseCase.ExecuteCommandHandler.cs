using DddStarter.Application.Contracts.UseCases;
using MediatR;

namespace DddStarter.Application.UseCases.Commands;

public static partial class MonitoringBusinessUseCase
{
    public sealed class ExecuteCommandHandler : IRequestHandler<ExecuteCommand, Unit>
    {
        private readonly IMonitoringExecutionUseCase _useCase;

        public ExecuteCommandHandler(IMonitoringExecutionUseCase useCase)
        {
            _useCase = useCase;
        }

        public Task<Unit> Handle(ExecuteCommand request, CancellationToken cancellationToken)
        {
            return HandleExecuteAsync(request, cancellationToken);
        }

        public Task<Unit> HandleExecuteAsync(ExecuteCommand request, CancellationToken cancellationToken)
        {
            _useCase.Execute();
            return Task.FromResult(Unit.Value);
        }
    }
}
