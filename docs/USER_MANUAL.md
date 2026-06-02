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

## 4. Source-of-Truth
- Rule contracts: `docs/CONVENTIONS.md`
- Machine-enforced policy: `src/GenericDddLinter/linter.policy.sample.json`
- Flow process: `docs/FLOW_DECOMPOSITION_GUIDE.md`
- Canonical sample slice: `docs/SAMPLE_VERTICAL_SLICE.md`

## 5. MediatR Runtime Shape (Quick)
- `use-cases/*BusinessUseCase.cs`: request records only
- `handlers/*Handler.cs`: execution logic
- `validators/*Validator.cs`: request validation rules
- `behaviors/*Behavior.cs`: cross-cutting middleware
- `workflows/*Workflow.cs`: dispatch only (`ISender` + `new`)

## 6. Typical Workflow
1. Change code.
2. Run linter.
3. Fix findings.
4. Re-run until clean.
5. Build solution.
