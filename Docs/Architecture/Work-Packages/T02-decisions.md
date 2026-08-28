# T02: Approved Platform Decisions

Status: Approved (human owner) — revised per product clarification (2026-08-27)

Approves the product, security, merchant, and provider-storage decisions that gate CF-FIN-GEO-001 tasks T03-T12. Decision-only: no production code or migration by this task.

> Revision note: supersedes the prior "eligibility gates discovery and blocks booking" reading of A1/A1b/A5. Per the human owner, `PatientAccount` exists solely to control which account (and its clinics) may access a patient's data together with the patient's consents — it never filters patient browsing, and booking auto-provisions the membership instead of being blocked by it.

## References

- `../CF-FIN-GEO-001/spec.md`
- `../CF-FIN-GEO-001/acceptance.md`
- `../CF-FIN-GEO-001/tasks.md`
- `../ADRs/ADR-002-financial-state-before-providers.md`
- `../ADRs/ADR-003-stripe-connect-direct-charges.md`
- `../ADRs/ADR-004-tenant-first-geospatial-discovery.md`
- `../Work-Packages/T01-baseline-clinic-security.md`

## Decisions accepted (human owner, A1-A7)

### A1 — PatientAccount scope and eligibility semantics

`PatientAccount` is staff access control: it determines which account (and its clinics) may view and attend a patient, in conjunction with the patient's per-account consents. It is **not** a discovery filter and never blocks booking.

Staff-access eligibility predicate: an account is eligible for a patient when it holds the patient's active (non-deleted) primary membership, **or** an active secondary membership where `IsVerifiedByPatient == true` AND `ConsentToShareProfile == true`.

### A1b — No-eligible-account behavior

Applies to staff access only. A patient with no eligible membership is not accessible to that account's staff unless the legacy-ghost clinic-link path applies (A6). Patient browsing is independent of eligibility (A5/A7): every active clinic within radius is browsable. A patient with no resolvable location still receives an actionable response, never a global fallback.

### A2 — Discovery radius policy

- System default effective radius: **25 km**.
- Valid radius range clamp: min **3 km**, max **100 km**. Values outside the clamp are coerced to the boundary.
- Override precedence (highest first): clinic-level override → account-level override → system default.
- Effective radius is computed once per request from the resolved patient context and clinic; behavior at the exact boundary is tested (inside = included, outside = excluded).
- Radius applies after the active-clinic predicate and location quality (browse) or after the tenant predicate (staff-scoped queries), per ADR-004.

### A3 — Connect merchant boundary and direct charges

- Each clinic (account) is its own Stripe Connect merchant via a connected account at the approved legal merchant boundary.
- Patient payment intents are created in connected-account scope (direct charges).
- ClinicFlow SaaS subscriptions remain on the platform Billing account (unchanged).
- Platform and Connect webhook streams are consumed as distinct, signature-verified, replay-safe streams (per ADR-001 worker and acceptance).
- The connected account id is persisted with every provider reference.
- **Merchant onboarding ownership:** the clinic staff is responsible for Connect onboarding; the patient portal offers guest checkout in v1 (no patient-side funding/saved cards).
- Merchant-of-record, fees, disputes, refunds, tax, and negative-balance responsibility remain legal/commercial items to be confirmed externally before T10 production go-live; recorded as caveats on ADR-003.

### A4 — Geospatial storage and provider boundary

- **ADR-004 accepted:** persist spatial data as SQL Server `geography` SRID 4326 with a spatial index; nearby searches use local SQL distance and never call Google per search; missing location produces an actionable response, never a global fallback.
- **Ordering:** "tenant-first" (resolve authorized accounts before radius) applies to **staff-scoped** queries. **Patient browse** applies active-clinic → location-quality → radius with **no membership predicate**.
- Clinic canonical geocode: persist the Google **Place ID** plus derived coordinates; Place ID is the canonical persisted identity.
- Exact patient coordinates are persisted **only on explicit patient opt-in** (consent); opt-out deletes stored coordinates.
- Google Places/Geocoding storage obligations are honored (persist Place IDs/derived data permitted under Maps terms; no prohibited caching or address scraping).
- All provider access sits behind an adapter contract (e.g., `IGeocodingProvider`); the platform never calls Google directly from search/booking paths.

### A5 — Booking and membership auto-provisioning

- Booking at a clinic whose owning account is not linked to the patient **auto-provisions** the `PatientAccount` membership if missing (primary if the patient has none, otherwise secondary).
- Patient consent is required **per account** for (a) medical records and (b) WhatsApp, in both booking scenarios: patient-initiated (portal) and staff-initiated.
- The consent-capture mechanism and the timing of the verified/consent flags are a **defined gap** ("el hueco"), anticipated in the Booking Self-Service task (T02c).
- The former "no-primary bypass" in `AppointmentService` is **intended behavior, not a bug**: the control is consent, not a membership block.

### A6 — Unified staff-access predicate

One eligibility predicate (A1) is applied consistently by the **staff-facing** layers:

- `PatientAccessHandler` rule 3 (record access),
- the single-tenant (account) branch of the global EF query filters for `Patient` and derived entities,

The legacy `PatientClinics` clinic-link fallback is **demoted**: it grants staff access only for **legacy ghosts** — patients with no eligible membership anywhere — and is scheduled for removal after the linking window.

### A7 — Patient discovery and share-link booking flow

- **Discovery:** an authenticated patient browses **every active clinic within the effective radius regardless of membership**. Geographic browse lands with T04/T05/T07; the current membership-scoped endpoint (`GET /api/patient/clinics`) remains as "mis clínicas" legacy.
- **Booking:** patient- or staff-initiated at any clinic; membership auto-provisioned (A5); consent required (gap).
- **Share link:** staff generate a URL with **clinic + specialist pre-filled**; the patient chooses **only date and time**.
  - With an open session: books directly.
  - With an account but no session: logs in, then books.
  - No account: enters **only name + phone** → a ghost patient is registered → picks date/time → booking fires and auto-provisions the membership → a **unique access-token link** is sent (WhatsApp/email) so the patient completes registration (password + missing data) after booking.
- Unique-token mechanics (issue, expiry, replay) and consent capture in the anonymous path belong to the defined gap (T02c).

## Production data impact note

- Prod patients 2/3/4 hold a single verified-and-consented membership (account 3002) → unaffected by A1/A6.
- Prod ghost patients 1002/2002/2003 have no `PatientAccount` → not staff-accessible per account until linked (T02b backfill); the legacy-ghost clinic-link path (A6) keeps current clinic staff able to reach them meanwhile.
- No migration is required by this task; schema additions land in T03.

## Contract impact

- Browsing contract for patients: radius endpoint (T07). `/api/patient/clinics` stays membership-scoped as "mis clínicas".
- Booking: **no block**; membership auto-provisioned (T02c).
- Staff access: unified A1 predicate (T02b) with legacy-ghost fallback.

## Task linkage

- T02b — Align the staff-access predicate (A6) and relink ghosts 1002/2002/2003.
- T02c — Booking self-service and share link (A5/A7; consent and token gap).
- T03 — Add expand-only schema and module interfaces under these decisions.
- T04/T05/T07 — Geocoding, geography query, radius clamp, and patient discovery UX per A2/A4/A7.
- T10 — Connect payment/refund/webhook adapters per A3.