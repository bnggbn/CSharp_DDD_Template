# Conventions (Source-of-Truth Indexed)

This document is organized by **who guarantees the rule**.

## Principles (Human-Reviewed)
| Principle | Enforced By |
|---|---|
| Domain must not depend on infrastructure details. | `DEP001`/`DEP002` in generated lint contracts |
| State transitions should produce next values (no boundary-crossing mutation). | `IMM001` for request contracts; code review for wider domain mutation boundaries |
| Workflow is dispatch-only (`ISender` + `new`). | `FLOW001`/`FLOW002`/`FLOW003`/`CTRL001` + sample slice |

## Lint-Enforced Contracts (Machine-Guaranteed)
Canonical source is policy, and this doc does not duplicate rows manually.
- Generated mirror: `docs/LINT_RULES.generated.md`
- Policy source: `src/GenericDddLinter/linter.policy.sample.json`
- Rule runner implementation: `src/GenericDddLinter/RoslynRuleRunner.cs`, `src/GenericDddLinter/RegexRuleRunner.cs`

## Conventions (Not Lint-Enforced)
| Convention | Why |
|---|---|
| Repository method naming: `Verb + Noun + Async`. | Consistent discoverability |
| Define contracts first (`contracts/use-cases`, `contracts/ports`). | Stable boundaries before implementation |
| Run repository query-shape checks (N+1, N x M). | Prevent accidental performance regressions |

## Sample Vertical Slice
Canonical source:
- `docs/SAMPLE_VERTICAL_SLICE.md`
