# Acceptance Criteria

- A patient browses every active clinic within the effective radius regardless of account membership; booking auto-provisions the account link and per-account consent gates data access.
- An authorized clinic outside the effective radius is excluded; exact-boundary behavior is tested.
- Missing/ambiguous location produces an actionable response, never a global clinic fallback.
- Repeated discovery and booking attempts do not call Google unless an approved address refresh is required.
- Booking atomically ensures the account membership (auto-provisioned when missing) and consent exist while re-validating radius, clinic/doctor relationship, and slot overlap.
- Payment/refund callbacks are signature-verified, deduplicated, replay-safe, and tolerate out-of-order delivery.
- Internal receivable totals reconcile to successful payments and refunds.
- CFDI failure never rewrites payment success; it enters retry or manual review.
- Geocode backfill is observable, restartable, quota-aware, and non-blocking.
- Legacy routes are tenant-safe adapters or disabled after the migration window.
- Core API, worker, staff UI, patient API, and patient UI builds/tests pass with migration rollback evidence.

