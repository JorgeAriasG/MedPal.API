# ADR-006: Availability Blocks (Bloqueo de horario)

Status: Accepted (implementation)

Entidad dedicada para "bloquear" rangos de horario de un doctor o de una clínica
completa. Un bloqueo NO es una cita: no tiene paciente, no genera recordatorio ni
factura, y no transita por el flujo de estados de `Appointment`.

## Context

El staff necesita impedir la reserva/agenda de horarios concretos (vacaciones,
capacitación, sesión administrativa, etc.). Antes de este cambio solo existía
`Appointment` (con paciente) para ocupar tiempo, lo que forzaba a crear citas
ficticias. Además, la disponibilidad pública y la del staff (`DoctorAvailabilityController`)
ignoraban cualquier concepto de bloqueo.

## Decision

- **Entidad dedicada** `AvailabilityBlock`: `ClinicId` (requerido), `DoctorId`
  (nullable → bloqueo de clínica completa cuando es null), `Date` (DateOnly),
  `StartTime`/`EndTime` (TimeOnly), `Reason` (nullable).
- **Sin recurrencia en v1**: un bloqueo es de un solo día. Repeticiones semanales
  se modelarán como filas explícitas o una entidad de recurrencia futura; no se
  añade complejidad de RRULE ahora.
- **Solapamiento rechazado**: `AvailabilityBlock` no puede solaparse con una cita
  (`Appointment` `!IsDeleted`) del mismo doctor (o de cualquier doctor de la clínica
  cuando el bloqueo es de clínica), ni con otro bloqueo del mismo doctor o de la
  clínica. Este chequeo vive en `AvailabilityBlockValidator` (FluentValidation) →
  `IAvailabilityBlockRepository.HasOverlapWithAppointmentsAsync` /
  `HasOverlapWithBlocksAsync`.
- **Multi-tenancy por `ClinicId`**: el query filter del `AppDbContext` aísla los
  bloques por clínica (ClinicAdmin y staff) y por cuenta vía `Clinic.AccountId`
  (AccountAdmin); SuperAdmin ve todos. El repo hereda `TenantAwareRepository`.
- **Soft delete + auditoría**: implementa `ISoftDelete` e `IAuditableEntity`;
  `AuditSaveChangesInterceptor` rellena `CreatedAt`/`UpdatedAt`/`DeletedAt`/
  usuarios y emite `AuditLog` automáticamente. `DELETE` = soft delete.
- **Disponibilidad integrada**: `DoctorAvailabilityController.GetAvailability` y
  `BookingService.GetPublicAvailabilityAsync` marcan `IsAvailable = false` en los
  30-min slots que se solapan con un bloqueo del doctor o de la clínica.

## Consequences

- Los bloqueos no aparecen en la agenda de citas (no son `Appointment`).
- Las citas existentes no se eliminan/editan al crear un bloqueo: solo se impide
  crear un bloqueo que se solape con citas vigentes.
- Romper un bloqueo para reagendar una cita puntual es un cambio de negocio
  pendiente de aprobación del owner (fuera del alcance v1).
- El tenant check de escritura es de paridad con `AppointmentsController` (políticas
  de permiso + query filters). Validar `ClinicId` del usuario (p. ej. arreglando
  `ITenantContextService.HasAccessToClinicAsync` para AccountAdmin) queda como
  recomendación de endurecimiento (ver reporte).

## Referencias

- Modelo: `Models/Appointment.cs` (convenciones), `Models/AvailabilityBlock.cs` (nuevo).
- Políticas reutilizadas: `Appointments.Create`, `Appointments.Cancel`, `ViewAppointmentsPolicy`.
- Endpoints: `api/AvailabilityBlocks` (GET por clínica/fecha, POST, DELETE).