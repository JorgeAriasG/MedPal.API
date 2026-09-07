# Controller → Policy Map — ClinicFlow (ADR-005 F3 work items)

Status: Proposed (companion to `../ADRs/ADR-005-authorization-hardening.md` and `Permission-Matrix.md`)

For every controller/endpoint: the current authorization state, the target fine permission + scope handler, and whether the endpoint is public (explicit allow-list), legacy (kept for retro-compatibility), or needs a concrete code fix. All line numbers verified on branch `feature/authorization-hardening` (2026-09-06).

## Target conventions

- **Policy** = one permission name from the ADR-005 catalog; scopes in `{A=Account, C=Clinic, Asg=Assigned, Self}` per `Permission-Matrix.md`.
- **Scope handler** = the handler that enforces the row boundary: `PermissionHandler` (permission name), `PatientAccessHandler` (staff access to patient, T02 A1/A6), `MedicalRecordAccessHandler` (NOM-004 creator/admin/consent), plus new `Account/Clinic/Assigned/Self` handlers from F4.
- `[Authorize]` (bare) endpoints stay authenticated but gain a permission policy where the resource is sensitive.
- **FIX** = a concrete defect to repair; **LEGACY** = kept for the compatibility window; **PUBLIC** = explicit `[AllowAnonymous]`.

## Public surface (explicit allow-list — 16 endpoints)

| # | Route | File:line | Notes |
|---|---|---|---|
| 1 | POST /api/user/login | `UserController.cs:152` | staff auth; also remove `Role ?? "HealthProfessional"` fallback (`:165`) |
| 2 | POST /api/user/register | `UserController.cs:175` | self-service account bootstrap + Stripe checkout |
| 3 | POST /api/user/initiate-registration | `UserController.cs:294` | |
| 4 | POST /api/user/complete-registration | `UserController.cs:313` | |
| 5 | POST /api/patientauth/signup | `PatientAuthController.cs:104` | patient self-signup |
| 6 | POST /api/patientauth/login | `PatientAuthController.cs:158` | patient login |
| 7 | POST /api/booking/complete | `BookingController.cs:43` | guest booking; add rate limiting |
| 8 | POST /api/booking/registration/complete | `BookingController.cs:74` | |
| 9 | POST /api/booking/registration/resend | `BookingController.cs:88` | add rate limiting |
| 10 | GET /api/booking/availability | `BookingController.cs:102` | |
| 11 | GET /api/checkout/session/{sessionId} | `CheckoutController.cs:20` | |
| 12 | GET /api/subscription/plans | `SubscriptionController.cs:30` | |
| 13 | GET /api/Clinic/all | `ClinicController.cs:49` | LEGACY: only when `Discovery:AllowAnonymousPublicClinics=true` (T01 D2); retire in F4 |
| 14 | GET /api/prescription/validate/{uniqueCode} | `PrescriptionController.cs:184` | pharmacy validation |
| 15 | POST /api/webhooks/stripe | `StripeWebhookController.cs:19` | signature-verified in service; add explicit `[AllowAnonymous]` (today anonymous by omission) |
| 16 | GET /api/webhooks/whatsapp (+ POST :56) | `WhatsAppWebhookController.cs:40,56` | explicit `[AllowAnonymous]` + verify token/HMAC; **GAP**: `VerifySignature` returns true when `WhatsApp:AppSecret` is empty (`:189-190`) — require secret in prod |

**Open question (owner):** `POST /api/waitlist/register` (`WaitlistController.cs:23`) is a public lead-capture form but is not in the original allow-list. Recommended: explicit `[AllowAnonymous]` + rate limiting, or gate it if it is meant to be staff-only.

## 1. Anonymous controllers to protect (F2)

### InvoiceController (`Controllers/InvoiceController.cs:12` — bare `ControllerBase`)

| Route | Line | Target policy | Target scope | Notes |
|---|---|---|---|---|
| GET /api/invoice | 24 | Invoices.View | A (AA) / C (CA) | add class-level `[Authorize]` |
| GET /api/invoice/{id} | 33 | Invoices.View | A/C + invoice account check | |
| GET /api/invoice/patient/{patientId} | 46 | Invoices.View | A/C + `PatientAccessHandler` | |
| GET /api/invoice/appointment/{appointmentId} | 55 | Invoices.View | A/C | |
| GET /api/invoice/status/{status} | 66 | Invoices.View | A/C | |
| POST /api/invoice | 75 | Invoices.Manage | A/C | |
| PUT /api/invoice/{id} | 95 | Invoices.Manage | A/C | |
| PATCH /api/invoice/{id}/status | 119 | Invoices.Manage | A/C | |
| DELETE /api/invoice/{id} | 143 | Invoices.Manage | A/C | |

