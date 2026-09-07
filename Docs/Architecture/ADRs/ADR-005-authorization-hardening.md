# ADR-005: Authorization hardening — permission + scope over role policies

Status: Proposed (owner review required; implementation agents must not apply until accepted)

Replace the current role-assertion authorization model with a permission-catalog model enforced through fine-grained policies plus scope handlers (Account, Clinic, Assigned, Self). Keep the seven seeded system roles, but express what they may do as concrete permission grants — never as inline `RequireAssertion` role switches in `Program.cs` — and normalize the JWT identity contract so the frontend and backend agree on who is calling. SuperAdmin becomes a platform-administration role without routine clinical access; clinical data is reachable only through assignment or explicit consent (per `../Work-Packages/T02-decisions.md` A1/A6), and deletion of clinical records is granted to no default role (break-glass only).

## Context

The audited state (this branch, 2026-09-06) has three coexisting and inconsistent authorization layers:

1. **30 fine-grained permissions** seeded in `Data/Seeders/AuthorizationSeeder.cs` (7 roles, grants 30/30/30/15/8/10/5) and registered as policies in `Program.cs:250-293` (`Patients.*`, `Appointments.*`, `MedicalRecords.*`, `Billing.*`, `Users.*`, `Reports.*`, `Clinics.*`, `Roles.*`).
2. **8 role-assertion policies** inline in `Program.cs:296-411` (`ViewUsersPolicy`, `ViewPatientsPolicy`, `ViewAppointmentsPolicy`, `ManageUsersPolicy`, `ManagePatientsPolicy`, `ViewAuditLogPolicy`, `AdministerAccountPolicy`, `AdministerClinicPolicy`) and duplicated dead code in `Authorization/Policies/AuthorizationPoliciesExtension.cs` (never called; contains `Console.WriteLine` debug noise at lines 169-190).
3. **Hand-rolled checks in controllers** that mix fine permissions, role policies, resource handlers, and raw claim reads with inconsistent results.

Audit findings that motivate this ADR (all verified on this branch):

- `Controllers/AppointmentsController.cs:89-90` double-gates the create-appointment endpoint with `Appointments.Create` + `ManagePatientsPolicy`, and `ManagePatientsPolicy` excludes `Receptionist` — causing 403s for the very role the model says must book appointments.
- `Controllers/MedicalHistoryController.cs:146` uses `[Authorize(Roles = "Admin")]`; no `Admin` role exists among the 7 seeded roles, so clinical-record deletion is unreachable for everyone (403), which is inconsistent with an explicit authorization design.
- 5 controllers are 100% anonymous because they inherit `ControllerBase` directly with no `[Authorize]`: `InvoiceController`, `PaymentController`, `EmergencyContactController`, `NotificationMessageController`, `WaitlistController` — full CRUD on invoices/payments/contacts/notifications and waitlist signup without authentication.
- `Services/TokenService.cs:92-97` emits `role=Patient` + `ClaimTypes.Role=User` when the user has no roles (insecure fallback); additionally `Controllers/UserController.cs:165` falls back to `HealthProfessional` in the login DTO.
- `Enums/SystemRole.cs` omits `Nurse` (=5): seeding uses the name `"Nurse"` but the enum jumps `HealthProfessional=4 → Receptionist=6`, so role comparisons against the enum silently fail for the Nurse role.
- `Repositories/Authorization/PermissionRepository.cs:85-102` checks only `UserRole.ExpiresAt`; it ignores `Role.IsActive` and `Role.IsDeleted`, both of which exist on `Models/Authorization/Role.cs:32-42`.
- There is no `FallbackPolicy` on `AddAuthorizationBuilder` (`Program.cs:248`); the pipeline is `UseAuthentication → UseAuthorization → MapControllers` (`Program.cs:477-479`), so anonymous controllers are reachable by anybody.
- Staff tokens (`TokenService.cs:57-62,65-88`) and patient tokens (`Controllers/PatientAuthController.cs:203-212`) diverge: staff emit `sub`+`user_id`+`account_id`+`clinic_id`; patients emit `patient_id`+`user_type` but no `sub`. Because the default JWT outbound/inbound claim maps serialize `ClaimTypes.Role` as `"role"` in the raw payload (colliding with the custom `"role"` claim), multi-role users risk being reduced to a single `"role"` value after round-tripping; the contract must make claim names deterministic.

## Decision

### D1 — Permission + Scope is the enforcement model

