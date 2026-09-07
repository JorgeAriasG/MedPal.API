# Permission Matrix — ClinicFlow Authorization (ADR-005 target state)

Status: Proposed (companion to `../ADRs/ADR-005-authorization-hardening.md`)

Define the canonical target catalog, the permission × role matrix with scopes, the invariants I1-I6, the legacy→new alias table, and the orphan-permission policy. Implementation agents seed and enforce exactly this matrix in F2-F3.

## Roles

| Role | Symbol | Model definition (owner-approved) |
|---|---|---|
| SuperAdmin | SA | Platform administration only. No routine clinical or scheduling access; break-glass clinical access is a future mechanism (reason + expiry + audit). |
| AccountAdmin | AA | Tenant (Account) administration. All clinics and users of the account; **no** clinical records, vitals, prescriptions, or consent approval by default. |
| ClinicAdmin | CA | Clinic sub-scope administration. Staff and users of one clinic; **no** clinical records, vitals, prescriptions, or consent approval by default. |
| HealthProfessional | HP | Own agenda, assigned patients, full clinical records, prescriptions, vitals, consents (approve/revoke on assigned), own reports. |
| Nurse | NU | Assigned patients, minimal demographics, vitals/anthropometry, minimal clinical information only; cannot create/sign prescriptions; no unlimited records. |
| Receptionist | RE | Agenda and appointment CRUD, minimal demographics, consent status; no clinical records. |
| Patient | PA | Own data only (profile, appointments, documents, prescriptions, payments, consents). |

## Scope model

| Scope | Meaning | Resolved by |
|---|---|---|
| Account (A) | Rows belonging to the caller's tenant (`account_id` claim → `User.AccountId`) | Tenant query filters + account scope handler |
| Clinic (C) | Rows belonging to one clinic inside the account (`clinic_id` claim or `X-Clinic-Id`/`clinicId`) | Clinic scope handler |
| Assigned (Asg) | Rows linked to the caller via `HealthcareProfessional` assignment or Nurse assignment, AND eligible `PatientAccount` membership + per-account consent (T02 A1) | `PatientAccessHandler` (T02 A6) + assignment predicates |
| Self | Rows where caller == resource owner (`sub`/`user_id` for staff, `patient_id` for patients) | Self scope handler |

Notes:
- Account is the **tenant boundary**; Clinic is a **sub-scope**; HealthcareProfessional/Patient are **resource scopes**, never tenants.
- Staff access to any patient row additionally requires `PatientAccessHandler` eligibility (T02 A1/A6): active primary membership, or secondary with `IsVerifiedByPatient && ConsentToShareProfile`; legacy-ghost clinic-link fallback only for legacy ghosts.

## Target catalog (55 permissions)

Groups: **Patients (6), Appointments (5), ClinicalRecords (6), Vitals (4), Prescriptions (4), Consents (4), Users (3), Roles (4), Reports (2), Audit (2), Clinics (2), Subscriptions (2), Invoices (3), Payments (4), FiscalSettings (2), Notifications (2)** = 55.

`ClinicalRecords.Delete` is seeded but granted to **no default role** (break-glass only). `Payments.Create` is granted to AA/CA only by default and explicitly **off for Receptionist** (I6).

## Matrix (permission × role)

Legend: `A`=Account scope, `C`=Clinic scope, `Asg`=Assigned scope, `Self`=Self scope, `–`=no grant.

### Patients.*

| Permission | SA | AA | CA | HP | NU | RE | PA |
|---|---|---|---|---|---|---|---|
| Patients.ViewDemographics | – | A | C | – | – | C | – |
| Patients.ViewAssignedDemographics | – | – | – | Asg | Asg | – | – |
| Patients.ViewOwn | – | – | – | – | – | – | Self |
| Patients.Create | – | A | C | Asg | – | C | – |
| Patients.UpdateDemographics | – | A | C | Asg | Asg | C | – |
| Patients.Delete | A | A | – | – | – | – | – |

SuperAdmin holds `Patients.Delete` for platform data-governance only (no `Patients.*` view/create/update by default, per I2).

### Appointments.*

| Permission | SA | AA | CA | HP | NU | RE | PA |
|---|---|---|---|---|---|---|---|
| Appointments.ViewAll | – | A | C | – | – | C | – |
| Appointments.ViewOwn | – | – | – | Asg | Asg | – | Self |
| Appointments.Create | – | A | C | Asg | Asg | C | Self |
| Appointments.Update | – | A | C | Asg | Asg | C | Self |
| Appointments.Cancel | – | A | C | Asg | – | C | Self |