Patient self: future portal read via `Invoices.ViewOwn` (Self) — no route today.

### PaymentController (`Controllers/PaymentController.cs:11` — bare `ControllerBase`)

| Route | Line | Target policy | Target scope | Notes |
|---|---|---|---|---|
| GET /api/payment | 25 | Payments.View | A (AA) / C (RE,CA) | |
| GET /api/payment/{id} | 34 | Payments.View | A/C + payment account | |
| GET /api/payment/invoice/{invoiceId} | 47 | Payments.View | A/C | |
| GET /api/payment/patient/{patientId} | 56 | Payments.View | A/C + `PatientAccessHandler` | |
| POST /api/payment | 67 | Payments.Create | A/C | **I6**: RE off by default |
| PUT /api/payment/{id} | 94 | Payments.Create (record correction) | A/C | consider dedicated `Payments.Update` if reversal semantics needed |
| DELETE /api/payment/{id} | 128 | Payments.Refund | A | reversal instead of hard delete |

### EmergencyContactController (`Controllers/EmergencyContactController.cs:11` — bare `ControllerBase`)

| Route | Line | Target policy | Target scope | Notes |
|---|---|---|---|---|
| GET /api/emergencycontact | 23 | Patients.ViewDemographics | A/C + `PatientAccessHandler` | |
| GET /api/emergencycontact/{id} | 32 | Patients.ViewDemographics | A/C + `PatientAccessHandler` | |
| GET /api/emergencycontact/patient/{patientId} | 45 | Patients.ViewDemographics | Asg (HP/NU) / A,C | |
| GET /api/emergencycontact/patient/{patientId}/active | 54 | Patients.ViewDemographics | Asg / A,C | |
| POST /api/emergencycontact | 63 | Patients.UpdateDemographics | Asg / C | |
| PUT /api/emergencycontact/{id} | 82 | Patients.UpdateDemographics | Asg / C | |
| DELETE /api/emergencycontact/{id} | 106 | Patients.UpdateDemographics | Asg / C | |

### NotificationMessageController (`Controllers/NotificationMessageController.cs:12` — bare `ControllerBase`)

| Route | Line | Target policy | Target scope | Notes |
|---|---|---|---|---|
| GET /api/notificationmessage | 24 | Notifications.View | Self (own) / A (AA) | catalog additions per `Permission-Matrix.md` |
| GET /api/notificationmessage/{id} | 33 | Notifications.View | Self (owner) | add ownership check (`RecipientUserId == sub`) — today any authenticated caller could read any id |
| GET /api/notificationmessage/user/{userId} | 46 | Notifications.View | Self (userId == sub) or A (AA) | **FIX**: userId must resolve to caller or account staff |
| GET /api/notificationmessage/user/{userId}/unread | 55 | Notifications.View | Self / A | same fix |
| GET /api/notificationmessage/type/{type} | 66 | Notifications.View | Self / A | |
| POST /api/notificationmessage | 75 | Notifications.Send | A/C | staff operational (reminders); self-service if sending to own channel |
| PUT /api/notificationmessage/{id} | 95 | Notifications.Send | A/C + ownership | |
| PATCH /{id}/mark-as-read | 119 | Notifications.View | Self (owner) | |
| PATCH /{id}/mark-as-sent | 139 | Notifications.Send | A/C | |
| DELETE /api/notificationmessage/{id} | 158 | Notifications.Send | A/C + ownership | soft-delete |

### WaitlistController (`Controllers/WaitlistController.cs:12` — bare `ControllerBase`)

| Route | Line | Target policy | Target scope | Notes |
|---|---|---|---|---|
| POST /api/waitlist/register | 23 | PUBLIC (explicit `[AllowAnonymous]`) | — | pending owner decision; add basic abuse protection (rate limit / CAPTCHA option) |

## 2. Auth & user management