- A single **permission catalog** (55 permissions). Each endpoint is authorized by exactly one permission policy; no endpoint is authorized by a role-assertion policy or `[Authorize(Roles=…)]`.
- Every permission is paired with a **scope handler** chosen from {Account, Clinic, Assigned, Self}; the same permission name may be granted to several roles, and the scope handler decides which rows the caller may reach (account boundary = tenant, clinic = sub-scope, assigned = `HealthcareProfessional`/`Nurse` assignments plus `PatientAccount` membership per T02 A1, self = `sub`/`patient_id`).
- The seven system roles keep their names and seeding order; `SystemRole` enum is fixed to include `Nurse = 5`.
- `SuperAdmin` is a platform administration role: Users, Roles, Reports, Audit, Clinics, Subscriptions, FiscalSettings, Invoices/Payments management, and `Patients.Delete` (data governance). It has **no** `Patients.*` view scope, no `Appointments.*`, no `ClinicalRecords.*`, no `Vitals.*`, no `Prescriptions.*` by default. Break-glass clinical access is a future mechanism requiring explicit reason, expiry, and audit — out of scope for F1-F5 beyond its design note.

### D2 — FallbackPolicy and an explicit public allow-list

- Add `FallbackPolicy = authenticated` to `AddAuthorizationBuilder` in `Program.cs` so every endpoint without `[AllowAnonymous]` or an explicit `[Authorize]` is denied.
- Keep exactly the public surface anonymous with explicit `[AllowAnonymous]` (16 endpoints enumerated in `Controller-Policy-Map.md`): staff/patient auth and registration, guest booking, checkout session lookup, public subscription plans, pharmacy prescription validation, flag-gated clinic discovery, and the two signed webhooks. Webhook controllers additionally get explicit `[AllowAnonymous]` — today they are anonymous by omission, not by attribute.
- `Controllers/WaitlistController.cs` signup is a public lead-capture form; recommended explicit `[AllowAnonymous]` + rate limiting, pending owner confirmation (not part of the original allow-list).

### D3 — Normalized JWT claims contract

- Adopt the contract in `Authorization/Jwt-Claims-Contract.md`: `sub`, `user_id`, `account_id` (when scoped), `clinic_id` (when > 0), `patient_id` (patients only), `user_type = "staff" | "patient"`, `role` (first role, legacy UI), and `roles` (array, canonical). Use `JwtSecurityTokenHandler.DefaultMapInboundClaims = false` and explicit `NameClaimType`/`RoleClaimType` so claim names are deterministic and multi-role is preserved.
- Tokens with no roles are **not emitted**: login returns 403 "no roles assigned". The insecure fallback (`TokenService.cs:94-97`) and the login-DTO fallback (`UserController.cs:165`) are removed.
- Patients must receive `sub` = `patient_id` and no `account_id`/`clinic_id` (tenant resolved server-side via `PatientAccount`, per T02 A1).

### D4 — Policy consolidation

- The 8 legacy role policies are removed from `Program.cs`; during the compatibility window (F3) they are re-registered as **aggregates of fine permissions** with the same names, so existing `[Authorize(Policy = "…Policy")]` attributes keep working. They are deleted in F4.
- `Authorization/Policies/AuthorizationPoliciesExtension.cs` is deleted (dead code).
- `PermissionRepository.UserHasPermissionAsync` additionally filters `Role.IsActive == true` and `Role.IsDeleted == false` (and aliases legacy names during F3).

### D5 — Matrix and invariants

- The target grants are defined in `Authorization/Permission-Matrix.md` and enforced by an idempotent, versioned seeder. Invariants I1-I6 are codified as automated conformance tests per role.

### D6 — Scope semantics

- Account = tenant boundary (multi-tenancy); Clinic = sub-scope; HealthcareProfessional and Nurse assignments define the Assigned scope; patient portal uses Self.
- Staff access to a patient continues to resolve through `PatientAccessHandler` per T02 A1/A6 (eligible `PatientAccount` memberships + per-account consent; legacy-ghost clinic-link fallback only for legacy ghosts).

## Consequences

- Positive: single, greppable authorization model; role checks stop leaking into controllers; 5 anonymous controllers become protected by default; the Receptionist booking 403 disappears (D4 + `AppointmentsController.cs:90` fix); the fake `Roles = "Admin"` deletion path becomes an explicit, role-less permission.
- Negative: any client that today calls the 5 anonymous controllers, or relies on role-less users getting a Patient token, breaks (see Breaking changes). The 8 legacy policy names become inert after F4, requiring controller attribute updates to be completed by then.
- Cost: a new scope-handler layer, a permissions re-seed, alias resolution during F2-F4, and expanded authorization tests (matrix conformance + scope cases).

## Breaking changes

1. **Role-less tokens**: users with no roles can no longer log in (403). Today they silently become `Patient`/`User`.
2. **Anonymous controllers become authenticated**: `InvoiceController`, `PaymentController`, `EmergencyContactController`, `NotificationMessageController` return 401/403 for unauthenticated callers after F2. `WaitlistController` is the only one kept public (explicit `[AllowAnonymous]`, pending owner confirmation).
3. **JWT claim normalization**: `ClaimTypes.Role` is replaced by the `roles` array contract; clients reading the raw payload lose the current implicit `"role"`/`nameid` behavior. The UI must decode the new contract (espejo: `roles[]`, `sub`/`user_id`, `patient_id`).
4. **Legacy policy removal**: after F4, `ViewUsersPolicy`, `ViewPatientsPolicy`, `ViewAppointmentsPolicy`, `ManageUsersPolicy`, `ManagePatientsPolicy`, `ViewAuditLogPolicy`, `AdministerAccountPolicy`, `AdministerClinicPolicy` no longer exist; any controller still referencing them breaks compilation/startup.
5. **Orphan permissions**: `Billing.View`, `Billing.Manage`, `MedicalRecords.Read`, `Patients.ViewAll`, `Patients.ViewAssigned`, `Patients.Update` leave the catalog after the alias window; code referencing them fails.
6. **Behavioral clarifications (no client contract change)**: `MedicalHistory` DELETE keeps returning 403/404 for all default roles but now via an explicit permission instead of a typo'd role attribute; `AppointmentsController` POST stops excluding `Receptionist` (contract improves, not breaks).

