# Sample Vertical Slice

Use this as the canonical clone-and-modify example:
- `src/DddStarter.Application/use-cases/MonitoringBusinessUseCase.cs`
- `src/DddStarter.Application/handlers/MonitoringBusinessUseCase.ExecuteCommandHandler.cs`
- `src/DddStarter.Application/validators/MonitoringCommandValidator.cs`
- `src/DddStarter.Application/workflows/MonitoringWorkflow.cs`
- `src/DddStarter.Application/contracts/use-cases/IMonitoringExecutionUseCase.cs`
- `src/DddStarter.Application/services/MonitoringExecutionService.cs`

Role split note:
- `MonitoringBusinessUseCase.cs` defines request contracts (`record *Command/*Query`) only.
- `MonitoringExecutionService.cs` is an application service implementation behind `IMonitoringExecutionUseCase`, consumed by handlers.