### ClinicalRecords.*

| Permission | SA | AA | CA | HP | NU | RE | PA |
|---|---|---|---|---|---|---|---|
| ClinicalRecords.ViewAssigned | – | – | – | Asg | – | – | – |
| ClinicalRecords.ViewMinimalAssigned | – | – | – | – | Asg | – | – |
| ClinicalRecords.Create | – | – | – | Asg | – | – | – |
| ClinicalRecords.Update | – | – | – | Asg | – | – | – |
| ClinicalRecords.ViewOwn | – | – | – | – | – | – | Self |
| ClinicalRecords.Delete | – | – | – | – | – | – | – |

### Vitals.*

| Permission | SA | AA | CA | HP | NU | RE | PA |
|---|---|---|---|---|---|---|---|
| Vitals.ViewAssigned | – | – | – | Asg | Asg | – | – |
| Vitals.Create | – | – | – | Asg | Asg | – | – |
| Vitals.Update | – | – | – | Asg | Asg | – | – |
| Vitals.ViewOwn | – | – | – | – | – | – | Self |

Anthropometry/body-composition endpoints of the Nutrition module map to `Vitals.*` (see `Controller-Policy-Map.md`).

### Prescriptions.*

| Permission | SA | AA | CA | HP | NU | RE | PA |
|---|---|---|---|---|---|---|---|
| Prescriptions.Create | – | – | – | Asg | – | – | – |
| Prescriptions.Update | – | – | – | Asg | – | – | – |
| Prescriptions.ViewAssigned | – | – | – | Asg | – | – | – |
| Prescriptions.ViewOwn | – | – | – | – | – | – | Self |

Nurse has **no** `Prescriptions.*` (I4).

### Consents.*

| Permission | SA | AA | CA | HP | NU | RE | PA |
|---|---|---|---|---|---|---|---|
| Consents.View | – | A | C | Asg | Asg | C | – |
| Consents.ViewStatus | – | A | C | Asg | Asg | C | Self |
| Consents.Approve | – | – | – | Asg | – | – | Self |
| Consents.Revoke | – | – | – | Asg | – | – | Self |

AA/CA have **no** `Consents.Approve/Revoke` (I5); approval is a clinical/patient action.

### Users.* / Roles.*

| Permission | SA | AA | CA | HP | NU | RE | PA |
|---|---|---|---|---|---|---|---|
| Users.ViewAll | A | A | C | – | – | – | – |
| Users.Manage | A | A | C | – | – | – | – |
| Users.ManageRoles | A | A | C | – | – | – | – |
| Roles.View | A | A | C | C | C | C | – |
| Roles.Assign | A | A | C | – | – | – | – |
| Roles.Revoke | A | A | C | – | – | – | – |
| Roles.ViewAudit | A | A | – | – | – | – | – |

`Roles.View` is kept for staff UI rendering of role names/chips (was already granted to HP/NU/RE in the legacy seed).

### Reports.* / Audit.* / Clinics.* / Subscriptions.* / Invoices.* / Payments.* / FiscalSettings.* / Notifications.*

| Permission | SA | AA | CA | HP | NU | RE | PA |
|---|---|---|---|---|---|---|---|
| Reports.Generate | A | A | C | – | – | – | – |
| Reports.View | A | A | C | Asg | – | – | – |
| Audit.View | A | A | – | – | – | – | – |
| Audit.Export | A | A | – | – | – | – | – |
| Clinics.View | A | A | C | C | C | C | – |
| Clinics.Manage | A | A | C | – | – | – | – |
| Subscriptions.View | A | A | – | – | – | – | – |
| Subscriptions.Manage | A | A | – | – | – | – | – |
| Invoices.View | A | A | C | Asg | – | C | – |
| Invoices.ViewOwn | – | – | – | – | – | – | Self |
| Invoices.Manage | A | A | C | – | – | – | – |
| Payments.View | A | A | C | – | – | C | – |
| Payments.ViewOwn | – | – | – | – | – | – | Self |
| Payments.Create | A | A | C | – | – | – | – |
| Payments.Refund | A | A | – | – | – | – | – |
| FiscalSettings.View | A | A | C | – | – | – | – |
| FiscalSettings.Manage | A | A | – | – | – | – | – |
| Notifications.View | Self | Self | Self | Self | Self | Self | Self |
| Notifications.Send | A | A | C | – | – | C | – |

