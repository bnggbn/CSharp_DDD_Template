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
- `PATH006`: `*Service` classes should stay in `domain/services` (pure domain services that return results).
- `CQRS100`: each command file must contain exactly one `*Command` and exactly one matching `IRequestHandler<ThatCommand, ...>`.
- `CQRS101`: each query file must contain exactly one `*Query` and exactly one matching `IRequestHandler<ThatQuery, ...>`.

## Mutation Guardrails
- `CFG001`: config override map assignment is allowed only in approved configuration paths.
- `CFG002`: writing `appsettings.json` is allowed only in approved configuration paths.
- `SEV001`: `SetSeverity` mutation is restricted to approved paths.
- `CONST001`: `*Constants` classes (in configured infrastructure paths) may only contain `const` or `static readonly` fields.

## Files
- `FILE001`: filename should match primary type.
- `ASCII001`: source paths should be ASCII-only.
