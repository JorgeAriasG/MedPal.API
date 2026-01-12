# Checklist Fase 3: Consentimiento y Auditoría

**Duración Estimada:** 3-4 días  
**Estado General:** ⏳ Pendiente  
**Requisito Previo:** Fase 1 y 2 completadas  
**Última actualización:** 12 de enero de 2026

---

## 📋 Tareas

### 3.1 Crear modelo PatientConsent

**Archivo:** `Models/PatientConsent.cs`

- [ ] Crear clase
- [ ] Definir propiedades:
  - [ ] int Id (PK)
  - [ ] int PatientDetailsId (FK)
  - [ ] int RequestingClinicId (FK a Clinic)
  - [ ] int OwnerClinicId (FK a Clinic)
  - [ ] string ConsentScope (default "AllRecords")
  - [ ] bool IsApproved
  - [ ] DateTime ConsentDate
  - [ ] DateTime? ExpiryDate (nullable)
  - [ ] int ApprovedByUserId (FK a User - el paciente)
- [ ] Implementar ISoftDelete:
  - [ ] bool IsDeleted
  - [ ] DateTime? DeletedAt
  - [ ] int? DeletedByUserId
- [ ] Implementar IAuditableEntity:
  - [ ] DateTime CreatedAt
  - [ ] int? CreatedByUserId
  - [ ] DateTime? UpdatedAt
  - [ ] int? UpdatedByUserId
  - [ ] DateTime? LastModifiedAt
  - [ ] int? LastModifiedByUserId
- [ ] Agregar navegaciones:
  - [ ] public virtual PatientDetails PatientDetails { get; set; }
  - [ ] public virtual Clinic RequestingClinic { get; set; }
  - [ ] public virtual Clinic OwnerClinic { get; set; }
  - [ ] public virtual User ApprovedByUser { get; set; }

**Progreso:** 0/4 completado

---

### 3.2 Crear modelo MedicalRecordAccessLog

**Archivo:** `Models/MedicalRecordAccessLog.cs`

- [ ] Crear clase
- [ ] Definir propiedades:
  - [ ] int Id (PK)
  - [ ] int UserId (FK a User)
  - [ ] int MedicalHistoryId (FK a MedicalHistory)
  - [ ] int PatientDetailsId (FK a PatientDetails)
  - [ ] DateTime AccessTime (default DateTime.UtcNow)
  - [ ] string Purpose ("Treatment", "Audit", "Administration")
  - [ ] int AccessingClinicId (FK a Clinic)
  - [ ] int MedicalRecordOwnerClinicId (FK a Clinic)
  - [ ] bool HadValidConsent
  - [ ] string? Reason (nullable, para documentar por qué)
  - [ ] string? IpAddress (nullable)
- [ ] Agregar navegaciones:
  - [ ] public virtual User User { get; set; }
  - [ ] public virtual MedicalHistory MedicalHistory { get; set; }
  - [ ] public virtual PatientDetails PatientDetails { get; set; }
  - [ ] public virtual Clinic AccessingClinic { get; set; }
  - [ ] public virtual Clinic OwnerClinic { get; set; }
- [ ] **IMPORTANTE:** No implementar soft delete en AccessLog (nunca borrar logs)

**Progreso:** 0/4 completado

---

### 3.3 Actualizar modelo MedicalHistory

**Archivo:** `Models/MedicalHistory.cs`

- [ ] Agregar propiedad: `public int OwnerClinicId { get; set; }`
- [ ] Agregar atributo ForeignKey: `[ForeignKey("Clinic")]`
- [ ] Agregar navegación: `public virtual Clinic OwnerClinic { get; set; }`
- [ ] Validar que no rompe datos existentes
- [ ] Hacer OwnerClinicId requerido (non-nullable)

**Progreso:** 0/5 completado

---

### 3.4 Crear interfaz IPatientConsentService

**Archivo:** `Services/IPatientConsentService.cs`

