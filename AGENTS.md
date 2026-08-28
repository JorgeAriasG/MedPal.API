# ClinicFlow Core API Agent Rules

## Role in the platform

This repository is the source of truth for ClinicFlow domain behavior. It is an ASP.NET Core .NET 8 API using EF Core 8 and SQL Server. The current architecture is a layered monolith (`Controllers -> Services -> Repositories -> Data`) evolving toward a modular monolith plus a separately deployed worker.

The staff application and patient portal consume this domain. Channel-specific backends may orchestrate, but must not duplicate financial, tenant, clinic-discovery, or booking rules.

## Human and agent responsibilities

- The human owner approves architecture, security boundaries, migrations, public contracts, provider choices, and deployment changes.
- The architect agent prepares or reviews specs and ADRs. It does not implement product code unless explicitly requested.
- OpenCode implementation agents own development, tests, and deployment work only through accepted task files.
- No task is complete without build/test evidence and a concise security/migration impact report.

## Non-negotiable invariants

1. Derive tenant and patient identity from authenticated claims/context. Never trust caller-supplied IDs as authority.
2. Tenant authorization precedes every business filter. Distance never grants access; it only narrows an already authorized clinic set.
3. Controllers handle HTTP concerns. Domain decisions belong in application/domain services; persistence belongs behind module ports or current repository seams.
4. Preserve DTO mapping, async I/O, soft delete, audit, and authorization conventions.
5. ClinicFlow receivables, payments, refunds, and fiscal documents are the source of truth. Stripe and CFDI vendors are adapters.
6. Keep ClinicFlow SaaS subscriptions separate from patient-to-clinic clinical payments.
7. Verify external callback signatures, persist inbox identity, deduplicate, acknowledge quickly, and process asynchronously.
8. Use expand/backfill/verify/contract migrations. Do not combine destructive schema changes with a feature rollout.
9. Store money in integer minor units plus ISO currency in new financial models.
10. Never return all clinics when patient location or eligibility is missing.

## Required workflow

For cross-cutting work, read the accepted spec and linked ADRs under `Docs/Architecture` before editing. Work on one task ID at a time. Report affected files, commands, results, risks, and any deviation from the spec. Stop when a task requires an unapproved contract, tenant rule, provider, destructive migration, or production action.

## Verification

- Build the solution/project and run relevant .NET tests.
- Add integration tests for tenant isolation and persistence behavior.
- For scheduling: verify overlap and concurrent booking behavior.
- For geolocation: test inside/outside/exact radius and missing/ambiguous locations.
- For payments: test idempotency, duplicates, out-of-order callbacks, partial payments, and refunds.

