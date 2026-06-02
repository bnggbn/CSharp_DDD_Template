# Rule Catalog Template

## Dependency
- `DEP001`: domain must not depend on application/infrastructure/bootstrap/controller.
- `DEP002`: application contracts must not depend on infrastructure/bootstrap/controller.
- `MEDIATR001~002`: MediatR usage is limited to approved scope (application/controller allowed, domain/infrastructure forbidden).
- `DIP001`: constructor dependencies in target layers should depend on interfaces (DIP), not concrete classes.
- `MOCK001`: contract/abstraction interfaces must have at least one mock/fake/stub implementation in approved paths.

## Naming
- `NAME001`: interfaces must start with `I`.
- `NAME002`: use case implementations must end with `UseCase`.
- `VO000`: value object classes should end with `Vo`.
- `INFH001`: infrastructure helper classes should end with `Helper` (under `infrastructure/helpers`).
- `INFX001`: infrastructure extension classes should end with `Extensions` (under `infrastructure/extensions`).
- `INFC001`: infrastructure constant classes should end with `Constants` (under `infrastructure/constants`).

## Placement
- `PATH001`: monitor classes should stay in `domain/monitors`.
- `PATH002`: use-case classes should stay in `application/use-cases`.
- `PATH003`: `*Helper` classes should stay in `infrastructure/helpers`.
- `PATH004`: `*Extensions` classes should stay in `infrastructure/extensions`.
- `PATH005`: `*Constants` classes should stay in `infrastructure/constants`.
- `CQRS100`: `*Command` must be declared as `record` and nested in `*BusinessUseCase` under `application/use-cases/`.
- `CQRS101`: `*Query` must be declared as `record` and nested in `*BusinessUseCase` under `application/use-cases/`.
- `CQRS102`: files under `application/use-cases/` must be `*UseCase.cs`; class names must end with `UseCase`; class members must be `record *Command/*Query` only.

## Mutation Guardrails
- `CFG001`: config override map assignment is allowed only in approved configuration paths.
- `CFG002`: writing `appsettings.json` is allowed only in approved configuration paths.
- `SEV001`: `SetSeverity` mutation is restricted to approved paths.
- `CONST001`: `*Constants` classes (in configured infrastructure paths) may only contain `const` or `static readonly` fields.
- `IMM001`: command/query records must be positional or init-only; no mutable members.
- `IMM002`: domain entities/VOs must not expose public setters for boundary-crossing state.
- `PURE001` (optional): handlers must not mutate inbound request records.

## Files
- `FILE001`: filename should match primary type.
- `ASCII001`: source paths should be ASCII-only.