- [ ] Crear interfaz
- [ ] Definir métodos:
  - [ ] Task<PatientConsent> CreateConsentAsync(int patientId, int requestingClinicId, int ownerClinicId, string scope, int approvedByUserId)
  - [ ] Task<PatientConsent> GetConsentAsync(int patientId, int requestingClinicId, int ownerClinicId)
  - [ ] Task<IEnumerable<PatientConsent>> GetPatientConsentsAsync(int patientId)
  - [ ] Task<bool> IsConsentValidAsync(int patientId, int requestingClinicId, int ownerClinicId)
  - [ ] Task RevokeConsentAsync(int consentId, int revokedByUserId)
  - [ ] Task<IEnumerable<PatientConsent>> GetConsentHistoryAsync(int patientId)

**Progreso:** 0/1 completado

---

### 3.5 Implementar PatientConsentService

**Archivo:** `Services/Implementations/PatientConsentService.cs`

- [ ] Implementar IPatientConsentService
- [ ] Inyectar IRepository<PatientConsent>
- [ ] Inyectar IUnitOfWork
- [ ] Implementar CreateConsentAsync:
  - [ ] Validar que patientId existe
  - [ ] Validar que clínicas existen
  - [ ] Crear record de consentimiento
  - [ ] Guardar en BD
- [ ] Implementar GetConsentAsync:
  - [ ] Buscar por clinics + patient
  - [ ] Retornar null si no existe
- [ ] Implementar IsConsentValidAsync:
  - [ ] Validar que existe
  - [ ] Validar que IsApproved == true
  - [ ] Validar que no está expirado (ExpiryDate > Now)
  - [ ] Validar que no está soft deleted
- [ ] Implementar RevokeConsentAsync:
  - [ ] Usar soft delete (IsDeleted = true)
  - [ ] Registrar quién revocó

**Progreso:** 0/8 completado

---

### 3.6 Crear interfaz IAuditLogService

**Archivo:** `Services/IAuditLogService.cs`

- [ ] Crear interfaz
- [ ] Definir métodos:
  - [ ] Task LogMedicalRecordAccessAsync(int userId, int medicalHistoryId, int patientId, string purpose, int accessingClinicId, int ownerClinicId, bool hadConsent, string? reason, string? ipAddress)
  - [ ] Task<IEnumerable<MedicalRecordAccessLog>> GetAccessHistoryAsync(int patientId)
  - [ ] Task<IEnumerable<MedicalRecordAccessLog>> GetAccessHistoryByRecordAsync(int medicalHistoryId)
  - [ ] Task<IEnumerable<MedicalRecordAccessLog>> GetAccessHistoryByUserAsync(int userId)

**Progreso:** 0/1 completado

---

### 3.7 Implementar AuditLogService

**Archivo:** `Services/Implementations/AuditLogService.cs`

- [ ] Implementar IAuditLogService
- [ ] Inyectar IRepository<MedicalRecordAccessLog>
- [ ] Inyectar IRepository<MedicalHistory>
- [ ] Inyectar IHttpContextAccessor (para IP)
- [ ] Implementar LogMedicalRecordAccessAsync:
  - [ ] Crear registro
  - [ ] Guardar IP del cliente
  - [ ] Guardar timestamp actual
  - [ ] Guardar en BD inmediatamente
- [ ] Implementar GetAccessHistoryAsync:
  - [ ] Buscar todos los accesos para paciente
  - [ ] Retornar ordenado por fecha DESC
- [ ] Implementar GetAccessHistoryByRecordAsync:
  - [ ] Buscar accesos para un registro médico específico
  - [ ] Retornar con usuario y clínica info
- [ ] Implementar GetAccessHistoryByUserAsync:
  - [ ] Buscar qué registros accedió un usuario
  - [ ] Útil para auditoría

**Progreso:** 0/8 completado

---

### 3.8 Registrar servicios en DI

**Archivo:** `Program.cs`

- [ ] Agregar: `services.AddScoped<IPatientConsentService, PatientConsentService>();`
- [ ] Agregar: `services.AddScoped<IAuditLogService, AuditLogService>();`
- [ ] Compilar sin errores

**Progreso:** 0/3 completado

---

### 3.9 Actualizar AppDbContext

**Archivo:** `Data/AppDbContext.cs`

- [ ] Agregar DbSet: `public DbSet<PatientConsent> PatientConsents { get; set; }`
- [ ] Agregar DbSet: `public DbSet<MedicalRecordAccessLog> MedicalRecordAccessLogs { get; set; }`
- [ ] Configurar en OnModelCreating:
  - [ ] PatientConsent relaciones y fluent API
  - [ ] MedicalRecordAccessLog relaciones y fluent API
