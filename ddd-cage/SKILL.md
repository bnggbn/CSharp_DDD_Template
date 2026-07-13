---
name: ddd-cage
# This skill is an agent contract, not a linter-authoring guide.
description: Use when modifying .NET code in a repository with DDD/CQRS or architecture-linter signals, such as Domain/Application/Infrastructure projects, Commands/Queries/Handlers/Workflows, MediatR request handlers, thin controllers, concrete linter policy files, or rule catalogs. This skill makes Codex/GPT detect the repo's actual architecture rules, run the repo linter when one exists, and refuse to claim completion while the linter exits non-zero. Do not use for pure legacy .NET maintenance when no DDD/CQRS or repo-linter signal exists.
---

# DDD Cage: Codex/GPT Contract

This skill is a behavior contract for Codex/GPT while editing C#/.NET code in DDD/CQRS-shaped repositories. It is not primarily a guide for creating a reusable linter.

The goal is simple: when the repository already has an architecture gate, step on that gate every time relevant code changes. Do not silently weaken the gate to make the work appear complete.

## Activation Contract

Use this skill when the task touches `.cs`, `.csproj`, `.sln`, test projects, dependency injection, controllers, handlers, workflows, repositories, domain models, value objects, DTOs, validators, or infrastructure adapters **and** at least one of these signals is present:

- Projects or folders named like `*.Domain`, `*.Application`, `*.Infrastructure`, `*.Controller`, `*.Api`, `*.Bootstrap`, `Application`, `Domain`, `Infrastructure`, `Controllers`, `Workflows`, `Handlers`, `Commands`, or `Queries`.
- CQRS/MediatR code such as `IRequest`, `IRequestHandler`, `Command`, `Query`, `Handler`, `Validator`, `Behavior`, `Workflow`, `*BusinessUseCase`, `application/use-cases`, `application/workflows`, or `_sender.Send(...)`.
- Existing architecture-linter assets such as a concrete policy file, rule catalog, linter project, linter script, `GenericDddLinter`, `linter.policy.sample.json`, `LINT_RULES.generated.md`, `ddd-lint`, `architecture-lint`, `linter-policy`, `rule-catalog`, or similar repo-specific enforcement files.
- A change that moves code across application/domain/infrastructure/controller boundaries.

Do **not** activate the full DDD contract for pure legacy maintenance when the repository has no DDD/CQRS or linter signal. Examples: isolated WebForms bug fixes, old .NET Framework page/event-handler edits, WinForms maintenance, stored-procedure wiring, or one-off compatibility fixes that do not touch DDD/CQRS boundaries.

If a repo-specific linter exists, run it for relevant .NET changes even if the DDD/CQRS shape is weak. The committed linter is an explicit repository gate.

## Source of Authority

The authority order is:

1. The repository's committed concrete policy, rule catalog, scripts, CI config, and documentation.
2. Existing code structure and established local conventions.
3. This skill's references.
4. Templates in this skill.

The files under `references/*.template.*` are schema and vocabulary aids for policy authors. They are not law. In cage/contract mode, the law is the concrete policy already committed in the target repository.

## Mandatory Linter Gate

Before declaring the task complete:

1. Discover the repo's linter invocation.
2. Run the linter if one exists.
3. Treat exit code `0` as clean.
4. Treat any non-zero exit code as **not complete**.
5. Fix code findings and re-run until the linter exits `0`, or report that the task is blocked.

Never claim the implementation is complete, clean, done, or ready when the linter exits non-zero.

If the linter cannot run because of missing dependencies, broken local tooling, permission issues, or environment problems, do not call the task clean. Report the exact command attempted, the failure mode, and what remains unverified.

## Linter Invocation Discovery

Find the repo's linter by checking, in order:

1. User instructions in the current task.
2. Repo documentation such as `README*`, `CONTRIBUTING*`, `docs/*`, or architecture docs.
3. CI/pre-commit definitions such as `.github/workflows/*`, `.gitlab-ci.yml`, `azure-pipelines.yml`, `Jenkinsfile`, `.pre-commit-config.yaml`, `lefthook.yml`, or `husky` hooks.
4. Build scripts such as `Makefile`, `justfile`, `build.ps1`, `build.sh`, `eng/*`, `scripts/*lint*`, `scripts/*architecture*`, or `tools/*`.
5. .NET tool manifests such as `.config/dotnet-tools.json`.
6. Linter projects or commands with names like `ArchitectureLinter`, `DddLinter`, `Architecture.Tests`, `ArchUnit`, `NetArchTest`, `lint-architecture`, `ddd-lint`, or `rule-catalog`.

