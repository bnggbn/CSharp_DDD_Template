using DddStarter.Application.Contracts.Ports;
using DddStarter.Application.Contracts.UseCases;

namespace DddStarter.Application.Services;

public sealed class MonitoringExecutionService : IMonitoringExecutionUseCase
{
    private readonly IAppLogger _logger;

    public MonitoringExecutionService(IAppLogger logger)
    {
        _logger = logger;
    }

    public void Execute()
    {
        _logger.Info("MonitoringExecutionService started.");
    }
}
