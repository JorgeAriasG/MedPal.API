# 📋 ANÁLISIS ARQUITECTÓNICO - MedPal API

**Fecha**: 09 Enero 2026  
**Estado**: Revisión Post-Refactorización

---

## 🔍 RESUMEN EJECUTIVO

El proyecto tiene una **arquitectura general sólida** con una buena separación de responsabilidades. Sin embargo, hay **5 problemas clave** y **8 recomendaciones de mejora** que podrían optimizar significativamente la mantenibilidad y escalabilidad.

---

## 🚨 PROBLEMAS CRÍTICOS IDENTIFICADOS

### 1. **UserTask: Redundancia de Información (CRÍTICO)**

**Ubicación**: `Models/UserTask.cs`

```csharp
public class UserTask
{
    public int AppointmentId { get; set; }    // FK a Appointment
    public int PatientId { get; set; }        // FK a Patient (REDUNDANTE)
    public int UserId { get; set; }           // FK a User (REDUNDANTE)
    
    // El PatientId ya está en Appointment.PatientId
    // El UserId ya está en Appointment.UserId
}
```

**Problema**: 
- Los FKs `PatientId` y `UserId` se pueden obtener desde `Appointment`
- Crea inconsistencia: ¿Qué pasa si `UserTask.PatientId ≠ Appointment.Patient.Id`?
- Duplica datos innecesariamente

**Solución**:
```csharp
public class UserTask
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Appointment")]
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public string TaskDescription { get; set; }

    [Required]
    public string TaskStatus { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigations (obtener Patient y User desde Appointment)
    public virtual Appointment Appointment { get; set; }
}
```

---

### 2. **Invoice.TotalAmount vs Payment.AmountPaid: Falta Validación (ALTO)**

**Ubicación**: `Models/Invoice.cs`, `Models/Payment.cs`

**Problema**:
- No hay validación de que `SUM(Payment.AmountPaid) <= Invoice.TotalAmount`
- No hay campo para `PaidAmount` en Invoice para auditoría rápida
- No hay campo `RemainingAmount`
- No hay estado claro: ¿cuándo un Invoice está "Paid"?

**Solución**:
```csharp
public class Invoice
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Patient")]
    [Required]
    public int PatientId { get; set; }

    [ForeignKey("Appointment")]
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    // NUEVO: Suma en caché de pagos para consultas rápidas
    public decimal PaidAmount { get; set; } = 0m;

    // NUEVO: Campo calculado para balance
    public decimal RemainingAmount => TotalAmount - PaidAmount;

    [Required]
    public string Status { get; set; } // "Pending", "PartiallyPaid", "Paid", "Overdue"

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public virtual Patient Patient { get; set; }
    public virtual Appointment Appointment { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }
}
```

---

### 3. **Report: Uso Unclear y Falta de Contexto**

**Ubicación**: `Models/Report.cs`

```csharp
public class Report
{
    public int PatientId { get; set; }
    public string ReportType { get; set; }
    public string ReportFile { get; set; }      // ¿Ruta? ¿URL? ¿Blob?
    public string Description { get; set; }
    
    // Falta:
    // - ¿Generado por quién? (UserId)
    // - ¿De cuál consulta? (AppointmentId)
    // - ¿Acceso ARCO? (IsArcoReport)
}
```

**Problema**:
- Falta relación con el profesional que generó el reporte
- No hay trazabilidad de auditoría
- `ReportFile` es ambiguo (¿cómo se accede?)
- No hay relación con `Appointment` o `MedicalHistory`

**Solución**:
```csharp
public class Report
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Patient")]
    [Required]
    public int PatientId { get; set; }

    [ForeignKey("MedicalHistory")]
    public int? MedicalHistoryId { get; set; }  // Opcional

    [ForeignKey("CreatedBy")]
    public int? CreatedByUserId { get; set; }

    [Required]
    public string ReportType { get; set; }  // "Clinical", "Diagnostic", "ARCO", etc.

    [Required]
    public string FileUrl { get; set; }  // URL clara al archivo

    public string Description { get; set; }

    public bool IsConfidential { get; set; } = true;
    
    public bool IsArcoReport { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public virtual Patient Patient { get; set; }
    public virtual MedicalHistory MedicalHistory { get; set; }
    public virtual User CreatedBy { get; set; }
}
```

