# Coding Style and CQRS/DDD Behavior Standards

## Naming and Readability
- Use clear names aligned with DDD semantics.
- Add focused comments only for business logic decisions.

## Repository and CQRS Naming
- Use `Verb + Noun + Async` for repository method names.
- Use `Verb + Noun + Command/Query` for command/query names.
- Use `record` for commands and queries; do not use inheritance for them.

## Command Dispatch Pattern
- Create the command object first, then call `_sender.Send(command, ct)`.

## Data Access Safety
- After implementing repository logic, re-check ORM queries and avoid N+1 patterns.
- Also avoid N x M Cartesian-product style queries.

## DTO and VO Boundaries
- Build DTOs in the application layer for transport concerns; services may adjust DTO content.
- Build VOs in the service layer; once created, treat them as immutable/read-only.

## Validation and Exception Handling
- For data-fix requests, validate first and handle via normal logic flow, not by falling into exception flow.
- After an exception occurs, confirm middleware catches it.
- If middleware catches it, do not mutate data inside exception handlers.
- If middleware does not catch it, log the error and exception details.