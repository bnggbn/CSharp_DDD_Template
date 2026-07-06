# Invocation Contract Template

Use this template only when a repository does not already document how to run its architecture linter. Prefer committed repo documentation and CI scripts first.

## Policy Location

Document the concrete policy file used by the repo, for example:

- `architecture/linter-policy.json`
- `tools/ArchitectureLinter/policy.json`
- `docs/architecture/rule-catalog.md`
- `tests/Architecture.Tests/*`

## Command

Document the exact command, working directory, and expected runtime, for example:

```bash
# from repository root
dotnet test tests/Architecture.Tests/Architecture.Tests.csproj
```

or:

```bash
# from repository root
dotnet tool restore
dotnet tool run ddd-lint -- --policy architecture/linter-policy.json --root .
```

## Exit Codes

- `0`: no architecture violations.
- non-zero with findings: violations remain; implementation is not complete.
- non-zero from tool/runtime failure: verification is blocked; do not claim the repo is clean.

## Policy Change Rule

Do not modify policy, allowlists, baselines, suppressions, or linter implementation to make unrelated code changes pass. Policy changes require explicit human approval and should be submitted separately from implementation fixes.