---

### 4. **Appointment.Status: String sin Validación**

**Ubicación**: `Models/Appointment.cs`

```csharp
[Required]
public string Status { get; set; }  // ¿"scheduled"? ¿"Scheduled"? ¿"pending"?
```

**Problema**:
- Permite valores arbitrarios
- Errores tipográficos rompen la lógica
- Duplicado en `Invoice.Status`, `UserTask.TaskStatus`, etc.

**Solución**: Usar `Enum` centralizado

```csharp
public enum AppointmentStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4,
    Rescheduled = 5
}

public class Appointment
{
    [Required]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    
    // ...
}
```

---

### 5. **Patient: Datos de Contacto Duplicados**

**Ubicación**: `Models/Patient.cs`

```csharp
public class Patient
{
    public string Name { get; set; }
    public string Middlename { get; set; }
    public string Lastname { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string EmergencyContact { get; set; }
    
    // Relación 1:1 con User que también tiene:
    // - Email
    // - Name (aunque no es el mismo campo)
}
```

**Problema**:
- Si un Patient tiene portal de usuario (`UserId`), ¿cuál email es válido?
- `EmergencyContact` solo tiene nombre, sin tipo de relación ni teléfono

---

## 💡 RECOMENDACIONES DE MEJORA

### **RECOMENDACIÓN 1: Crear Enum para Estados Globales**

**Prioridad**: ALTA

Centralizar todos los enums para consistencia:

```csharp
// Enums/AppointmentStatus.cs
public enum AppointmentStatus { Scheduled, InProgress, Completed, Cancelled, NoShow }

// Enums/InvoiceStatus.cs
public enum InvoiceStatus { Pending, PartiallyPaid, Paid, Overdue, Cancelled }

// Enums/PaymentMethod.cs
public enum PaymentMethod { Cash, CreditCard, BankTransfer, Insurance }

// Enums/TaskStatus.cs
public enum TaskStatus { Pending, InProgress, Completed, Cancelled }

// Enums/ReportType.cs
public enum ReportType { Clinical, Diagnostic, Imaging, Laboratory, ARCO }
```

**Beneficio**: Eliminación de errores, mejor validación, queries más eficientes.

---

### **RECOMENDACIÓN 2: Crear Modelo `EmergencyContact` Separado**

**Prioridad**: MEDIA

```csharp
public class EmergencyContact
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Patient")]
    public int PatientId { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    public string Relationship { get; set; }  // "Parent", "Sibling", "Spouse", etc.

    [Required]
    public string Phone { get; set; }

    public string Email { get; set; }

    public string Address { get; set; }

    public int Priority { get; set; } = 1;  // 1 = primario, 2 = secundario, etc.

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public virtual Patient Patient { get; set; }
}
```

**En Patient.cs**:
```csharp
[Required]
public string EmergencyContact { get; set; }  // REMOVER

public virtual ICollection<EmergencyContact> EmergencyContacts { get; set; }  // AGREGAR
```

---

### **RECOMENDACIÓN 3: Auditoría Consistente en Todos los Modelos**

**Prioridad**: MEDIA

Crear interfaz base:

```csharp
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    int? CreatedByUserId { get; set; }
    int? UpdatedByUserId { get; set; }
    User CreatedByUser { get; set; }
    User UpdatedByUser { get; set; }
}

// Implementar en: Patient, Appointment, Invoice, Payment, etc.
public class Appointment : IAuditableEntity
{
    // ...
    public int? CreatedByUserId { get; set; }
    public int? UpdatedByUserId { get; set; }
    public User CreatedByUser { get; set; }
    public User UpdatedByUser { get; set; }
}
```

---

### **RECOMENDACIÓN 4: Soft Delete Consistente**

**Prioridad**: MEDIA

```csharp
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    int? DeletedByUserId { get; set; }
}

// Algunos modelos lo tienen, otros no:
// ✅ Patient.IsDeleted
// ✅ User.IsDeleted
// ❌ Appointment.IsDeleted (FALTA)
// ❌ Invoice.IsDeleted (FALTA)
// ❌ MedicalHistory.IsDeleted (FALTA)
```

---