### UserController (`Controllers/UserController.cs:20` — `BaseController`)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/user | 57-59 | `Users.ViewAll` + `ViewUsersPolicy` | Users.ViewAll | A / C | remove `ViewUsersPolicy` (F3-F4) |
| GET /api/user/{id} | 67-69 | same double | Users.ViewAll | A / C | |
| GET /api/user/account | 81-83 | same double | Users.ViewAll | A | built from `account_id` claim — keep |
| GET /api/user/me | 368 | `[Authorize]` | (none — identity) | Self | already claim-based; keep |
| POST /api/user | 97-99 | `Users.Manage` + `ManageUsersPolicy` | Users.Manage | A / C | remove `ManageUsersPolicy` |
| PUT /api/user | 332 | `Users.Manage` | Users.Manage | Self / A | today writes `user.Id` from current user — self-update; verify intent |
| DELETE /api/user/{id} | 342 | `Users.Manage` | Users.Manage | A | add no-self-delete guard |
| POST /api/user/soft-delete/{id} | 350 | `Users.Manage` | Users.Manage | A | |
| POST /api/user/restore/{id} | 359 | `Users.Manage` | Users.Manage | A | **FIX**: `id` is overwritten by current user id (`:363`) — restore cannot target another user |
| POST /api/user/login | 152 | `[AllowAnonymous]` | PUBLIC | — | remove `Role ?? "HealthProfessional"` fallback (`:165`) |
| POST /api/user/register | 175 | `[AllowAnonymous]` | PUBLIC | — | |
| POST /api/user/initiate-registration | 294 | `[AllowAnonymous]` | PUBLIC | — | |
| POST /api/user/complete-registration | 313 | `[AllowAnonymous]` | PUBLIC | — | |

### PatientAuthController (`Controllers/PatientAuthController.cs:16` — `BaseController`)

| Route | Line | Current | Target | Notes |
|---|---|---|---|---|
| POST /api/patientauth/register | 32 | **no `[AllowAnonymous]`** (inherits `[Authorize]`) | PUBLIC (explicit) or delete | **FIX/decision**: duplicate of `signup`; owner picks one canonical route |
| POST /api/patientauth/signup | 104 | `[AllowAnonymous]` | PUBLIC | |
| POST /api/patientauth/login | 158 | `[AllowAnonymous]` | PUBLIC | |
| `GeneratePatientToken` | 194-224 | — | emit JWT contract (add `sub`, `roles[]`) | F2 |

### SubscriptionController (`Controllers/SubscriptionController.cs:14` — `[Authorize]` class)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/subscription/plans | 30 | `[AllowAnonymous]` | PUBLIC | — | |
| GET /api/subscription/current | 38 | authenticated | Subscriptions.View | A | |
| GET /api/subscription/status | 52 | authenticated | Subscriptions.View | A | |
| POST /api/subscription/create-checkout | 63 | authenticated | Subscriptions.Manage | A | |
| POST /api/subscription/create-portal | 79 | authenticated | Subscriptions.Manage | A | |

### RoleController (`Controllers/RoleController.cs:19` — `BaseController`)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/role | 47 | `Roles.View` | Roles.View | A/C | |
| GET /api/role/by-name/{name} | 67 | `Roles.View` | Roles.View | A/C | |
| POST /api/role/{roleId}/assign-to-user | 93 | `Roles.Assign` | Roles.Assign | A/C | keep hierarchical guards (`:156-177`) |
| POST /api/role/{roleId}/remove-from-user/{userId} | 218 | `Roles.Revoke` | Roles.Revoke | A/C | keep guards + reason requirement |
| remaining audit endpoints | 330+ | `Roles.ViewAudit` (expected) | Roles.ViewAudit | A | verify each against `Roles.ViewAudit` |

### CheckoutController (`Controllers/CheckoutController.cs:11` — bare `ControllerBase`)

| Route | Line | Current | Target |
|---|---|---|---|
| GET /api/checkout/session/{sessionId} | 20 | `[AllowAnonymous]` | PUBLIC (keep); add explicit class `[Authorize]`? **No** — public by design |

## 3. Booking (guest + staff)

### BookingController (`Controllers/BookingController.cs:24` — `BaseController`)

