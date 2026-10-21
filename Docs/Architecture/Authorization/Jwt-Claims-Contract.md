# JWT Claims Contract — ClinicFlow (ADR-005 D3)

Status: Proposed (companion to `../ADRs/ADR-005-authorization-hardening.md`)

Exact specification of the claims emitted by the backend (`TokenService.GenerateToken` for staff, `PatientAuthController.GeneratePatientToken` for patients) and consumed by the frontend (mirror contract). Both sides must treat this document as the single source of truth; backend and UI normalize against it in F2.

## Why a contract (audited defects)

- Today staff tokens (`Services/TokenService.cs:57-97`) and patient tokens (`Controllers/PatientAuthController.cs:203-212`) emit different identity claims: staff get `sub`+`user_id`+`account_id`+`clinic_id`; patients get `patient_id`+`user_type` but **no `sub`**.
- With the default `JwtSecurityTokenHandler` outbound/inbound claim maps, `ClaimTypes.Role` serializes to the short name `"role"` — colliding with the custom `"role"` (first-role) claim. Because the JWT payload is a dictionary, multi-role users can be reduced to a single role value after round-tripping, and the "first role" semantics are not deterministic. The contract makes claim names explicit and disables default mapping.
- `Services/TokenService.cs:94-97` falls back to `role=Patient` + `ClaimTypes.Role=User` when the user has no roles — an insecure default that this contract forbids.

## Token envelope

| Property | Value |
|---|---|
| Algorithm | HS256 (same `Jwt:Key` as bearer validation) |
| Issuer | `Jwt:Issuer` (currently `MedPalAPI`) |
| Audience | `Jwt:Audience` (currently `MedPalClient`) |
| Expiry | `Jwt:ExpiryInMinutes` (default 60) — staff; patient token currently uses `Jwt:ExpireMinutes` fallback `"60"`; normalize both to `Jwt:ExpiryInMinutes` |
| Extra | `jti` GUID (both today) |

## Claim naming convention

- Custom claims are **`snake_case`** (already the case for `user_id`, `account_id`, `clinic_id`, `patient_id`, `user_type`).
- The backend sets `JwtSecurityTokenHandler.DefaultMapInboundClaims = false` (or `option.MapInboundClaims = false` on the bearer options) and `TokenValidationParameters.NameClaimType = "sub"`, `RoleClaimType = "roles"` so claims arrive with exactly the names below.
- Claim order is not significant; presence and values are.

## Staff token (TokenService)

| Claim | Type | Always? | Value | Notes |
|---|---|---|---|---|
| `sub` | string | ✅ | `user.Id.ToString()` | canonical subject |
| `user_id` | string | ✅ | `user.Id.ToString()` | duplicate of sub for UI convenience |
| `email` | string | ✅ | `user.Email` | |
| `account_id` | string | ⚠️ | `user.AccountId.Value` | only when `AccountId.HasValue`; SuperAdmin may omit |
| `clinic_id` | string | ⚠️ | `user.ClinicId` | only when `user.ClinicId > 0` |
| `user_type` | string | ✅ | `"staff"` | **new**: discriminates ID claims |
| `roles` | string[] | ✅ | all role names, e.g. `["HealthProfessional","ClinicAdmin"]` | **canonical** — one entry per `UserRole` |
| `role` | string | ✅ | first role name | legacy UI convenience; **must not** be used to grant privileges |
| `jti` | string | ✅ | GUID | |

Removed behaviors:
- `ClaimTypes.Role` string claims are no longer the transport; the bearer options map `"roles"` as the role claim type so `User.IsInRole(...)` keeps working in .NET.
- The no-role fallback (`TokenService.cs:94-97`) is removed: `GenerateToken` throws/returns failure when `UserRoles` is empty → login returns **403** with "no roles assigned". A token with no `roles` claim is treated as invalid by middleware (deny).

## Patient token (GeneratePatientToken)

| Claim | Type | Always? | Value | Notes |
|---|---|---|---|---|
| `sub` | string | ✅ | `patient.Id.ToString()` | **added** (today missing) |
| `patient_id` | string | ✅ | `patient.Id.ToString()` | canonical patient subject |
| `email` | string | ✅ | patient email | |
| `user_type` | string | ✅ | `"patient"` | exists today |
| `roles` | string[] | ✅ | `["Patient"]` | |
| `role` | string | ✅ | `"Patient"` | legacy |
| `jti` | string | ✅ | GUID | |

Patient tokens must **not** carry `account_id`/`clinic_id`: the tenant boundary is resolved server-side through `PatientAccount` membership + consents (T02 A1/A6), never from caller-supplied IDs.

## Deny rules (backend enforcement)

| Situation | Behavior |
|---|---|
| Missing/unparseable `sub` or `user_id` on a staff endpoint | 401 (today `PermissionHandler` fails naturally; make it explicit) |
| `user_type = "staff"` with no `account_id` on an account-scoped endpoint | 401 "no tenant" (account endpoints) |
| `user_type = "staff"` on a patient-portal endpoint (expects `patient_id`) | 401 |
| `user_type = "patient"` on a staff endpoint | 401/403 |
| Missing `patient_id` on a patient-portal endpoint | 401 |
| Empty or missing `roles` claim | 403 (deny; no default role) |
| `roles` present but containing only inactive/deleted role names (DB check) | 403 |
| `role` (first role) missing while `roles` present | accept `roles[0]` as `role` (legacy normalize) |
| Claim with unexpected type name (e.g., legacy `nameid` only) | treated as absent per this contract (deny if the endpoint needs identity) |

## Frontend mirror (espejo) — scheduling.ui

The Angular app decodes the JWT payload (base64url) and uses exactly these claims; no privilege decision may rely on the first-role `role` claim alone.

| Use | Claim(s) |
|---|---|
| User id | `sub` OR `user_id` (fall back from `sub` to `user_id`) |
| Tenant | `account_id` |
| Clinic scope | `clinic_id` (may be absent → UI must not assume a clinic) |
| Patient portal | `patient_id` + `user_type === "patient"` |
| Role list | `roles[]` (all) — used by guards/UI role rendering |
| Legacy role (render only) | `role` |

Frontend rules:
- Token without `roles[]` → treat as unauthenticated (logout/redirect), never as Patient.
- Role guards (`AuditAccessGuard`, `AuditAdminGuard`, `ConsentAccessGuard` stubs today) must read `roles[]`.
- `AuthService` keeps the `auth_token` storage key; the decode helper must handle the new claim names (snake_case) and the absence of legacy `nameid` when maps change.
- `account_id`/`clinic_id` present anywhere in a patient token → reject (contract violation).

## Config keys to normalize

- `Jwt:ExpiryInMinutes` for both token issuers (today patient path uses `Jwt:ExpireMinutes` fallback `"60"`; one key, no fallback guess).
- Add `Authorization:EnforceV2` (bool) consumed by the alias layer and by the token issuance deny rules during F2-F4 transitions.

## Migration order

1. Backend F2: emit new claims; remove fallbacks; disable default claim maps; update `JwtBearerOptions` (`MapInboundClaims=false`, `RoleClaimType="roles"`, `NameClaimType="sub"`).
2. UI F2 in parallel: decode `roles[]` + `sub`/`user_id`; drop reliance on `role` for permissions.
3. Backend F3-F4: enforce deny rules and alias layer; remove legacy claim reads in controllers (`ClaimTypes.NameIdentifier` → `sub` handled by bearer options; `FindFirst("patient_id")` unchanged).
4. Delete any client code reading `nameid`/duplicated `role`.