### **RECOMENDACIÓN 5: Crear Modelo `AuditLog` Centralizado**

**Prioridad**: ALTA

Para cumplimiento normativo (NOM-004):

```csharp
public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public string EntityType { get; set; }  // "Patient", "MedicalHistory", etc.

    [Required]
    public int EntityId { get; set; }

    [Required]
    public string Action { get; set; }  // "Create", "Update", "Delete", "View"

    public string ChangedFields { get; set; }  // JSON con cambios

    public string OldValues { get; set; }  // JSON con valores previos

    public string NewValues { get; set; }  // JSON con nuevos valores

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string IpAddress { get; set; }

    public string UserAgent { get; set; }

    public virtual User User { get; set; }
}
```

---

### **RECOMENDACIÓN 6: Limpiar NotificationMessage**

**Prioridad**: BAJA

`NotificationMessage` NO tiene `[Key]` y no está en `DbContext`. Parece ser un DTO.

```csharp
// Debe estar en DTOs/, no en Models/
// O si es modelo de DB, debe tener [Key] e Id

public class Notification
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    public string Recipient { get; set; }

    [Required]
    public string Subject { get; set; }

    [Required]
    public string Body { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime SentAt { get; set; }

    public DateTime? ReadAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; }
}
```

---

### **RECOMENDACIÓN 7: Validar Relaciones de Datos**

**Prioridad**: MEDIA

En `AppDbContext.OnModelCreating`:

```csharp
// Invoice: AppointmentId debe corresponder al PatientId
modelBuilder.Entity<Invoice>()
    .HasOne(i => i.Patient)
    .WithMany()
    .HasForeignKey(i => i.PatientId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Invoice>()
    .HasOne(i => i.Appointment)
    .WithMany(a => a.Invoices)
    .HasForeignKey(i => i.AppointmentId)
    .OnDelete(DeleteBehavior.Restrict);

// Validación: Appointment.PatientId == Invoice.PatientId
// (implementar en el servicio/controller)

// UserTask: Solo debe referenciar AppointmentId
// (remover redundantes PatientId, UserId)
```

---

### **RECOMENDACIÓN 8: Documentación de Estados**

**Prioridad**: BAJA

Agregar comentarios en enums:

```csharp
/// <summary>
/// Estados de cita médica
/// - Scheduled: Cita agendada
/// - InProgress: En consulta
/// - Completed: Completada
/// - Cancelled: Cancelada (se puede reagendar)
/// - NoShow: Paciente no asistió
/// - Rescheduled: Reagendada (la original está cerrada)
/// </summary>
public enum AppointmentStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4,
    Rescheduled = 5
}
```

---

## 📊 MATRIZ DE IMPACTO

| Problema | Severidad | Esfuerzo | Prioridad |
|----------|-----------|----------|-----------|
| UserTask Redundancia | 🔴 CRÍTICO | 2 horas | 1️⃣ **AHORA** |
| Invoice Validación | 🟠 ALTO | 3 horas | 1️⃣ **AHORA** |
| Report Context | 🟠 ALTO | 2 horas | 2️⃣ **PRONTO** |
| Status Enums | 🟡 MEDIO | 4 horas | 2️⃣ **PRONTO** |
| EmergencyContact | 🟡 MEDIO | 2 horas | 3️⃣ **DESPUÉS** |
| AuditLog Central | 🟡 MEDIO | 5 horas | 1️⃣ **AHORA** |
| Soft Delete Consistencia | 🟡 MEDIO | 3 horas | 2️⃣ **PRONTO** |
| Patient Datos | 🟡 MEDIO | 2 horas | 3️⃣ **DESPUÉS** |

---

## ✅ COSAS BIEN HECHAS

1. **Separación clara de responsabilidades** entre models, DTOs, repositories y controllers
2. **Relaciones bien definidas** entre Patient ↔ Appointment ↔ Invoice ↔ Payment
3. **Migración de MedicalHistory** ya completada (sin redundancias)
4. **Sistema de autorización** con Roles y Permissions bien estructurado
5. **Manejo de ARCO** (derecho de acceso) implementado
6. **Timestamps** (CreatedAt, UpdatedAt) en la mayoría de modelos
7. **Soft deletes** (IsDeleted, IsAnonymized) en modelos sensibles
8. **Enums para especialidades** en MedicalHistory (SpecialtyData polimórfico)

