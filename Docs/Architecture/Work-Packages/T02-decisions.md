# T02: Approved Platform Decisions

Status: Approved (human owner)

Approves the product, security, merchant, and provider-storage decisions that gate CF-FIN-GEO-001 tasks T03-T12. This task is decision-only: no production code, no migration, no configuration change.

## References

- `../CF-FIN-GEO-001/spec.md`
- `../CF-FIN-GEO-001/acceptance.md`
- `../CF-FIN-GEO-001/tasks.md`
- `../ADRs/ADR-002-financial-state-before-providers.md`
- `../ADRs/ADR-003-stripe-connect-direct-charges.md`
- `../ADRs/ADR-004-tenant-first-geospatial-discovery.md`
- `../Work-Packages/T01-baseline-clinic-security.md`

## Decisions accepted (human owner, A1-A6)

### A1 — Patient-account eligibility semantics

Eligible accounts for a patient's clinic discovery and booking reference are:

- the patient's active (non-deleted) primary `PatientAccount`, plus
- any active secondary `PatientAccount` where `IsVerifiedByPatient == true` AND `ConsentToShareProfile == true`.

An unverified or non-consented membership is not eligible. This supersedes T01 D3 ("primary plus any active membership") as the authoritative cross-account rule and aligns with the `PatientAccessHandler` rule 3.

### A1b — No-eligible-account behavior

When a patient has no eligible account, discovery MUST return an empty result with a controlled, actionable payload ("no verified membership — verify your membership to see clinics"), and must never fall back to all clinics. Unknown or missing identity produces 401 (not 200/404). This satisfies acceptance criterion "Missing/ambiguous location produces an actionable response, never a global clinic fallback."

### A2 — Discovery radius policy

- System default effective radius: **25 km**.
- Valid radius range clamp: min **3 km**, max **100 km**. Values outside the clamp are coerced to the boundary.
- Override precedence (highest first): clinic-level override → account-level override → system default.
- Effective radius is computed once per request from the resolved patient context and clinic; behavior at the exact boundary is tested (inside = included, outside = excluded).
- Radius applies after the eligibility and active-clinic predicates (tenant-first, per ADR-004).

### A3 — Connect merchant boundary and direct charges

- Each clinic (account) is its own Stripe Connect merchant via a connected account at the approved legal merchant boundary.
- Patient payment intents are created in connected-account scope (direct charges).
- ClinicFlow SaaS subscriptions remain on the platform Billing account (unchanged).
- Platform and Connect webhook streams are consumed as distinct, signature-verified, replay-safe streams (per ADR-001 worker and acceptance).
- The connected account id is persisted with every provider reference.
- **Merchant onboarding ownership:** the clinic staff is responsible for Connect onboarding; the patient portal offers guest checkout in v1 (no patient-side funding/saved cards).
- Merchant-of-record, fees, disputes, refunds, tax, and negative-balance responsibility remain legal/commercial items to be confirmed externally before T10 production go-live; recorded as caveats on ADR-003.

### A4 — Geospatial storage and provider boundary

- **ADR-004 accepted:** persist spatial data as SQL Server `geography` SRID 4326 with a spatial index; resolve patient-authorized accounts before location-quality, radius, and availability predicates; nearby searches use local SQL distance and never call Google per search; missing location produces an actionable response, never a global fallback.
- Clinic canonical geocode: persist the Google **Place ID** plus derived coordinates; Place ID is the canonical persisted identity.
- Exact patient coordinates are persisted **only on explicit patient opt-in** (consent); opt-out deletes stored coordinates.
- Google Places/Geocoding storage obligations are honored (persist Place IDs/derived data permitted under Maps terms; no prohibited caching or address scraping).
- All provider access sits behind an adapter contract (e.g., `IGeocodingProvider`); the platform never calls Google directly from search/booking paths.

### A5 — Booking membership gate (no-primary bypass)

The current `AppointmentService` booking gate only enforces verification when a primary membership exists and differs from the destination account; a patient with no primary membership can book at any clinic ("no-primary bypass").

Decision: **block booking** at any clinic where the patient has no eligible membership (per A1). An unlinked patient must be linked (staff-side clinic assignment or portal self-verification) before the booking is accepted. Ghost patients created from staff booking flows continue to auto-link to the booking clinic as primary (existing behavior).

### A6 — Unified eligibility predicate across access paths

One eligibility predicate (per A1) is used consistently by:

- patient-facing clinic discovery queries,
- `PatientAccessHandler` staff record access,
- the booking gate (replacing the verified-only `HasVerifiedMembershipAsync` check).

The legacy `PatientClinics` clinic-link fallback is demoted to a **legacy-ghost-only, deprecated** path: it may not grant eligibility for discovery or booking; a patient with only clinic links has no eligible account (A1b applies).

## Production data impact note

- Prod patients 2/3/4 each hold a single verified-and-consented membership (account 3002) → unaffected by A1 narrowing.
- Prod ghost patients 1002/2002/2003 have no `PatientAccount` → not eligible under A1 until linked. T02b links their target accounts and restores visibility/booking.
- No migration is required by this task; schema additions land in T03.

## Contract impact

- Patient discovery `GET /api/patient/clinics`: membership rule narrows from "any active membership" to A1 eligibility; empty → actionable payload (A1b).
- Booking reference: gate now blocks unlinked bookings (A5); enforced in T02b.
- Staff visibility and record access: aligned to the single eligibility predicate (A6).

## Task linkage

- T02b — Enforce A5/A6 (booking gate + unified predicate, deprecate clinic-link fallback) and relink ghosts 1002/2002/2003. Queued as the next task.
- T03 — Add expand-only schema and module interfaces under these decisions.
- T04 — Geocoding adapter/backfill per A4.
- T05 — Geography query and radius clamp per A2/A4.
- T10 — Connect payment/refund/webhook adapters per A3.