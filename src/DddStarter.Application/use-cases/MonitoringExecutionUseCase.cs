using DddStarter.Application.Contracts.Ports;
using DddStarter.Application.Contracts.UseCases;

namespace DddStarter.Application.UseCases;

public sealed class MonitoringExecutionUseCase : IMonitoringExecutionUseCase
{
    private readonly IAppLogger _logger;

    public MonitoringExecutionUseCase(IAppLogger logger)
    {
        _logger = logger;
    }

    public void Execute()
    {
        _logger.Info("MonitoringExecutionUseCase started.");
    }
}