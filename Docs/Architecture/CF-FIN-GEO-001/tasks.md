# Tasks

- [x] T01 Capture endpoint/database contract tests and cross-account denial tests.
- [x] T02 Approve patient-account eligibility, radius, Connect merchant boundary, and provider-storage decisions.
- [x] T02b Align the staff-access predicate (A6): eligible-membership filter/handler, account-based staff roster, demote clinic-link fallback to legacy ghosts; relink ghosts 1002/2002/2003 (memberships + clinic links backfilled, live-verified).
- [ ] T02c Booking self-service and share link (A5/A7): membership auto-provisioning, name+phone ghost path, staff share URL, unique-token registration completion.
- [ ] T03 Add expand-only schema and module interfaces.
- [x] NOTE (deuda técnica, Fase posterior): Múltiples clases por archivo preexistentes en `ArcoController.cs` (ArcoRequestDto), `ConsentController.cs` (ConsentGrantDTO, ConsentCheckDTO) e `InvoiceController.cs` (UpdateInvoiceStatusRequest). Mover cada tipo a su propio archivo (DTOs/ o requests) en tarea de limpieza. Librar el criterio "1 tipo por archivo" en todo código nuevo.
- [ ] T04 Implement geocoding adapter, durable job, restartable backfill, and review state.
- [ ] T05 Implement tenant-first SQL geography query, spatial index, and radius clamp.
- [ ] T06 Version patient discovery, availability, and booking endpoints.
- [ ] T07 Add patient/staff location UX with consent and manual fallback.
- [ ] T08 Implement Receivables and legacy invoice/payment compatibility.
- [ ] T09 Add inbox/outbox and `ClinicFlow.Worker` deployment artifacts.
- [ ] T10 Implement Connect onboarding, payment, refund, webhook, and reconciliation adapters.
- [ ] T11 Implement CFDI port, provider adapter, retry/poll/manual-review flows.
- [ ] T12 Execute backfill verification, canary flags, dashboards, and legacy contract cleanup.

