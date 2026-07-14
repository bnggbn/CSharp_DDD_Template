# Lint Rules (Generated)

Generated from policy: src/GenericDddLinter/linter.policy.sample.json.

| RuleId | Description |
|---|---|
| ASCII001 | Source file paths must be ASCII-only. |
| BUILD001 | Lint is build-gated: the repository must compile before rule evaluation is trusted. |
| CFG001 | Config override assignment is restricted to approved paths. |
| CFG002 | Settings file write is restricted to approved paths. |
| CONST001 | Constants classes may only contain const or static readonly fields. |
| CQRS100 | Command records must be nested in '*UseCase' under application/use-cases. |
| CQRS101 | Query records must be nested in '*UseCase' under application/use-cases. |
| CQRS102 | Use-case files must be '*UseCase.cs' and contain only record command/query members. |
| CQRS103 | Command/query records must not use custom inheritance; only approved request interfaces are allowed. |
| CTRL001 | Controller constructors must stay in workflow/framework dependency scope. |
| DEP001 | Domain layer must not depend on application/infrastructure/bootstrap/controller namespaces. |
| DEP002 | Application contracts must not depend on infrastructure/bootstrap/controller namespaces. |
| DIP001 | Constructor dependencies in target layers must prefer interfaces, except approved configuration-style concrete types. |
| FILE001 | File name should match the primary declared type. |
| FLOW001 | Workflow constructors may only depend on ISender. |
| FLOW002 | Workflow methods may not call injected dependencies other than '_sender.Send(...)'. |
| FLOW003 | Handlers must not orchestrate by depending on mediator sender/publisher or dispatching nested requests. |
| IMM001 | Command/query records must remain immutable: init-only properties and readonly fields only. |
| INFC001 | Infrastructure constants classes should end with 'Constants'. |
| INFH001 | Infrastructure helper classes should end with 'Helper'. |
| INFX001 | Infrastructure extension classes should end with 'Extensions'. |
| MEDIATR001 | MediatR usage is forbidden in the domain layer. |
| MEDIATR002 | MediatR usage is forbidden in the infrastructure layer. |
| MOCK001 | Key interfaces must have at least one mock/fake/stub implementation. |
| NAME001 | Interfaces under application contracts must start with 'I'. |
| NAME002 | Classes under application/use-cases must end with 'UseCase'. |
| PATH001 | Classes matching '*Monitor' must be placed under '/monitors/'. |
| PATH002 | Classes matching '*UseCase' must be placed under '/use-cases/'. |
| PATH003 | Classes matching '*Helper' must be placed under '/infrastructure/helpers/'. |
| PATH004 | Classes matching '*Extensions' must be placed under '/infrastructure/extensions/'. |
| PATH005 | Classes matching '*Constants' must be placed under '/infrastructure/constants/'. |
| PATH006 | Classes matching '*Service' must be placed under '/domain/services/'. |
| SEV001 | SetSeverity mutation is restricted to approved paths. |
| VO000 | Domain value-object classes should end with 'Vo'. |
