using DddStarter.Application.Contracts.Ports;
using DddStarter.Domain.Enums;
using DddStarter.Domain.Services;
using DddStarter.Domain.ValueObjects;
using MediatR;

namespace DddStarter.Application.UseCases.Commands;

public partial class MonitoringBusinessUseCase
{
    public sealed class ExecuteCommandHandler : IRequestHandler<ExecuteCommand, Unit>
    {
        private const SeverityLevel PersistThreshold = SeverityLevel.Medium;

        private readonly MonitoringExecutionService _execution;
        private readonly IAppLogger _logger;

        public ExecuteCommandHandler(MonitoringExecutionService execution, IAppLogger logger)
        {
            _execution = execution;
            _logger = logger;
        }

        public Task<Unit> Handle(ExecuteCommand request, CancellationToken cancellationToken)
        {
            return HandleExecuteAsync(request, cancellationToken);
        }

        public Task<Unit> HandleExecuteAsync(ExecuteCommand request, CancellationToken cancellationToken)
        {
            // The domain service is pure: it returns a result and never logs or persists.
            MonitoringResultVo result = _execution.Execute(request.TriggeredBy);

            // Application orchestration owns logging...
            _logger.Info($"Monitoring executed for '{result.TriggeredBy}' (severity: {result.Severity}).");

            // ...and the persist/skip decision, based on the returned result.
            if (result.Severity >= PersistThreshold)
            {
                _logger.Warn($"Monitoring result recorded: {result.Summary}");
            }

            return Task.FromResult(Unit.Value);
        }
    }
}