---

## 🎯 PLAN DE ACCIÓN REALIZADO & PENDIENTE

### **Fase 1 (Esta semana)**: Críticos ✅ COMPLETADO
- [x] Refactorizar `UserTask` (remover PatientId, UserId)
- [x] Mejorar `Invoice` (agregar PaidAmount, RemainingAmount, DueDate)
- [x] Crear `AuditLog` centralizado
- [x] Crear migración para estos cambios
- [x] Crear enums: AppointmentStatus, InvoiceStatus, PaymentMethod, ReportType, TaskStatus
- [x] Crear IInvoiceService y PaymentService con validaciones

### **Fase 2 (Próxima semana)**: Importantes ✅ COMPLETADO
- [x] Implementar `Enum` centralizados (Status, PaymentMethod, ReportType)
- [x] Mejorar `Report` (agregar auditoria, MedicalHistory FK)
- [x] Implementar interface `ISoftDelete`
- [x] Implementar interface `IAuditableEntity`
- [x] Crear migración
- [x] Crear AppDbContext.SaveChangesAsync con validaciones automáticas

### **Fase 3 (Después)**: Mejoras ✅ COMPLETADO
- [x] Crear `EmergencyContact` modelo separado
- [x] Limpiar `NotificationMessage` (agregar UserId, IsSent, IsRead, SentAt, ReadAt)
- [x] Documentar estados en comentarios
- [x] Implementar validaciones en contexto

### **Fase 4 (SIGUIENTE)**: API Layer & Validaciones Avanzadas
- [ ] Crear DTOs para modelos clave (EmergencyContact, Payment, Invoice, NotificationMessage)
- [ ] Crear/mejorar Controllers REST para entidades nuevas
- [ ] Implementar FluentValidation para DTOs
- [ ] Crear servicios de aplicación (ApplicationServices)
- [ ] Implementar manejo de excepciones consistente
- [ ] Crear endpoints para sincronización de datos relacionados

#### **Detalles de Phase 4**

**DTOs a Crear:**
- `EmergencyContactReadDTO` / `EmergencyContactWriteDTO`
- `PaymentReadDTO` / `PaymentWriteDTO` 
- `InvoiceReadDTO` / `InvoiceWriteDTO`
- `NotificationMessageReadDTO` / `NotificationMessageWriteDTO`

**Controllers a Crear/Mejorar:**
- `EmergencyContactController` (GET, POST, PUT, DELETE)
- `PaymentController` (POST, GET by invoice, DELETE)
- `InvoiceController` (GET, POST, GET payments, sync)

**Servicios de Aplicación:**
- `IEmergencyContactService` con CRUD y validaciones
- `IInvoiceService` (ya existe, mejorar endpoints)
- `IPaymentService` (ya existe, mejorar endpoints)

**Validadores FluentValidation:**
- `EmergencyContactValidator`
- `PaymentValidator`
- `InvoiceValidator`
- `NotificationMessageValidator`

**Infraestructura:**
- `ExceptionHandlingMiddleware` para manejo global de excepciones
- Actualizar `MappingProfile` con todos los DTOs
- Registrar servicios en `Program.cs`

**Esfuerzo Estimado:** ~7 horas

---

### **Fase 5 (FUTURA)**: Testing & Optimización
- [ ] Unit tests para servicios (InvoiceService, PaymentService)
- [ ] Integration tests para controllers
- [ ] Performance testing y optimización de queries
- [ ] Documentación API (Swagger/OpenAPI)

### **Fase 6 (FUTURA)**: Características Avanzadas
- [ ] Implementar CQRS si es necesario
- [ ] Caché distribuido para consultas frecuentes
- [ ] Auditoría centralizada en tiempo real
- [ ] Webhooks para notificaciones

---

## 📚 REFERENCIAS NORMATIVAS

- **NOM-004**: Requisitos de auditoría y trazabilidad ✅
- **LSSI-PC**: Datos confidenciales ✅
- **GDPR**: Derecho al olvido, consentimiento ✅
- **HIPAA**: Integridad de datos médicos 🟡 (mejorable con auditoría centralizada)