| Route | Line | Current | Target | Notes |
|---|---|---|---|---|
| POST /api/booking/complete | 43 | `[AllowAnonymous]` | PUBLIC | guest booking per A5/A7 |
| POST /api/booking/registration/complete | 74 | `[AllowAnonymous]` | PUBLIC | |
| POST /api/booking/registration/resend | 88 | `[AllowAnonymous]` | PUBLIC | rate limit |
| GET /api/booking/availability | 102 | `[AllowAnonymous]` | PUBLIC | |
| POST /api/booking/staff/link | 127 | `[Authorize]` | Appointments.Create | C (staff clinic) — used to generate share links |

## 4. Clinics & catalogs

### ClinicController (`Controllers/ClinicController.cs:13` — `BaseController`)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/Clinic | 31 | `Clinics.View` | Clinics.View | A | |
| GET /api/Clinic/all | 48 | `[AllowAnonymous]` | LEGACY flag-gated | — | T01 D2; retire F4 |
| GET /api/patient/clinics | 63 | `[Authorize]` | (identity, `patient_id`) | Self | T01 contract — keep |
| GET /api/Clinic/{id} | 77 | `Clinics.View` | Clinics.View | A/C | |
| POST /api/Clinic | 91-94 | `Clinics.Manage` ×2 + `AdministerClinicPolicy` | Clinics.Manage | A | **FIX**: remove duplicate `Clinics.Manage` (`:92`/`:94`) and `AdministerClinicPolicy` |
| PUT /api/Clinic | 118 | `Clinics.Manage` | Clinics.Manage | A/C | |
| DELETE /api/Clinic/{id} | 135 | `Clinics.Manage` | Clinics.Manage | A | soft-delete already expected |

### Cie10Controller (`Controllers/Cie10Controller.cs:12` — `[Authorize]` class)

Catalog of non-PII diagnostics codes: keep class `[Authorize]` (any authenticated staff/patient) — no fine permission needed (F4 note: catalog endpoints may use `Clinics.View` if policy uniformity is required).

### NutritionController (`Controllers/NutritionController.cs:14` — `[Authorize]` class)

| Area | Lines | Target policy | Scope | Notes |
|---|---|---|---|---|
| GET /api/nutrition/food… (catalog) | 47-80 | `[Authorize]` only (non-PII) | — | keep |
| Anthropometry / body composition | ~80-250 | Vitals.ViewAssigned / Vitals.Create / Vitals.Update | Asg | mapped to `Vitals.*` per matrix |
| Diet plans | ~250-380 | ClinicalRecords.Create / ClinicalRecords.Update / ViewAssigned | Asg | HP only |
| Supplements | ~380-530 | Prescriptions.Create / ViewAssigned (HP) | Asg | NP screen for Nurse (no `Prescriptions.*`) |

Verify exact route→handler mapping during F3 implementation; the point is that Nutrition PII follows `Vitals.*`/`ClinicalRecords.*`/`Prescriptions.*`, not generic `[Authorize]`.

## 5. Patients & clinical

### PatientsController (`Controllers/PatientsController.cs:17` — `PatientController : BaseController`)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/patient | 30 | `ViewPatientsPolicy` | Patients.ViewDemographics (RE/AA/CA) OR Patients.ViewAssignedDemographics (HP/NU) | C / A / Asg | remove `ViewPatientsPolicy`; today repo returns account-scoped list (`:44`) |
| GET /api/patient/{id} | 49 | resource handler only | Patients.ViewDemographics + `PatientAccessHandler` OR Patients.ViewOwn (self) | A/C/Asg/Self | keep `PatientAccessRequirement`; NO policy attribute today — add fine permission |
| POST /api/patient | 69 | `Patients.Create` | Patients.Create | C/A/Asg | |
| PUT /api/patient/{id} | 92 | `Patients.Update` → alias Patients.UpdateDemographics | Patients.UpdateDemographics | C/A/Asg | |
| DELETE /api/patient/{id} | ~121 | (verify) | Patients.Delete | A | SA/AA only |

### PatientDetailsController (`Controllers/PatientDetailsController.cs:15` — `[Authorize]` class)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/patientdetails | 41 | `[Authorize]` only | ClinicalRecords.ViewAssigned (HP) / ViewMinimalAssigned (NU) | Asg | **FIX**: today returns all rows (`:45`) — enforce assignment filter |
| GET /api/patientdetails/{id} | 50 | `PatientAccessRequirement` | ClinicalRecords.ViewAssigned / ViewMinimalAssigned | Asg | |
| GET /api/patientdetails/patient/{patientId} | 64 | `PatientAccessRequirement` | same | Asg | |
| remaining routes | 80-180 | `PatientAccessRequirement` (expected) | ClinicalRecords.* / Vitals.* per route | Asg | verify per route |