- [ ] Agregar QueryFilter para PatientConsent (solo no-deleted):
  ```csharp
  modelBuilder.Entity<PatientConsent>()
      .HasQueryFilter(pc => !pc.IsDeleted);
  ```

**Progreso:** 0/4 completado

---

### 3.10 Crear Query Filter para MedicalHistory (con Consent)

**En AppDbContext.OnModelCreating:**

```csharp
modelBuilder.Entity<MedicalHistory>()
    .HasQueryFilter(m =>
        _tenantContext.IsSuperAdmin ||
        m.OwnerClinicId == _tenantContext.CurrentClinicId ||
        (m.PatientDetails.PatientConsents
            .Where(c => !c.IsDeleted)
            .Any(c => c.RequestingClinicId == _tenantContext.CurrentClinicId 
                   && c.IsApproved 
                   && (c.ExpiryDate == null || c.ExpiryDate > DateTime.UtcNow)))
    );
```

- [ ] Agregar filtro con consentimiento
- [ ] Validar que query es correcta
- [ ] Compilar sin errores
- [ ] Testing de filtro

**Progreso:** 0/4 completado

---

### 3.11 Crear interceptor de auditoría

**Archivo:** `Data/AuditInterceptor.cs` (Opcional pero RECOMENDADO)

- [ ] Crear SaveChangesInterceptor
- [ ] Inyectar IAuditLogService
- [ ] Interceptar guardar de MedicalHistory
- [ ] Registrar automáticamente acceso

**Progreso:** 0/1 completado

---

### 3.12 Crear Controller para gestionar consentimientos

**Archivo:** `Controllers/ConsentController.cs`

- [ ] POST /api/consents (crear consentimiento)
- [ ] GET /api/consents/{patientId} (mis consentimientos)
- [ ] DELETE /api/consents/{consentId} (revocar)
- [ ] GET /api/consents/{consentId}/history (historial)
- [ ] Agregar [Authorize] apropiados
- [ ] Documentación en XML

**Progreso:** 0/1 completado

---

### 3.13 Crear Controller para auditoría

**Archivo:** `Controllers/AuditLogController.cs`

- [ ] GET /api/audit/my-access-history (mi historial)
- [ ] GET /api/audit/patient/{patientId}/access-history (accesos a paciente)
- [ ] GET /api/audit/record/{recordId}/access-history (accesos a registro)
- [ ] Solo SuperAdmin y AccountAdmin pueden ver
- [ ] Agregar paginación

**Progreso:** 0/1 completado

---

### 3.14 Generar Migration

**Comando:**
```bash
dotnet ef migrations add Phase3_PatientConsent_And_Auditing
```

- [ ] Ejecutar comando
- [ ] Revisar archivo generado
- [ ] Verificar:
  - [ ] Tabla PatientConsents creada
  - [ ] Tabla MedicalRecordAccessLogs creada
  - [ ] OwnerClinicId en MedicalHistories
  - [ ] Foreign keys correctas
  - [ ] Índices en AccessLog (importante por queries)

**Progreso:** 0/4 completado

---

### 3.15 Aplicar Migration

**Comando:**
```bash
dotnet ef database update
```

- [ ] Ejecutar comando
- [ ] Verificar en SQL Server:
  - [ ] Tablas creadas
  - [ ] Columnas correctas
  - [ ] Relaciones funcionales
- [ ] Sin errores

**Progreso:** 0/3 completado

---

### 3.16 Actualizar MedicalHistoryController

**Archivo:** `Controllers/MedicalHistoryController.cs`

- [ ] Inyectar IPatientConsentService
- [ ] Inyectar IAuditLogService
- [ ] En GetMedicalHistory:
  - [ ] Validar acceso (policy)
  - [ ] Registrar acceso en AuditLog
  - [ ] Retornar con info de consentimiento (si aplica)
- [ ] En GetMedicalHistoryById:
  - [ ] Validar acceso
  - [ ] Registrar acceso
- [ ] En CreateMedicalHistory:
  - [ ] Asignar OwnerClinicId automáticamente
- [ ] En UpdateMedicalHistory:
  - [ ] Registrar acceso

**Progreso:** 0/5 completado

---

### 3.17 Testing de Consentimientos

