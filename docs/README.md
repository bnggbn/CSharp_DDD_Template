# DddLinterSkillKit Docs

## Documents
- `FLOW_DECOMPOSITION_GUIDE.md`: how to split one business flow into DDD/CQRS slices.
- `USER_MANUAL.md`: quick user manual.

## Project Overview
- `src/DddStarter.Domain`: domain entities, value objects, enums.
- `src/DddStarter.Application`: contracts and use-case implementations.
- `src/DddStarter.Infrastructure`: logging, database abstractions, repositories.
- `src/DddStarter.Controller`: API/CLI/Console entry controllers.
- `src/DddStarter.Bootstrap`: DI/composition and Autofac container wiring.
- `src/GenericDddLinter`: reusable linter (regex rules + Roslyn rules).
- `ddd-architecture-linter-skill`: skill package and rule references.

## Build
```powershell
dotnet build DddLinterSkillKit.slnx
```

## Run Linter
```powershell
dotnet src/GenericDddLinter/bin/Debug/net10.0/GenericDddLinter.dll src src/GenericDddLinter/linter.policy.sample.json
```

## Important Lint Rules
- `DIP001`: constructor dependencies in application/controller/bootstrap should use interfaces.
- `MOCK001`: interfaces under contracts/abstractions should have mock/fake/stub implementations.
- `CQRS100`: one command + one corresponding handler per command file.
- `CQRS101`: one query + one corresponding handler per query file.
