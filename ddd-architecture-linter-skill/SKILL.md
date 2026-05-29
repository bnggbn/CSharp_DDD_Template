---
name: ddd-architecture-linter
description: Create or evolve a reusable DDD-focused architecture linter for .NET repositories, including dependency direction rules, naming conventions, path placement rules, mutation guardrails, and policy-driven templates. Use when teams want to standardize clean architecture/DDD constraints across projects.
---

# DDD Architecture Linter

Build and maintain a policy-driven linter for DDD architecture.

## Workflow

1. Read linter policy first.
2. Run linter against target repository.
3. Report findings as `[RuleId] file: message`.
4. Patch policy or rules.
5. Re-run until clean.

## Reference Loading Rules

- Read `references/linter-policy.template.json` when creating or updating rule configuration.
- Read `references/rule-catalog.template.md` when aligning rule IDs, semantics, and documentation.
- Read `references/project-structure.template.txt` when validating DDD folder boundaries and layer placement.
- Read `references/coding-style-guidelines.md` when enforcing CQRS naming, repository method style, DTO/VO boundaries, and exception-handling behavior.

## Rules to Prioritize

1. Dependency direction (`DEP*`)
2. Naming constraints (`NAME*`, `VO*`)
3. Sensitive mutation allowlist (`CFG*`, `SEV*`)
4. Path and file constraints (`PATH*`, `FILE*`, `ASCII*`)
5. CQRS request-handler constraints (`CQRS*`)

## Required Output Format

When returning lint work, always output sections in this order:

1. `Summary`
2. `Findings` (one line per finding: `[RuleId] /relative/path: message`)
3. `Applied Changes` (files created/edited)
4. `Verification` (commands run and result)
5. `Next Actions` (optional)

If no findings exist, output `Findings: none`.

## Output Contract

- Keep rule ids stable.
- Return deterministic text output.
- Fail process with non-zero exit code when violations exist.