Prefer the repository's documented command over inventing a command.

If multiple commands exist, run the most specific architecture/DDD linter first, then broader test/build gates if the task also requires them.

## Policy Immutability Rule

Do not modify concrete linter policy, rule catalog, allowlists, baselines, suppression files, or linter logic merely to remove findings from your own code changes.

Default behavior is deny-by-default:

- Fix the code, not the policy.
- Do not relax dependency rules, naming rules, mutation allowlists, path rules, or CQRS rules to make the linter green.
- Do not add suppressions/baselines for newly introduced violations.
- Do not silently edit templates or policy files as part of a feature/refactor task.

Policy changes are allowed only when the user explicitly asks for a policy/rule change or explicitly approves one after you propose it. Keep policy changes separate from implementation fixes and label them clearly as policy changes.

If a real false positive appears, stop treating it as a normal code fix. Report:

- The rule id.
- The finding.
- Why it appears to be a false positive.
- The exact policy/rule change that would be required.
- That human approval is needed before changing the gate.

## DDD/CQRS Behavior Expectations

Follow the repository's concrete policy first. When the repo does not specify a rule, use these defaults:

- Domain should not depend on application, infrastructure, bootstrap, controller, API, UI, or framework-specific transport concerns.
- Application may define use cases, workflows, commands, queries, ports/contracts, validators, and behaviors.
- Business/domain services live in the **domain** layer (`domain/services/`, `*Service`). They are pure: they compute and **return a result** (typically a value object) and must not log, decide persistence, or reference application/infrastructure.
- `application/use-cases/*BusinessUseCase.cs` should normally contain request records only: nested `Command`/`Query` contracts, not execution logic.
- Infrastructure implements application contracts and owns external systems, persistence details, file systems, logging sinks, and adapters. Infrastructure-internal abstractions (that only infrastructure consumes) belong in infrastructure (e.g. `infrastructure/<area>/abstractions/`), not in `application/contracts`.
- `application/contracts/ports` should expose only interfaces the application actually calls (e.g. logger, validator, repository/port abstractions).
- Controllers stay thin: parse input, build request, call a workflow or application entry point, and return a response.
- Workflows own orchestration and step order; in MediatR-shaped repos they should usually be dispatch-only through `ISender`.
- Handlers execute one command/query. They **orchestrate side effects** (logging, persist/skip decisions, notifications) based on the result returned by domain services, but should not dispatch other commands.
- Commands and queries remain contract-shaped immutable request records, not business-service containers.
- Cross-cutting validation, logging, and exception handling belong in behaviors/middleware unless the repo's concrete pattern says otherwise.
- DTOs are transport/application boundary objects. Value objects belong to domain or business logic and should be treated as immutable once constructed.

## Required Work Loop

For each relevant task:

1. Check activation signals and avoid overloading pure legacy work.
2. Identify the repo's concrete policy and linter command, if present.
3. Read only the references needed for the touched area.
4. Modify code according to repo rules and local conventions.
5. Run the required linter gate if it exists.
6. If the linter exits non-zero, fix code findings and re-run.
7. Do not alter policy/rules/suppressions unless explicitly authorized.
8. Report verification honestly.

## Required Output Format

When returning work on a DDD/CQRS or architecture-linted .NET repo, include these sections in order:

1. `Summary`
2. `Activation`
3. `Findings`
4. `Applied Changes`
5. `Verification`
6. `Blocked / Needs Approval` only when applicable
7. `Next Actions` only when useful

`Verification` must include the exact linter command run and its result. If no repo linter exists, say `repo linter: not found` and describe the manual checks performed. Do not present manual checks as equivalent to a passing linter.

## Exit-Code Done Condition

The task is not done while any required repo linter exits non-zero.

Use this language precisely:

- Exit code `0`: linter gate passed.
- Non-zero exit code with findings: implementation not complete; findings remain.
- Non-zero exit code from tool/runtime failure: verification blocked; implementation cannot be claimed clean.
- Linter not found: linter gate unavailable; report manual checks and residual risk.

## Structural Limit

This skill is a soft cage. It can guide Codex/GPT to behave correctly, but it is not itself enforcement.

The hard cage is a linter, pre-commit hook, CI job, or external runner outside the agent's control. This skill's value is to force the agent to step on that hard gate whenever it exists, and to prevent the agent from quietly loosening the gate to pass.
