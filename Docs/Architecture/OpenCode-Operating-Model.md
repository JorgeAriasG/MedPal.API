# OpenCode Operating Model

## Responsibility split

| Role | Owns | Must not do |
|---|---|---|
| Human owner | Approvals for architecture, security, contracts, providers, migrations, merge, and production | Delegate irreversible or high-impact decisions without review |
| Codex architect/orchestrator | Specs, ADRs, task boundaries, decision support, architecture/security review | Routine product implementation, testing, or deployment execution |
| OpenCode architect agent | Repository analysis and design review against accepted artifacts | Implement without an accepted task |
| OpenCode implementation agents | Development and targeted refactoring within one task | Expand scope or invent cross-module contracts |
| OpenCode QA agent | Tests, verification evidence, regression and risk reporting | Change production behavior to satisfy tests |
| OpenCode deployment work | Approved deployment automation and evidence | Run production changes without explicit human approval |

## Required work-package fields

- Task ID and objective.
- Accepted spec and ADR links.
- Owning repository/module and dependencies.
- Allowed and forbidden paths.
- Functional, security, migration, and observability acceptance criteria.
- Required commands and evidence.
- Explicit stop conditions.

## Execution cycle

1. Run `/analyze-change` or the equivalent agent command.
2. Resolve decisions and accept/update the spec and ADRs.
3. Select exactly one work package.
4. Run `/implement-spec` with the appropriate implementation agent.
5. Run `/verify-scope` plus the QA agent.
6. Present the diff, commands, results, risks, and unresolved items to the human owner.
7. Merge only after approval; production actions require a separate explicit approval.

## Evidence contract

OpenCode reports:

- Files changed and why.
- Tests/builds run with pass/fail results.
- Security, contract, migration, and deployment impact.
- Acceptance criteria mapped to evidence.
- Assumptions, untested areas, and follow-up tasks.

Token efficiency comes from loading only the active spec/task and relevant code, keeping agents specialized, and preventing repeated rediscovery through canonical ADRs and repository rules.

