---
name: clinicflow-platform
description: Apply ClinicFlow platform invariants and the spec-driven workflow to cross-cutting tenancy, scheduling, geolocation, payments, fiscal-document, and worker tasks.
---

# ClinicFlow Platform Skill

Use this skill for work that crosses modules or affects tenancy, patient booking, money, provider callbacks, geolocation, migrations, or deployment topology.

Before acting, read the nearest `AGENTS.md`, the active task, the accepted spec, linked ADRs, relevant migrations, and current implementation. Search for reusable contracts before creating new ones.

Identify the owning module and source of truth, enumerate trust boundaries, define contract and failure states, confirm migration/rollback, and map acceptance evidence. Do not implement a cross-cutting change without an accepted spec task.

Core invariants:

- Distance narrows authorized clinics; it never authorizes them.
- Nearby searches use persisted SQL spatial data, not repeated Google calls.
- Subscription billing and patient clinical payments are different bounded contexts.
- Receivables exist independently of Stripe and CFDI.
- Payment and fiscal-document state machines remain separate.
- Appointment creation revalidates eligibility and slot availability atomically.

Return affected modules/files, contracts, migration/security impact, tests, and explicit decisions still needed.

