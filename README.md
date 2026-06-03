# DddLinterSkillKit

DDD/CQRS starter template with a policy-driven architecture linter.

## Why
- Preserve dependency direction (Domain/Application depend on abstractions, not infrastructure concrete types).
- Keep state/data flow explicit (`next = transform(current)` at boundaries).
- Keep request flow predictable: `use-cases` defines request records, handlers execute logic, workflows dispatch.

## Add One Flow (Recipe)
1. Define request records in `src/DddStarter.Application/use-cases/*BusinessUseCase.cs`.
2. Implement handler logic in `src/DddStarter.Application/handlers/`.
3. Add validators in `src/DddStarter.Application/validators/` when needed.
4. Dispatch from `src/DddStarter.Application/workflows/` via `_sender.Send(new Request(...), ct)`.
5. Run linter and build.

## Sample Slice (Clone This)
- `docs/SAMPLE_VERTICAL_SLICE.md`

## Build
```powershell
dotnet build DddLinterSkillKit.slnx
```

## Run Linter
```powershell
dotnet src/GenericDddLinter/bin/Debug/net10.0/GenericDddLinter.dll src src/GenericDddLinter/linter.policy.sample.json
```

## Rule/Contract Reference
- `docs/CONVENTIONS.md` (principle vs lint vs convention)
- `src/GenericDddLinter/linter.policy.sample.json` (machine-enforced source of truth)
