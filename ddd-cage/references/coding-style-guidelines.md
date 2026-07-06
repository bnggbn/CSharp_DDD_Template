# Coding Style and Flow Placement Guide

This guide is for Codex/GPT when editing C#/.NET code in a DDD/CQRS-shaped repository.

It is not the law. The committed repository policy, linter output, CI checks, and existing local conventions override this guide. Use this guide only when the concrete repo is silent.

## Decision Order

When deciding where code belongs, use this order:

1. Follow the committed linter policy and rule catalog.
2. Follow repo documentation and the canonical sample slice.
3. Follow existing nearby code.
4. Follow this guide as a fallback.

Do not change policy, allowlists, baselines, suppressions, or linter logic to make an implementation pass unless the user explicitly authorized a policy change.

## Flow-First Rule

Before adding a new feature or refactor slice, write the business flow as one sentence.

Example:

```text
When suspicious login is detected, validate input, persist an audit record, and send notifications.
```

Then classify each step:

| Step type | Meaning | Default placement |
|---|---|---|
| Command | Writes state/data or triggers a business action | `application/use-cases` record + handler |
| Query | Reads only | `application/use-cases` record + handler |
| Domain Rule | Invariant, policy, or business decision | `domain` |
| Side Effect | External integration, IO, logging sink, DB, queue, mail | infrastructure adapter behind application contract |
| Orchestration | Step ordering across commands/queries | workflow |

If a step both reads and writes, split it into a Query then a Command unless the repo's concrete pattern says otherwise.

## Expected Vertical Slice Shape

For a business flow, prefer this shape when the repository has the matching folders:

```text
src/<Context>.Application/use-cases/<Business>BusinessUseCase.cs
src/<Context>.Application/handlers/<Business>BusinessUseCase.<Request>Handler.cs
src/<Context>.Application/validators/<Business><Request>Validator.cs
src/<Context>.Application/workflows/<Business>Workflow.cs
src/<Context>.Application/contracts/use-cases/I<Business>ExecutionUseCase.cs
src/<Context>.Application/services/<Business>ExecutionService.cs
src/<Context>.Application/behaviors/*Behavior.cs          # only for cross-cutting concerns
src/<Context>.Infrastructure/...                         # technical implementations
src/<Context>.Bootstrap/...                              # composition/DI wiring
src/<Context>.Controller/...                             # API/CLI/Console delivery
```

Clone the repo's existing canonical slice before inventing a new shape.

## Controller Layer

Controllers are delivery boundaries. API, CLI, console, workers, and similar entrypoints count as delivery controllers.

Allowed:

- Parse transport input.
- Build request/workflow input.
- Call one workflow or one application entrypoint.
- Convert application result into transport output.
- Use framework services required by the entrypoint.

Avoid:

- Direct repository access.
- Direct infrastructure implementation calls.
- Business decisions.
- Multi-step orchestration.
- Direct domain mutation.
- Injecting handlers directly.

Preferred shape:

```csharp
public Task ExecuteAsync(string triggeredBy, CancellationToken ct)
{
    return _workflow.ExecuteAsync(triggeredBy, ct);
}
```

A controller should be thin enough that moving the same flow from HTTP to CLI should not rewrite the business process.

## Workflow Layer

A workflow owns orchestration order.

Default rule:

```text
Workflow = dispatch-only orchestration.
```

Allowed:

- Inject `ISender` when the repo uses MediatR.
- Create concrete Command/Query records.
- Call `_sender.Send(new Request(...), ct)`.
- Sequence multiple commands/queries.
- Pass one step's result into the next step.

Avoid:

- Injecting repositories, infrastructure services, or arbitrary business services.
- Performing business calculations.
- Doing persistence or external IO directly.
- Returning transport-specific response objects.
- Hiding policy changes or configuration mutation inside the workflow.

Preferred shape:

```csharp
public Task ExecuteAsync(string triggeredBy, CancellationToken cancellationToken = default)
{
    return _sender.Send(new MonitoringBusinessUseCase.ExecuteCommand(triggeredBy), cancellationToken);
}
```

If orchestration gets large, keep the order visible in the workflow instead of moving hidden dispatch into handlers.

## UseCase Contract Layer

`application/use-cases/*BusinessUseCase.cs` is the request-contract layer.

Default rule:

```text
UseCase file = nested request records only.
```

Allowed:

- `public static partial class <Business>BusinessUseCase` or the local equivalent.
- Nested `record` types ending in `Command` or `Query`.
- Approved request interfaces such as `IRequest<T>` or repo-specific validation marker interfaces.
- Immutable request data.

Avoid:

- Handler logic.
- Service methods.
- Mutable state.
- Custom inheritance trees.
- External SDK or infrastructure references.
- Business orchestration.

Preferred shape:

