# Sample Vertical Slice

Use this as the canonical clone-and-modify example:
- `src/DddStarter.Domain/services/MonitoringExecutionService.cs`
- `src/DddStarter.Domain/value-objects/MonitoringResultVo.cs`
- `src/DddStarter.Application/use-cases/MonitoringBusinessUseCase.cs`
- `src/DddStarter.Application/handlers/MonitoringBusinessUseCase.ExecuteCommandHandler.cs`
- `src/DddStarter.Application/validators/MonitoringCommandValidator.cs`
- `src/DddStarter.Application/workflows/MonitoringWorkflow.cs`
- `src/DddStarter.Application/contracts/ports/IAppLogger.cs`

Role split note:
- `MonitoringBusinessUseCase.cs` defines request contracts (`record *Command/*Query`) only.
- `MonitoringExecutionService.cs` is a **pure domain service** (under `domain/services/`, enforced by `PATH006`). It computes a result and returns a `MonitoringResultVo`. It must not log, decide persistence, or reference application/infrastructure (`DEP001`).
- `ExecuteCommandHandler` is the **application orchestrator**: it calls the domain service, then owns logging (`IAppLogger`) and the persist/skip decision based on the returned result.

Result-return pattern:
- Domain services never log and never decide "落檔 or not". They return a result value object.
- The handler inspects the result (e.g. `result.Severity`) and orchestrates side effects (logging, persistence, notifications).

Port placement:
- Only interfaces the application actually calls stay in `application/contracts/ports/` (e.g. `IAppLogger`, `IRequestValidator`, `IRequireValidation`).
- Infrastructure-internal abstractions live next to their implementations in infrastructure
  (`infrastructure/logging/abstractions/`, `infrastructure/configuration/abstractions/`), not in application contracts.
