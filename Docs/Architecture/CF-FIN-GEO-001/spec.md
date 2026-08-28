# CF-FIN-GEO-001: Financial and Nearby Clinic Platform Foundation

Status: Proposed - architecture reviewed; product/legal decisions remain

## Problem

ClinicFlow currently combines SaaS subscriptions, clinical invoices/payments, public clinic listing, and patient booking across the monolithic API and patient facade. Public/patient clinic queries can enumerate all clinics and do not enforce proximity. Financial state lacks a durable provider-neutral callback and reconciliation foundation.

## Goals

- Establish `Geolocation`, `Receivables`, `Payments`, `FiscalDocuments`, and `Integration` module boundaries inside the existing API deployable.
- Add a separately deployed `ClinicFlow.Worker` for outbox dispatch, inbox processing, geocoding, reconciliation, and CFDI work.
- Preserve Stripe Billing for ClinicFlow SaaS subscription fees.
- Add Stripe Connect direct charges for patient-to-clinic payments behind an adapter.
- Resolve an address only when created, selected, corrected, or deliberately refreshed; never call Google for each nearby search or booking attempt.
- Persist permitted spatial data and use SQL Server geography plus a spatial index for proximity queries.
- Return clinics only after account/tenant eligibility, active state, radius, and optional availability filters.

## Non-goals

- Microservice extraction.
- Replacing either Angular application.
- Replacing current identity in the first phase.
- Continuous patient-device tracking.
- Selecting a final CFDI vendor before evaluation.

## Authorization and discovery rule

`EligibleClinics = ActiveClinics INTERSECT AllowedAccounts(patient) INTERSECT Radius(origin, effectiveRadius) INTERSECT BookableAvailability(optional window)`

Distance is never authorization. Recommended account eligibility is the active primary PatientAccount plus verified and consented secondary PatientAccounts. Product and security must approve the final semantics. PatientClinic may restrict or prioritize clinics but cannot expand account scope.

## Effective radius

Resolve account/clinic policy override, then account default, then system default; clamp to approved minimum and maximum. The provisional default is 25 km and requires product approval. SQL distance uses meters and deterministic `<=` boundary behavior.

## Google usage and storage

Use Places Autocomplete/Place Details or Geocoding only during address capture/correction/approved refresh. Persist the Place ID indefinitely where permitted and store other provider-derived data only according to the applicable Google Maps Platform terms. Keep the provider behind an adapter so storage/refresh behavior can change without affecting discovery. Nearby searches operate exclusively on persisted SQL spatial data.

## API sketch

- `POST /api/v2/locations/resolve`
- `PUT /api/v2/clinics/{clinicId}/location`
- `PUT /api/v2/patients/me/location`
- `GET /api/v2/patient/clinics/nearby`
- `GET /api/v2/patient/clinics/{clinicId}/availability`
- `POST /api/v2/patient/appointments`
- `POST /api/v2/receivables/{id}/payment-session`
- `POST /api/v2/refunds`
- `POST /api/webhooks/stripe/platform`
- `POST /api/webhooks/stripe/connect`
- `POST /api/webhooks/cfdi/{provider}`

## Data model direction

- `Address`: structured and normalized address fields.
- `GeoLocation`: SQL geography point, interchange coordinates, Place ID, provider, quality/status, and verification timestamps.
- `DiscoveryPolicy`: account/clinic scope, radius defaults/limits, membership mode, and location-quality requirements.
- `Receivable` and `ReceivableLine`: internal amount due and lifecycle.
- `PaymentAttempt`, `Payment`, `Allocation`, and `Refund`: provider-neutral money state.
- `FiscalDocument` and `FiscalDocumentAttempt`: CFDI lifecycle and provider references.
- `InboxMessage` and `OutboxMessage`: durable, idempotent integration processing.

## State machines

- Receivable: Draft -> Open -> PartiallyPaid -> Paid; open balances may be Void; paid balances may become PartiallyRefunded/Refunded.
- Payment attempt: Created -> RequiresAction/Processing -> Succeeded or Failed/Expired/Cancelled.
- Fiscal document: NotRequested -> Pending -> Stamping -> Stamped or Rejected/Cancelled.
- Geocode: Missing -> Pending -> Verified/Approximate/Ambiguous/Failed/Stale.

## Safety

- No global-clinic fallback when location is missing.
- Patient identity comes from the authenticated principal.
- Exact patient coordinates are not exposed without approved purpose and policy.
- Webhooks require raw-body signature verification, inbox deduplication, rate/body limits, and asynchronous processing.
- New monetary values use integer minor units plus ISO currency.
- Booking atomically revalidates tenant eligibility, radius policy, clinic/doctor relationship, and slot overlap.

## Observability

Track geocode success/ambiguity, discovery latency and zero results, authorization denials, webhook lag/replay, payment reconciliation variance, CFDI latency/failure, outbox age, retries, and dead letters. Do not log raw patient addresses.