### MedicalHistoryController (`Controllers/MedicalHistoryController.cs:16` — bare `ControllerBase`, per-endpoint attributes)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/medicalhistory | 42 | `MedicalRecords.ViewAll` → alias ClinicalRecords.ViewAssigned | ClinicalRecords.ViewAssigned | Asg | **FIX**: never returns all rows globally; assignment filter |
| GET /api/medicalhistory/{id} | 52 | `MedicalRecords.Read` + `MedicalRecordAccessRequirement` | ClinicalRecords.ViewAssigned (HP) / ViewOwn (PA) | Asg/Self | NOM-004 log stays |
| POST /api/medicalhistory | 76 | `MedicalRecords.Create` → alias | ClinicalRecords.Create | Asg | |
| PUT /api/medicalhistory/{id} | 110 | `MedicalRecords.Update` → alias | ClinicalRecords.Update | Asg + creator/admin (MedicalRecordAccessHandler) | |
| DELETE /api/medicalhistory/{id} | 145 | `[Authorize(Roles = "Admin")]` | ClinicalRecords.Delete (no default role) | break-glass | **FIX**: replace nonexistent `Admin` role attribute; behavior stays 403 for all default roles |
| GET /api/medicalhistory/patient/{patientDetailsId} | 162 | `[Authorize]` | ClinicalRecords.ViewAssigned / ViewMinimalAssigned / ViewOwn | Asg/Self | |
| GET /api/medicalhistory/patient/{patientDetailsId}/recent | 190 | `[Authorize]` | same | Asg/Self | |

### MedicalHistoryAttachmentsController (`Controllers/MedicalHistoryAttachmentsController.cs:17` — `[Authorize]` class)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET attachments of history | 43 | `MedicalRecords.ViewOwn` | ClinicalRecords.ViewAssigned / ViewOwn | Asg/Self | |
| POST attachments | 64 | `MedicalRecords.Update` → alias | ClinicalRecords.Update | Asg | |
| GET /api/attachments/{id}/content | 118 | resource-based `MedicalRecords.Read` | ClinicalRecords.ViewAssigned / ViewOwn | Asg/Self | keep resource check |
| DELETE /api/attachments/{id} | 150 | `MedicalRecords.Update` → alias | ClinicalRecords.Update | Asg | |

### VitalSignController (`Controllers/VitalSignController.cs:15` — `[Authorize]` class)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/vitalsign | 53 | `[Authorize]` only | Vitals.ViewAssigned | Asg | **FIX**: today returns all rows (`:56`) |
| GET /api/vitalsign/{id} | 62 | `PatientAccessRequirement` | Vitals.ViewAssigned | Asg | |
| GET /api/vitalsign/patientdetails/{patientDetailsId} | 76 | `PatientAccessRequirement` | Vitals.ViewAssigned | Asg | |
| POST / PUT (create/update) | ~81-198 | `PatientAccessRequirement` (expected) | Vitals.Create / Vitals.Update | Asg | verify per route |
| patient self view (future) | — | — | Vitals.ViewOwn | Self | |

### PrescriptionController (`Controllers/PrescriptionController.cs:18` — `BaseController`)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| POST /api/prescription | 44 | `MedicalRecords.Create` → alias | Prescriptions.Create | Asg | keep allergy check + `PatientAccessRequirement` |
| POST /api/prescription/check-allergies | 111 | `MedicalRecords.Create` → alias | Prescriptions.Create | Asg | |
| GET /api/prescription/{id} | 135 | `MedicalRecords.ViewAssigned` → alias | Prescriptions.ViewAssigned | Asg | |
| GET /api/prescription/my | 156 | `[Authorize]` via BaseController + `patient_id` | Prescriptions.ViewOwn | Self | |
| GET /api/prescription/patient/{patientId} | 168 | `MedicalRecords.ViewAssigned` | Prescriptions.ViewAssigned | Asg | |
| GET /api/prescription/validate/{uniqueCode} | 183 | `[AllowAnonymous]` | PUBLIC | — | pharmacy validation |
| GET /api/prescription/{id}/qr | 218 | `MedicalRecords.ViewAssigned` | Prescriptions.ViewAssigned / ViewOwn | Asg/Self | |

