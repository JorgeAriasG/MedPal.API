# T01: Baseline Clinic Discovery Security

Status: Implemented (approved by human owner; decisions D1-D3 accepted) — see "Implementation evidence" below.

## Objective

Capture current clinic-discovery behavior and design the smallest versioned or feature-flagged change that prevents a patient from enumerating clinics outside authorized accounts. This task begins with analysis and tests; it must not introduce geocoding, Stripe, CFDI, or module movement.

## References

- `../CF-FIN-GEO-001/spec.md`
- `../CF-FIN-GEO-001/acceptance.md`
- `../ADRs/ADR-004-tenant-first-geospatial-discovery.md`

## Initial allowed paths

- Clinic controllers, services, repositories, DTOs, authorization code, and their tests.
- Patient facade clinic controller/service and their tests, in its own repository.
- Architecture/task documentation when findings require clarification.

## Forbidden scope

- Stripe, subscriptions, payments, invoices, CFDI, or provider credentials.
- Google APIs or coordinate persistence.
- Production deployment.
- Destructive migrations.
- Broad module refactoring.

## Analysis questions

1. Which current endpoints can enumerate all clinics, and who calls them?
2. How is authenticated patient identity bound to `PatientAccount` and `PatientClinic` today?
3. Which membership attributes are enforced versus only stored?
4. What backward compatibility is required for staff or public booking?
5. Where should the tenant-safe query live so the patient facade does not duplicate it?

## Required test design

- Authorized primary-account clinic is visible.
- Unauthorized account clinic is denied or excluded.
- Deleted membership/clinic is excluded.
- Patient-ID substitution cannot change scope.
- Missing eligible membership never falls back to all clinics.

## Stop conditions

Stop and request a decision if implementation requires choosing final secondary-membership semantics, breaking an existing public contract, adding a destructive migration, or changing production configuration.

## Required output

Return an analysis report with cited paths, proposed contract, affected files, test plan, compatibility/security impact, and `Ready`, `Needs decision`, or `Blocked`. Do not implement until the human owner accepts that report.

## Implementation evidence

### Decisions accepted (human owner, D1-D3)

- **D1** `Discovery:AllowAnonymousPublicClinics` defaults to `false` (secure-off). Global clinic exposure must not be the default; unresolved eligibility returns an empty result or a controlled 401/404, never all clinics.
- **D2** Patient discovery is a dedicated endpoint `GET /api/patient/clinics`. `GET /api/Clinic/all` is retained only behind the flag for the compatibility window, then retired. The patient portal must not depend on `/all`.
- **D3** T01 eligible accounts = the patient's primary plus any active (non-deleted) `PatientAccount` memberships in the current data model. Broader secondary-account semantics are deferred to CF-FIN-GEO-001 T02.

### Analysis findings (supporting decisions)

- `GET /api/Clinic/all` (`Controllers/ClinicController.cs`) was `[AllowAnonymous]` and called the no-arg `GetAllClinicsAsync()` → `_context.Clinics.Where(c => !c.IsDeleted)` → **all clinics**. The global tenant filter never stopped it: anonymous principals and patient tokens both yield an empty tenant snapshot (`Data/AppDbContext.cs`), so `HasContext=false` passes every row.
- No consumer of `/all` exists in the staff UI (`scheduling.ui` uses `GET /api/clinic` via `ClinicService.getClinics()`; there is no patient portal in any repo). Changing it breaks nothing shipped.
- Patient identity is bound server-side: the patient JWT carries `patient_id` + `user_type=patient` (`Controllers/PatientAuthController.cs`); tenancy is resolved from `PatientAccount` (primary/verified/consent) and `PatientClinic` links, never from caller-supplied IDs.
- Enforcement gap found (follow-up, out of scope): `AppointmentService` booking gate skips the verified-membership check when the patient has no primary membership ("no-primary bypass"). Saved for future tasking alongside aligning query filters with `IsVerifiedByPatient`.

### Contract implemented

- `GET /api/patient/clinics` — authenticated patient only. Resolves `patient_id` claim (missing/invalid → 401). Returns clinics whose `AccountId` is in the patient's non-deleted `PatientAccount` membership set (primary + any active), excluding clinics without an `AccountId`. No eligible memberships → `200 []` (no global fallback).
- `GET /api/Clinic/all` — returns legacy behavior only when `Discovery:AllowAnonymousPublicClinics == true`; otherwise `404`. Default in `appsettings.json` and `appsettings.Development.json` is `false`.
- Staff `GET /api/clinic`, `GET /api/clinic/{id}`, and the staff UI are unchanged.

### Affected paths

- `MedPal.API/Repositories/IClinicRepository.cs` — added `GetPatientClinicsAsync(int patientId)`.
- `MedPal.API/Repositories/Implementations/ClinicRepository.cs` — patient-scoped membership-join query.
- `MedPal.API/Controllers/ClinicController.cs` — new `[HttpGet("~/api/patient/clinics")]`; `/all` flag gate; `IConfiguration` injected.
- `MedPal.API/appsettings.json`, `appsettings.Development.json` — `Discovery:AllowAnonymousPublicClinics: false`.
- `MedPal.API.Tests/Controllers/ClinicControllerTests.cs` — 7 tests (claim handling, empty result, flag on/off, repo delegation).
- `MedPal.API.Tests/Data/ClinicRepositoryTests.cs` — 4 InMemory EF tests (primary+secondary, deleted membership, no membership, non-primary ghost).
- `MedPal.API.Tests/MedPal.API.Tests.csproj` — added `Microsoft.EntityFrameworkCore.InMemory` 8.0.8.

### Verification

- `dotnet build` → 0 errors.
- Test suite → **89/89 pass** (78 prior + 11 new).
- `dotnet ef migrations has-pending-model-changes` → no model changes (no migration in this task).
- Live smoke (local `MedPalDBDev`, `https` profile): anonymous `GET /api/Clinic/all` → 404; anonymous `GET /api/patient/clinics` → 401; patient token (account 1) → exactly its account's non-deleted clinics (clinics 1,2; clinic 1008 with null account excluded); patient with **no** membership (patient 57) → empty list; authenticated `GET /api/Clinic/all` → 404.

### Compatibility/security impact

- Removes anonymous global clinic enumeration (secure default ON deployment; no production config write required — flag added to repo defaults as `false`).
- Breaks the previously-anonymous `/all` public contract only in the "flag off" state; acceptable since no shipped consumer depends on it (D2).
- No new exposure, no destructive migration, no Stripe/CFDI/geo code touched.
