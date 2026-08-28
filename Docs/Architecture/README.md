# ClinicFlow Architecture Workbench

This directory is the canonical source for cross-repository ClinicFlow architecture decisions and accepted implementation specifications.

## Operating model

- The human owner approves key architecture, security, public-contract, provider, migration, and production decisions.
- Codex acts as architect/orchestrator: it prepares or reviews specs, ADRs, task boundaries, and evidence.
- OpenCode owns product development, testing, and deployment execution from accepted tasks.
- Project-local agent files may summarize these decisions but must not create conflicting copies.

## Workflow

1. Analyze the request and identify owning modules.
2. Accept or update the spec and linked ADRs.
3. Select one task with explicit allowed paths and acceptance criteria.
4. Let OpenCode implement and verify it.
5. Review the diff, security/migration impact, and evidence.
6. Obtain human approval before merging or performing production actions.

The first implementation spec is [`CF-FIN-GEO-001`](CF-FIN-GEO-001/spec.md).