### ConsentController (`Controllers/ConsentController.cs:18` — `[Authorize]` class)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/consent/patient/{patientDetailsId} | 30 | `[Authorize]` only | Consents.View | A/C/Asg | |
| GET /api/consent/patient/{patientDetailsId}/active | 39 | `[Authorize]` only | Consents.ViewStatus | A/C/Asg/Self | receptionist eligibility |
| POST /api/consent/grant | 48 | `ManagePatientsPolicy` | Consents.Approve | Asg/Self | **FIX**: remove role policy (excludes RE legitimately but also blocks patient self-approval flows) |
| POST /api/consent/{id}/revoke | 68 | `ManagePatientsPolicy` | Consents.Revoke | Asg/Self | |
| POST /api/consent/check | 90 | `[Authorize]` only | Consents.ViewStatus | Asg/Self | |

### ArcoController (`Controllers/ArcoController.cs:14` — `BaseController`)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| POST /api/arco/request | 24 | `[Authorize]` | Patients.ViewOwn (self) | Self | any authenticated user requests their ARCO rights |
| GET /api/arco/export/{patientId} | 45 | `Users.Manage` | Patients.ViewOwn (self) / Patients.Delete (SA/AA export for governance) | Self/A | refine during F3; ARCO export is a data-rights action |
| POST /api/arco/execute/{requestId} | 56 | `Users.Manage` | Patients.Delete (SA/AA) | A | data governance |
| POST /api/arco/anonymize/{patientId} | 67 | `Users.Manage` | Patients.Delete (SA/AA) | A | audit mandatory |
| POST /api/arco/marketing-block/{patientId} | 76 | `Users.Manage` | Patients.UpdateDemographics (AA) / self | A/Self | |

## 6. Scheduling

### AppointmentsController (`Controllers/AppointmentsController.cs:20` — `BaseController`)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/appointments | 34 | `ViewAppointmentsPolicy` | Appointments.ViewAll (AA/CA/RE) OR Appointments.ViewOwn (HP/NU) | A/C/Asg | resolve by effective scope; remove role policy |
| GET /api/appointments/patient/{patientId} | 52 | `Appointments.ViewOwn` + `ViewAppointmentsPolicy` | Appointments.ViewAll / ViewOwn + `PatientAccessHandler` | A/C/Asg | remove `ViewAppointmentsPolicy`; double gate restricts RE |
| GET /api/appointments/my | 62 | `patient_id` claim | Appointments.ViewOwn | Self | keep claim check |
| GET /api/appointments/{id} | 74 | `Appointments.ViewOwn` + `ViewAppointmentsPolicy` | Appointments.ViewAll / ViewOwn | A/C/Asg/Self | |
| POST /api/appointments | 88 | `Appointments.Create` + `ManagePatientsPolicy` | Appointments.Create | A/C/Asg/Self | **FIX**: remove `ManagePatientsPolicy` (`:90`) — it 403s Receptionist |
| PUT /api/appointments/{id} | 101 | `Appointments.Update` | Appointments.Update | A/C/Asg/Self | |
| DELETE /api/appointments/{id} | 115 | `Appointments.Cancel` | Appointments.Cancel | A/C/Asg/Self | |
| POST /{id}/start, /complete, /noshow, /reschedule, /reminder | 129-240 | `Appointments.Update` | Appointments.Update | Asg/C | |
| POST /{id}/cancel | 149 | `patient_id` claim only | Appointments.Cancel | Self | **FIX**: validate appointment ownership (`:156-160` reads the appointment but never checks `PatientId == patientId`) |
| POST /{id}/patient-reschedule | 168 | `patient_id` claim + ownership check | Appointments.Update | Self | ownership check already present (`:182`) |
| POST /patient-book | 198 | `patient_id` claim | Appointments.Create | Self | |

### DoctorAvailabilityController (`Controllers/DoctorAvailabilityController.cs:12` — `BaseController`)

