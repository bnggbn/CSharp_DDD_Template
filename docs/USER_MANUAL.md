# User Manual (Quick)

## 1. Build
```powershell
dotnet build DddLinterSkillKit.slnx
```

## 2. Run the Linter
```powershell
dotnet src/GenericDddLinter/bin/Debug/net10.0/GenericDddLinter.dll src src/GenericDddLinter/linter.policy.sample.json
```

## 3. Read Findings
- Output format: `[RuleId] file: message`
- If no issues: `Lint passed: no issues found.`

## 4. Key Rules to Tune
- `DIP001` (`constructorInterfaceRule`): enforce constructor dependencies on interfaces.
- `MOCK001` (`interfaceMockRule`): require mock/fake/stub implementations for selected interfaces.
- `CQRS100` / `CQRS101`: command/query file composition checks.
- `INFH001` / `PATH003`: helper naming + placement in infrastructure.
- `INFX001` / `PATH004`: extension naming + placement in infrastructure.
- `INFC001` / `PATH005`: constants naming + placement in infrastructure.
- `CONST001` (`constantsClassRule`): constants classes can only contain `const` or `static readonly` fields.

## 5. MediatR Middleware Stack (Template Default)
- Validator: implement `IRequestValidator<TRequest>` in application layer.
- Validation middleware: `ValidationBehavior<TRequest,TResponse>` runs before handlers.
- Exception middleware: `UnhandledExceptionBehavior<TRequest,TResponse>` logs and rethrows.
- Entry dispatch: controllers call `_sender.Send(command, ct)`.

Recommended scenarios:
- Use validator for request contract checks.
- Use behaviors for cross-cutting (validation, logging, exception boundary).
- Keep business orchestration in handlers/use-cases, not in controllers.

## 6. Update Rules
- Policy: `src/GenericDddLinter/linter.policy.sample.json`
- Rule catalog: `ddd-architecture-linter-skill/references/rule-catalog.template.md`
- Coding conventions: `ddd-architecture-linter-skill/references/coding-style-guidelines.md`

## 7. Typical Workflow
1. Change code.
2. Run linter.
3. Fix findings.
4. Re-run until clean.
5. Build solution.
