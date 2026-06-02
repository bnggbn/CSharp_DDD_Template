# Lint Rules (Generated)

Generated from policy: src/GenericDddLinter/linter.policy.sample.json.

| RuleId | Description |
|---|---|
| ASCII001 | Source file paths must be ASCII-only. |
| CFG001 | Config override assignment is restricted to approved paths. |
| CFG002 | Settings file write is restricted to approved paths. |
| CONST001 | Constants classes may only contain const or static readonly fields. |
| CQRS100 | Command records must be nested in '*BusinessUseCase' under application/use-cases. |
| CQRS101 | Query records must be nested in '*BusinessUseCase' under application/use-cases. |
| CQRS102 | Use-case files must be '*UseCase.cs' and contain only record command/query members. |
| DEP001 | Domain layer must not depend on application/infrastructure/bootstrap/controller namespaces. |
| DEP002 | Application contracts must not depend on infrastructure/bootstrap/controller namespaces. |
| DIP001 | Constructor dependencies in target layers must prefer interfaces. |
| FILE001 | File name should match the primary declared type. |
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
| SEV001 | SetSeverity mutation is restricted to approved paths. |
| VO000 | Domain value-object classes should end with 'Vo'. |