## Backward compatibility

- **Permission alias layer (F2-F4)**: legacy permission names resolve to new catalog names during authorization (`MedicalRecords.ViewAll → ClinicalRecords.ViewAssigned` for HP, `MedicalRecords.ViewAssigned → ClinicalRecords.ViewMinimalAssigned` for Nurse, `Patients.ViewAll → Patients.ViewDemographics`, `Billing.View → Invoices.View ⊕ Payments.View`, etc. — full table in `Permission-Matrix.md`). Orphan permissions are denied with an explicit logged reason.
- **Legacy role policies** are re-registered as permission aggregates (see D4) so existing `[Authorize(Policy = …)]` attributes do not 403 during F3.
- **Compatibility flag** `Authorization:EnforceV2` (default `false` in F2-F3, `true` after F4) controls whether the deny paths above are warnings or hard errors, so staging can be validated before production cutover.
- Public endpoints keep their exact routes; only the webhooks gain an explicit `[AllowAnonymous]` attribute that documents existing behavior.

## Approved decisions referenced

- T02 A1/A1b/A6 staff-access semantics; A5 membership auto-provisioning on booking; A7 public booking/share-link flow (`../Work-Packages/T02-decisions.md`).
- T01 D1/D2 secure clinic discovery and the `/all` flag (`../Work-Packages/T01-baseline-clinic-security.md`).
- ADR-003/004 remain untouched; this ADR does not change tenant/geospatial/financial decisions.

## Phases (each phase ends at a defined exit criterion)

### F1 — Contract (this ADR + companion docs) — documentation only

Deliver `ADR-005`, `Authorization/Permission-Matrix.md`, `Authorization/Jwt-Claims-Contract.md`, `Authorization/Controller-Policy-Map.md`. **Exit**: owner approves; no code changes.

### F2 — Claims, fallback, and data-plane hardening

- `TokenService` and `GeneratePatientToken` emit the normalized contract; no-role users fail login; `SystemRole` includes `Nurse = 5`; `MapInboundClaims = false` + explicit `NameClaimType`/`RoleClaimType`; `PermissionRepository` filters `Role.IsActive`/`IsDeleted`; `FallbackPolicy = authenticated`; explicit `[AllowAnonymous]` on the 16 public endpoints (and webhooks).
**Exit**: unit tests for claim presence/absence; a role-less user receives 403; the 5 anonymous controllers return 401 for anonymous callers except the allow-list.

### F3 — Permission catalog v2 and controller migration

- Seed 55 permissions with matrix grants; add alias layer; re-register the 8 legacy policies as aggregates; apply `Controller-Policy-Map.md` to controllers (including fixing `AppointmentsController.cs:90` and `MedicalHistoryController.cs:146`); delete the dead `AuthorizationPoliciesExtension.cs`.
**Exit**: matrix conformance tests per role; receptionist can `POST /api/appointments`; no controller uses a role-assertion policy; anonymous smoke of the full endpoint list matches `Controller-Policy-Map.md`.

### F4 — Scope handlers and deprecation

- Implement Account/Clinic/Assigned/Self scope handlers; remove the 8 legacy policy names; orphans hard-error; `Authorization:EnforceV2=true` in staging; align `PatientAccessHandler`/query filters with T02 A1.
**Exit**: zero references to legacy policies/orphan permissions in the codebase; scope tests (patient-ID substitution cannot change scope; no eligible membership yields empty result, never a global fallback).

### F5 — Hardening review, break-glass design, and acceptance

- Design the SuperAdmin break-glass flow (reason + expiry + audit) as a follow-up spec; penetration smoke: every endpoint with anonymous/role-less tokens → 401/404 except the allow-list; conformance suite in CI; third-party review.
**Exit**: full test suite green; owner sign-off; no open security findings from the smoke.

## Open questions

- Should `WaitlistController` stay public (explicit anonymous) or be gated?
- Which role (if any) may soft-delete/reassign patients in the future (today `Patients.Delete` = SuperAdmin/AccountAdmin only)?
- `UserController.cs:32 /api/patientauth/register` is currently NOT anonymous (inherits `[Authorize]` from `BaseController`) while `signup` is; owner should pick one canonical patient registration route.