using DddStarter.Application.Contracts.UseCases;
using DddStarter.Application.UseCases.Commands;
using MediatR;

namespace DddStarter.Application.UseCases.Handlers;

public sealed class ExecuteMonitoringCommandHandler : IRequestHandler<ExecuteMonitoringCommand, Unit>
{
    private readonly IMonitoringExecutionUseCase _useCase;

    public ExecuteMonitoringCommandHandler(IMonitoringExecutionUseCase useCase)
    {
        _useCase = useCase;
    }

    public Task<Unit> Handle(ExecuteMonitoringCommand request, CancellationToken cancellationToken)
    {
        _useCase.Execute();
        return Task.FromResult(Unit.Value);
    }
}