`Payments.Create` is **off by default for Receptionist** (I6); any additional role requires an explicit grant. `Reports.View` for HP is own-scope ("reportes propios" by model).

## Invariants (enforced by conformance tests)

| # | Invariant |
|---|---|
| I1 | Only HP sees full clinical records (`ClinicalRecords.ViewAssigned/Create/Update`); PA only `ViewOwn`; NU only `ViewMinimalAssigned`. |
| I2 | SuperAdmin has no clinical and no scheduling grants (no `Patients.*` view/create, `Appointments.*`, `ClinicalRecords.*`, `Vitals.*`, `Prescriptions.*`). |
| I3 | Receptionist has agenda/appointments (`Appointments.ViewAll/Create/Update/Cancel`) and minimal demographics (`Patients.ViewDemographics`) but no clinical (`ClinicalRecords.*`, `Vitals.*`, `Prescriptions.*`) — only `Consents.ViewStatus`. |
| I4 | Nurse has no `Prescriptions.*`. |
| I5 | AccountAdmin and ClinicAdmin have no `Vitals.*`, `Prescriptions.*`, `Consents.Approve`, `Consents.Revoke`. |
| I6 | `Payments.Create` only if explicitly granted; Receptionist off by default. |

## Legacy → new alias table

Applied by the authorization alias layer during F2-F4. `⊕` = OR-set of new permissions (member grant required). Alias resolution is **per role** where the mapping differs.

| Legacy permission | Alias target (new) | Notes |
|---|---|---|
| Patients.ViewAll | Patients.ViewDemographics | RE/AA/CA; orphan after F4 |
| Patients.ViewAssigned | Patients.ViewAssignedDemographics | HP/NU; orphan after F4 |
| Patients.ViewOwn | Patients.ViewOwn | PA |
| Patients.Create | Patients.Create | unchanged |
| Patients.Update | Patients.UpdateDemographics | orphan after F4 |
| Patients.Delete | Patients.Delete | unchanged |
| Appointments.* | Appointments.* | unchanged (5) |
| MedicalRecords.ViewAll | ClinicalRecords.ViewAssigned | never AA/CA under I2/I5; orphan after F4 |
| MedicalRecords.ViewOwn | ClinicalRecords.ViewOwn | PA |
| MedicalRecords.ViewAssigned | ClinicalRecords.ViewAssigned (HP) / ClinicalRecords.ViewMinimalAssigned (NU) | per role |
| MedicalRecords.Read | ClinicalRecords.ViewAssigned (HP) / ClinicalRecords.ViewOwn (PA) | folded; orphan after F4 |
| MedicalRecords.Create | ClinicalRecords.Create | HP |
| MedicalRecords.Update | ClinicalRecords.Update | HP |
| Billing.View | Invoices.View ⊕ Payments.View ⊕ Subscriptions.View | deprecated; orphan after F4 |
| Billing.Manage | Invoices.Manage ⊕ Payments.Create | deprecated; orphan after F4 |
| Users.ViewAll / Users.Manage / Users.ManageRoles | same | unchanged |
| Reports.Generate / Reports.View | same | unchanged |
| Clinics.View / Clinics.Manage | same | unchanged |
| Roles.View / Roles.Assign / Roles.Revoke | same | unchanged |
| Roles.ViewAudit | Roles.ViewAudit ⊕ Audit.View | audit split |

## Orphan permissions

`Patients.ViewAll`, `Patients.ViewAssigned`, `Patients.Update`, `MedicalRecords.Read`, `Billing.View`, `Billing.Manage` have **no 1:1 successor**. Policy:

- F2-F3: alias layer resolves them; granting role uses the new grants from this matrix; a denied orphan logs `ORPHAN_PERMISSION_DENIED`.
- F4: orphans are removed from the catalog; DB grants are revoked; any code/DTO still referencing them fails hard (startup/compile) so nothing silently downgrades.
- Seeder revision is additive and idempotent: it never deletes rows in F2/F3; in F4 it marks orphan permissions `IsActive = false` + `IsDeleted = true` (soft, auditable).

## Seeder contract (F3)

`AuthorizationSeeder` becomes versioned (`AuthorizationSeederV2` calling the same `SeedAsync` entrypoint for the transition): seeds 55 permissions and the exact grants above, honors `Authorization:EnforceV2`, and is covered by a per-role conformance test matrix (one test per role asserting the full expected permission set, plus one negative test per invariant).