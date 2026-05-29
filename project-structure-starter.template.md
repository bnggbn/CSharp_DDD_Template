# DDD Project Structure Starter

## Included in this kit

1. `src/GenericDddLinter`: policy-driven lint console template.
2. `src/DddStarter.Domain`: domain layer starter.
3. `src/DddStarter.Application`: application layer starter.
4. `src/DddStarter.Infrastructure`: infrastructure layer starter.
5. `src/DddStarter.Bootstrap`: DI/composition starter.
6. `src/DddStarter.Controller`: unified controller starter for API/CLI/Console entry.

## Suggested boot order

1. Rename project/namespace from `DddStarter.*` to your bounded context.
2. Copy and tune `src/GenericDddLinter/linter.policy.sample.json`.
3. Add project-specific rules (CFG/SEV/PATH/FILE).
4. Wire your app entrypoint in bootstrap.