```csharp
public static partial class MonitoringBusinessUseCase
{
    public sealed record ExecuteCommand(string TriggeredBy) : IRequireValidation<Unit>;
    public sealed record GetStatusQuery(string TriggeredBy) : IRequest<StatusDto>;
}
```

This layer keeps the command/query vocabulary stable and prevents controllers, handlers, and services from inventing competing request shapes.

## Handler Layer

A handler executes one Command or one Query.

Default rule:

```text
Handler = one unit of work, not a hidden workflow.
```

Allowed:

- Validate assumptions already expressed by the request contract.
- Call domain behavior.
- Call application contracts/ports.
- Call application services behind interfaces.
- Persist or read through abstractions.
- Return the request result.

Avoid:

- Injecting `ISender`, `IMediator`, or `IPublisher` unless the repo explicitly allows it.
- Calling `Send` or `Publish` to orchestrate another application request.
- Sequencing a full business flow.
- Calling controllers.
- Directly depending on infrastructure implementations.
- Catching exceptions only to hide failures.

Preferred shape:

```csharp
public sealed class ExecuteCommandHandler : IRequestHandler<ExecuteCommand, Unit>
{
    private readonly IMonitoringExecutionUseCase _execution;

    public ExecuteCommandHandler(IMonitoringExecutionUseCase execution)
    {
        _execution = execution;
    }

    public Task<Unit> Handle(ExecuteCommand request, CancellationToken cancellationToken)
    {
        _execution.Execute();
        return Task.FromResult(Unit.Value);
    }
}
```

If the code wants to call another command from a handler, move that sequencing up into a workflow.

## Validator Layer

Validators check request input before execution.

Allowed:

- Required fields.
- Format checks.
- Length/range checks.
- Simple cross-field input consistency.
- Returning validation messages/errors.

Avoid:

- Persistence.
- External API calls.
- Business orchestration.
- Domain mutation.
- Treating exceptions as normal validation flow.

Domain invariants still belong in domain objects. Validators are not a replacement for domain rules.

## Behavior / Pipeline Layer

Behaviors are for cross-cutting application concerns.

Allowed:

- Validation pipeline.
- Request logging.
- Exception logging.
- Timing.
- Correlation/trace IDs.
- Standard error mapping.

Avoid:

- Business-specific branching.
- Persistence decisions.
- Domain mutation.
- Swallowing exceptions without a clear application-level result.

If a behavior starts knowing too much about one use case, move that logic into the use case's handler or workflow.

## Application Contracts and Ports

Application contracts define stable boundaries.

Allowed:

- Interfaces under `application/contracts`.
- Ports consumed by handlers/services.
- Use-case abstractions.
- Logger, sanitizer, repository, clock, notification, and external integration abstractions.

Avoid:

- Infrastructure dependencies.
- Bootstrap dependencies.
- Controller/API dependencies.
- Concrete external SDK types leaking into application contracts unless the repo already standardizes on them.

Naming default:

```text
Interfaces under application contracts start with I.
Repository and port methods use Verb + Noun + Async when asynchronous.
```

Examples:

```text
IAppLogger
IRequestValidator<TRequest>
IMonitoringExecutionUseCase
GetActiveUserByAccountAsync
InsertPasswordResetTokenAsync
```

## Application Services

Application services implement use-case/application behavior behind contracts.

Allowed:

- Business-adjacent application logic.
- Calling application ports.
- Calling domain behavior.
- Coordinating small pieces inside one request execution.

Avoid:

- Becoming a god object.
- Replacing workflows for cross-request orchestration.
- Bypassing command/query contracts.
- Depending on infrastructure implementations directly.

Use an application service when a handler needs reusable application behavior but does not need a whole workflow.

## Domain Layer

Domain owns business concepts, invariants, and policies.

Allowed:

- Entities.
- Value objects.
- Domain services.
- Domain policies.
- Domain events.
- Business enums.
- Pure business validation.

Forbidden by default:

- `using MediatR`.
- Application dependencies.
- Infrastructure dependencies.
- Bootstrap dependencies.
- Controller/API/UI dependencies.
- Logging sinks, file systems, queues, mail clients, or external SDKs.

Preferred domain style:

```csharp
public PasswordResetToken Consume(DateTimeOffset now)
{
    if (ConsumedAt is not null)
        throw new TokenAlreadyConsumedException(Id);

    if (ExpiresAt < now)
        throw new TokenExpiredException(Id);

    ConsumedAt = now;
    return this;
}
```

Put business rules here instead of scattering them across controllers, repositories, or infrastructure helpers.

## Value Objects

Use value objects when a primitive has business meaning or validity rules.

Good candidates:

```text
EmailAddress
CompanyId
UserAccount
ResetToken
Money
DateRange
```

Defaults:

- Construct only valid values.
- Treat as immutable after creation.
- Use value-based equality when appropriate.
- Keep external services and IO out of value objects.