#### Test Case 1: Crear consentimiento
- [ ] Doctor A puede crear consentimiento
- [ ] Paciente recibe notificación (opcional)
- [ ] Consentimiento guardado en BD

#### Test Case 2: Acceso sin consentimiento
- [ ] Doctor de Clínica B NO puede ver registros de Clínica A
- [ ] Error 403 retornado

#### Test Case 3: Acceso con consentimiento válido
- [ ] Doctor de Clínica B PUEDE ver registros de Clínica A si tiene consent
- [ ] Acceso registrado en AuditLog

#### Test Case 4: Consentimiento expirado
- [ ] Consentimiento con ExpiryDate pasada es inválido
- [ ] Error 403 retornado

#### Test Case 5: Consentimiento revocado
- [ ] Soft delete del consentimiento
- [ ] Doctor NO puede acceder después de revocación

#### Test Case 6: Auditoría registrada
- [ ] Cada acceso se registra en MedicalRecordAccessLog
- [ ] Contiene: usuario, timestamp, clínica, propósito
- [ ] No se puede borrar

**Progreso:** 0/6 completado

---

### 3.18 Documentación

- [ ] Actualizar [README.md](README.md)
- [ ] Crear "Consent Management Guide"
- [ ] Crear "Audit Log Guide"
- [ ] Documentar flujo de consentimiento
- [ ] Documentar flujo de auditoría

**Progreso:** 0/5 completado

---

## 📊 Resumen de Progreso

### Por Componente

| Componente | Estado | Progreso |
|------------|--------|----------|
| PatientConsent Model | ⏳ Pendiente | 0/4 |
| AccessLog Model | ⏳ Pendiente | 0/4 |
| MedicalHistory Update | ⏳ Pendiente | 0/5 |
| IPatientConsentService | ⏳ Pendiente | 0/1 |
| PatientConsentService | ⏳ Pendiente | 0/8 |
| IAuditLogService | ⏳ Pendiente | 0/1 |
| AuditLogService | ⏳ Pendiente | 0/8 |
| DI Registration | ⏳ Pendiente | 0/3 |
| DbContext Update | ⏳ Pendiente | 0/4 |
| MedicalHistory QueryFilter | ⏳ Pendiente | 0/4 |
| AuditInterceptor | ⏳ Pendiente | 0/1 |
| ConsentController | ⏳ Pendiente | 0/1 |
| AuditLogController | ⏳ Pendiente | 0/1 |
| Migration | ⏳ Pendiente | 0/4 |
| Database Update | ⏳ Pendiente | 0/3 |
| MedicalHistoryController Update | ⏳ Pendiente | 0/5 |
| Testing | ⏳ Pendiente | 0/6 |
| Documentation | ⏳ Pendiente | 0/5 |

**Total:** 0/68 tareas completadas (0%)

---

## 🚀 Siguientes Pasos

Una vez completada la Fase 3:
1. Revisar [PHASE_4_TESTING.md](PHASE_4_TESTING.md)
2. Comenzar Fase 4: Testing integral
3. Validar seguridad completa

---

## 📝 Notas y Decisiones

### Decisión 1: PatientConsent Explícito
- **Razón:** Cumple NOM-004, da control al paciente
- **Alternativa:** Acceso automático entre clínicas
- **Estado:** APROBADO

### Decisión 2: Auditoría sin soft delete
- **Razón:** Nunca se debe permitir borrar evidencia legal
- **Alternativa:** Soft delete con auditoría de borración
- **Estado:** APROBADO

### Decisión 3: AccessLog inmediato
- **Razón:** No permitir que se pierdan logs por fallos
- **Alternativa:** Batch logging asincrónico
- **Estado:** APROBADO

---

## ⚠️ Riesgos y Mitigaciones

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|-----------|
| Logs crecen muy rápido | Media | Medio | Índices + archivado periódico |
| Query de consentimientos lenta | Media | Medio | Índices en RequestingClinicId |
| Consentimiento no se valida | Baja | Crítico | Unit tests exhaustivos |
| Datos no asignados a OwnerClinic | Alta | Alto | Script migración datos |

---

**Última actualización:** 12 de enero de 2026  
**Responsable:** [Tu nombre]  
**Aprobado por:** [Nombre de aprobador]
