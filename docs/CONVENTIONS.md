# Conventions (Source-of-Truth Indexed)

This document is organized by **who guarantees the rule**.

## Principles (Human-Reviewed)
| Principle | Enforced By |
|---|---|
| Domain must not depend on infrastructure details. | `DEP001`/`DEP002` in generated lint contracts |
| State transitions should produce next values (no boundary-crossing mutation). | `IMM001` for request contracts; code review for wider domain mutation boundaries |
| Workflow is dispatch-only (`IDispatcher` + `new`). | `FLOW001`/`FLOW002`/`FLOW003`/`CTRL001` + sample slice |
| Services are pure domain logic; they return results and never log or decide persistence. | `PATH006` (`*Service` under `domain/services/`) + `DEP001` (domain cannot reach `IAppLogger`) |
| Application (handlers) orchestrates logging and the persist/skip decision from the returned result. | Sample slice + code review |
| Only application-facing ports live in `contracts/ports`; infrastructure-internal abstractions live in infrastructure. | `DEP002` + convention (see below) |

## Lint-Enforced Contracts (Machine-Guaranteed)
Canonical source is policy, and this doc does not duplicate rows manually.
- Generated mirror: `docs/LINT_RULES.generated.md`
- Policy source: `src/GenericDddLinter/linter.policy.sample.json`
- Rule runner implementation: `src/GenericDddLinter/RoslynRuleRunner.cs`, `src/GenericDddLinter/RegexRuleRunner.cs`

## Conventions (Not Lint-Enforced)
| Convention | Why |
|---|---|
| Repository method naming: `Verb + Noun + Async`. | Consistent discoverability |
| Define application-facing ports first in `contracts/ports`. | Stable boundaries before implementation |
| Keep only application-consumed interfaces in `contracts/ports`; put infrastructure-only abstractions under `infrastructure/<area>/abstractions/`. | Contracts expose just what the application calls; keep the abstraction next to its implementation |
| Domain services return result value objects; the calling handler orchestrates logging/persistence. | Keep domain pure and side-effect free; centralize orchestration in the application layer |
| Run repository query-shape checks (N+1, N x M). | Prevent accidental performance regressions |

## Sample Vertical Slice
Canonical source:
- `docs/SAMPLE_VERTICAL_SLICE.md`
