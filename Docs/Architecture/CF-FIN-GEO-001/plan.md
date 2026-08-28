# Implementation Plan

1. Capture current contracts and add tenant-leakage tests; version or feature-flag the unsafe global clinic route.
2. Introduce module interfaces and dependency rules without extracting microservices.
3. Expand schema for location, discovery policy, receivables, provider references, inbox, and outbox.
4. Backfill clinic addresses asynchronously and review ambiguous results; add the spatial index.
5. Release tenant-safe nearby discovery and booking-time revalidation.
6. Migrate invoice/payment behavior onto Receivables through compatibility adapters.
7. Add `ClinicFlow.Worker` and idempotent callback processing.
8. Add Connect onboarding, direct payments, refunds, and reconciliation.
9. Add the provider-neutral CFDI lifecycle and selected adapter.
10. Contract unsafe legacy routes after telemetry and rollback windows.