| Route | Line | Current | Target policy | Scope | Notes |
|---|---|---|---|---|---|
| GET /api/doctoravailability/by-clinic/{clinicId} | 31 | `[Authorize]` (BaseController) | Appointments.ViewAll | C | RE/CA/AA |
| GET /api/doctoravailability/{doctorId}/availability | 45 | `[Authorize]` (BaseController) | Appointments.ViewOwn (HP self) / ViewAll (RE/AA/CA) | Self/C/Asg | |

## 7. Audit & entity logs

### EntityAuditLogController (`Controllers/EntityAuditLogController.cs:15`)

| Route | Line | Current | Target policy | Scope |
|---|---|---|---|---|
| GET /api/entity-audit-logs | 31 | `ViewAuditLogPolicy` | Audit.View | A |
| GET /api/entity-audit-logs/{id} | 62 | `ViewAuditLogPolicy` | Audit.View | A |

### MedicalRecordAccessLogController (`Controllers/MedicalRecordAccessLogController.cs:14`)

| Route | Line | Current | Target policy | Scope |
|---|---|---|---|---|
| GET /api/audit-logs | 31 | `ViewAuditLogPolicy` | Audit.View | A |
| GET /api/audit-logs/{id} | 66 | `ViewAuditLogPolicy` | Audit.View | A |
| GET /api/audit-logs/reports/generate | 79 | `ViewAuditLogPolicy` | Audit.Export | A |

## 8. Webhooks (explicit anonymous + signature)

### StripeWebhookController (`Controllers/StripeWebhookController.cs:10` — bare `ControllerBase`)

| Route | Line | Current | Target | Notes |
|---|---|---|---|---|
| POST /api/webhooks/stripe | 19 | anonymous by omission | PUBLIC: explicit `[AllowAnonymous]` | keep signature verification inside `IStripeService.HandleWebhookAsync`; add replay-safe processing note per ADR-003 |

### WhatsAppWebhookController (`Controllers/WhatsAppWebhookController.cs:21` — bare `ControllerBase`)

| Route | Line | Current | Target | Notes |
|---|---|---|---|---|
| GET /api/webhooks/whatsapp | 40 | anonymous by omission | PUBLIC: explicit `[AllowAnonymous]` | verify-token check already implemented (`:46`) |
| POST /api/webhooks/whatsapp | 56 | anonymous by omission | PUBLIC: explicit `[AllowAnonymous]` | HMAC `VerifySignature`; **GAP**: returns `true` when `AppSecret` empty (`:189-190`) — production must require a configured secret |

## Legacy endpoints kept for retro-compatibility (explicit)

- `GET /api/Clinic/all` — flag-gated (T01 D2), retire F4.
- `GET /api/patient/clinics` — T01 "mis clínicas" legacy, keep as-is (A7).
- All 12 remaining public endpoints in the allow-list (registration/booking/checkout/pharmacy) — required by the product flows; keep with hardening (rate limits, secret enforcement).
- Legacy role policies are re-registered as permission aggregates during F3 (name-compatible) and removed F4 — no endpoint is "kept" on a role policy after F4.

## F3 work-item checklist (derived from this map)

1. Class-level `[Authorize]` + per-endpoint policies on Invoice/Payment/EmergencyContact/NotificationMessage controllers.
2. Fix `AppointmentsController.cs:90` (remove `ManagePatientsPolicy`).
3. Fix `MedicalHistoryController.cs:146` (replace `Roles = "Admin"`).
4. Fix `ClinicController.cs:92-94` (deduplicate `Clinics.Manage`, drop `AdministerClinicPolicy`).
5. Add fine policies to `PatientDetailsController`, `VitalSignController`, `ConsentController`, `PrescriptionController` (replace `MedicalRecords.*` aliases with `ClinicalRecords.*`/`Prescriptions.*`).
6. Remove `ViewUsersPolicy`/`ViewPatientsPolicy`/etc. references from `UserController`, `PatientsController`, `AppointmentsController`, `EntityAuditLogController`, `MedicalRecordAccessLogController`.
7. Add explicit `[AllowAnonymous]` to webhook controllers; enforce WhatsApp `AppSecret` in prod.
8. Ownership fixes: notification reads, appointment self-cancel, user restore target.
9. Delete `Authorization/Policies/AuthorizationPoliciesExtension.cs`.
10. Conformance tests from `Permission-Matrix.md` invariants.