Do not wrap every primitive mechanically. Follow the repo's existing domain density.

## DTO Boundaries

DTOs are boundary objects.

Use DTOs for:

- API request/response shapes.
- Application input/output models.
- External system payloads.
- Projection results.

Avoid:

- Domain behavior inside DTOs.
- Sharing transport DTOs deep into domain.
- Mutating domain state through DTOs.

DTOs may be mutable for model binding when the repo uses that style. Domain/value objects should not become mutable just because DTOs are mutable.

## Infrastructure Layer

Infrastructure owns technical details.

Allowed:

- Repository implementations.
- EF Core / Dapper / SQL implementation.
- External API clients.
- File system adapters.
- Email/SMS/queue implementations.
- Logging implementation.
- Configuration implementation.
- Helpers, extensions, and constants under approved infrastructure paths.

Avoid:

- Business orchestration.
- MediatR request flow unless explicitly allowed by repo policy.
- Controller response logic.
- Domain decisions.
- Modifying application contracts casually to fit an infrastructure implementation.

Infrastructure implements application ports; application should not depend on infrastructure concrete types.

## Bootstrap / Composition Layer

Bootstrap wires the app.

Allowed:

- DI registration.
- Assembly scanning.
- Container configuration.
- Environment-specific wiring.
- Middleware registration.

Avoid:

- Business logic.
- Query logic.
- Domain rules.
- Runtime behavior that belongs in workflows or handlers.

Bootstrap may reference many layers for wiring, but that does not mean business code may do the same.

## Data Access Safety

After adding repository or query logic, check query shape.

Avoid:

- N+1 queries.
- N x M loops that execute queries inside nested loops.
- Accidental Cartesian products.
- Unbounded reads when a bounded query is expected.
- Raw SQL string interpolation.

Prefer:

- Parameterized SQL.
- Explicit transaction boundaries.
- Intent-revealing repository methods.
- Mapping near persistence code unless the repo has a mapper layer.

## Naming Defaults

Use names that describe business intent.

Preferred:

```text
ResetPasswordCommand
VerifyCustomerQuery
CreateBackupEmailCommand
GetCompanyUsersQuery
MonitoringBusinessUseCase
MonitoringWorkflow
MonitoringCommandValidator
ExecuteCommandHandler
```

Avoid:

```text
DoStuffCommand
ProcessDataQuery
ManagerService
Handler2
CommonHelper
```

Repository method names should usually be:

```text
Verb + Noun + Async
```

Examples:

```text
GetActiveUserByAccountAsync
InsertPasswordResetTokenAsync
ConsumeResetTokenAsync
ListBackupEmailsAsync
```

## Dependency Direction

Default direction:

```text
Controller/API/CLI
  -> Application Workflow
      -> Application Handler
          -> Domain
          -> Application Contracts
Infrastructure
  -> Application Contracts
  -> Domain when persistence mapping requires it
Bootstrap
  -> all layers only for composition
```

Forbidden by default:

```text
Domain -> Application
Domain -> Infrastructure
Domain -> Controller/API
Application Contracts -> Infrastructure
Application Contracts -> Bootstrap
Application Contracts -> Controller/API
Application -> Infrastructure implementation
Infrastructure -> Controller/API
Handler -> Controller
Controller -> Repository implementation
```

## Linter-Aware Editing Rules

When changing code:

1. Identify the touched layer before editing.
2. Place new code according to the existing vertical slice shape.
3. Prefer moving behavior to the correct layer over adding shortcuts.
4. Do not introduce new architecture concepts unless the repo already uses them or the change clearly requires them.
5. Do not weaken linter policy, allowlists, baselines, suppressions, or rule code to make findings disappear.
6. Run the repo linter when present.
7. Do not claim completion while the repo linter exits non-zero.

## Practical Placement Guide

| Code being added | Default placement |
|---|---|
| HTTP/CLI/Console entry | Controller layer |
| Multi-step business flow | Workflow |
| Command contract | `application/use-cases/*BusinessUseCase.cs` |
| Query contract | `application/use-cases/*BusinessUseCase.cs` |
| Single command execution | Handler |
| Single read operation | Query handler |
| Request validation | Validator |
| Cross-cutting validation/logging/exception pipeline | Behavior |
| Business invariant | Domain |
| Business value type | Domain value object |
| External API call | Infrastructure adapter behind application port |
| SQL / EF / Dapper implementation | Infrastructure repository |
| DI registration | Bootstrap/composition |
| Response/request shape | DTO boundary |
| Repository interface | Application contract/port |
| Repository implementation | Infrastructure |

## When Unsure

Use this question:

```text
If this same business flow moved from HTTP to CLI, which code should remain unchanged?
```

The unchanged code belongs in Application/Domain. The transport-specific code belongs in Controller/API/CLI. The technical implementation belongs in Infrastructure.
