# ADR-001: Modular monolith plus worker

Status: Proposed

Keep one transactional SQL Server database and one API deployable initially. Introduce module-owned tables, internal ports, dependency rules, and integration events. Add a separately deployed worker using the same application/module assemblies for retryable asynchronous work. This preserves current deployment simplicity while creating enforceable extraction boundaries.

