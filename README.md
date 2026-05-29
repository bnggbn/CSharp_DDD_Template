# DddLinterSkillKit

A DDD/CQRS starter template with a policy-driven architecture linter.

## Structure
- `src/DddStarter.Domain`: domain entities, value objects, enums
- `src/DddStarter.Application`: contracts and use-case implementations
- `src/DddStarter.Infrastructure`: logging, database abstractions, repository implementations
- `src/DddStarter.Controller`: API/CLI/Console entry controllers
- `src/DddStarter.Bootstrap`: DI/composition and Autofac container wiring
- `src/GenericDddLinter`: reusable linter (regex rules + Roslyn rules)
- `ddd-architecture-linter-skill`: skill package and references

## Build
```powershell
dotnet build DddLinterSkillKit.slnx
```

## Run Linter
```powershell
dotnet src/GenericDddLinter/bin/Debug/net10.0/GenericDddLinter.dll src src/GenericDddLinter/linter.policy.sample.json
```

## Key Features
- DDD-friendly layering and naming constraints
- CQRS file-level rules (`CQRS100`, `CQRS101`)
- Config/severity mutation guardrails
- NLog with sanitizer abstraction before file output
- Autofac assembly-marker scanning for registration

## Notes
- Policy can be customized in `src/GenericDddLinter/linter.policy.sample.json`.
- Rule definitions are documented in `ddd-architecture-linter-skill/references/rule-catalog.template.md`.