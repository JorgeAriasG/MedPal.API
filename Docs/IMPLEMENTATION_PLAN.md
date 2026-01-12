# Plan de Implementación Detallado

## Fase 1: Estructura Base (2-3 días)

### Objetivos
- [ ] Crear modelo Account
- [ ] Agregar AccountId a modelos existentes
- [ ] Redefinir sistema de Roles con enum
- [ ] Crear tablas base necesarias
- [ ] Generar y aplicar migrations
- [ ] Configurar relaciones

### Tareas

#### 1.1 Crear modelo Account
**Ubicación:** `Models/Account.cs`
```csharp
public class Account
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navegaciones
    public virtual ICollection<Clinic> Clinics { get; set; }
    public virtual ICollection<User> Users { get; set; }
    public virtual ICollection<Patient> Patients { get; set; }
}
```

#### 1.2 Crear enum de Roles
**Ubicación:** `Enums/SystemRole.cs`
```csharp
public enum SystemRole
{
    SuperAdmin = 1,
    AccountAdmin = 2,
    ClinicAdmin = 3,
    Doctor = 4,
    HealthProfessional = 5,
    Receptionist = 6,
    Patient = 7
}
```

#### 1.3 Actualizar modelo User
**Cambios en:** `Models/User.cs`
- Agregar: `public int? AccountId { get; set; }`
- Agregar: `public int? PrincipalClinicId { get; set; }`
- Agregar: `[ForeignKey("Account")] public virtual Account Account { get; set; }`

#### 1.4 Actualizar modelo Clinic
**Cambios en:** `Models/Clinic.cs`
- Agregar: `public int? AccountId { get; set; }`
- Agregar: `[ForeignKey("Account")] public virtual Account Account { get; set; }`

#### 1.5 Validar modelo Patient
**Archivo:** `Models/Patient.cs`

**Decisión:** Patient usa relación INDIRECTA a Account (Patient → Clinic → Account)
- VERIFICAR: `public virtual Clinic Clinic { get; set; }` existe
- VERIFICAR: `public int ClinicId { get; set; }` existe
- NO agregar AccountId (se obtiene vía `Patient.Clinic.Account`)
- Beneficio: Sin redundancia, integridad garantizada

#### 1.6 Crear Migration
```bash
dotnet ef migrations add Phase1_AccountAndRoles_Setup
dotnet ef database update
```

---

## Fase 2: Control de Acceso (2-3 días)

### Objetivos
- [ ] Implementar Query Filters automáticos
- [ ] Crear servicios de contexto (Tenant)
- [ ] Mejorar políticas de autorización
- [ ] Validar aislamiento de datos

### Tareas

#### 2.1 Crear ITenantContextService
**Ubicación:** `Services/ITenantContextService.cs`
```csharp
public interface ITenantContextService
{
    int? CurrentAccountId { get; }
    int? CurrentClinicId { get; }
    int? CurrentUserId { get; }
    SystemRole? CurrentRole { get; }
    bool IsSuperAdmin { get; }
    bool IsAccountAdmin { get; }
    bool IsClinicAdmin { get; }
}
```

#### 2.2 Implementar TenantContextService
**Ubicación:** `Services/Implementations/TenantContextService.cs`
- Leer claims de usuario
- Extraer Account, Clinic, Role
- Validar consistencia

#### 2.3 Agregar Query Filters en AppDbContext
**Cambios en:** `Data/AppDbContext.cs`

```csharp
// User filter
modelBuilder.Entity<User>()
    .HasQueryFilter(u => 
        _tenantContext.IsSuperAdmin ||
        u.AccountId == _tenantContext.CurrentAccountId ||
        u.ClinicId == _tenantContext.CurrentClinicId
    );

// Clinic filter
modelBuilder.Entity<Clinic>()
    .HasQueryFilter(c => 
        _tenantContext.IsSuperAdmin ||
        c.AccountId == _tenantContext.CurrentAccountId
    );

// Patient filter
modelBuilder.Entity<Patient>()
    .HasQueryFilter(p => 
        _tenantContext.IsSuperAdmin ||
        p.AccountId == _tenantContext.CurrentAccountId
    );
```

#### 2.4 Crear/Actualizar Policies
**Ubicación:** `Authorization/Policies/`
- ViewUsers Policy
- ViewPatients Policy
- ManageUsers Policy

#### 2.5 Testing de Query Filters
- Verificar que SuperAdmin ve todo
- Verificar que AccountAdmin ve su cuenta
- Verificar que ClinicAdmin ve su clínica

---

## Fase 3: Consentimiento y Auditoría (3-4 días)

### Objetivos
- [ ] Crear modelo PatientConsent
- [ ] Crear modelo MedicalRecordAccessLog
- [ ] Implementar servicios de consentimiento
- [ ] Auditoría automática en Medical Records
- [ ] Validar consentimientos en queries

### Tareas

#### 3.1 Crear modelo PatientConsent
**Ubicación:** `Models/PatientConsent.cs`
```csharp
public class PatientConsent
{
    public int Id { get; set; }
    public int PatientDetailsId { get; set; }
    public int RequestingClinicId { get; set; }
    public int OwnerClinicId { get; set; }
    
    // Qué puede ver: "AllRecords", "DiagnosisOnly", "PrescriptionsOnly"
    public string ConsentScope { get; set; } = "AllRecords";
    
    public bool IsApproved { get; set; }
    public DateTime ConsentDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int ApprovedByUserId { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

#### 3.2 Crear modelo MedicalRecordAccessLog
**Ubicación:** `Models/MedicalRecordAccessLog.cs`
```csharp
public class MedicalRecordAccessLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MedicalHistoryId { get; set; }
    public int PatientDetailsId { get; set; }
    public DateTime AccessTime { get; set; } = DateTime.UtcNow;
    
    public string Purpose { get; set; } // "Treatment", "Audit", "Administration"
    public int AccessingClinicId { get; set; }
    public int MedicalRecordOwnerClinicId { get; set; }
    public bool HadValidConsent { get; set; }
}
```

#### 3.3 Actualizar MedicalHistory
**Cambios en:** `Models/MedicalHistory.cs`
- Agregar: `public int OwnerClinicId { get; set; }`

#### 3.4 Crear IPatientConsentService
**Ubicación:** `Services/IPatientConsentService.cs`
- GetConsentAsync(patientId, clinicId)
- CreateConsentAsync(...)
- RevokeConsentAsync(...)
- IsConsentValidAsync(...)

#### 3.5 Crear IAuditLogService
**Ubicación:** `Services/IAuditLogService.cs`
- LogMedicalRecordAccessAsync(...)
- GetAccessHistoryAsync(...)

#### 3.6 Crear Migration Fase 3
```bash
dotnet ef migrations add Phase3_PatientConsent_And_Auditing
dotnet ef database update
```

#### 3.7 Query Filter para Medical Records
**En AppDbContext:**
```csharp
modelBuilder.Entity<MedicalHistory>()
    .HasQueryFilter(m =>
        _tenantContext.IsSuperAdmin ||
        m.OwnerClinicId == _tenantContext.CurrentClinicId ||
        (m.PatientDetails.Consents
            .Where(c => !c.IsDeleted)
            .Any(c => c.RequestingClinicId == _tenantContext.CurrentClinicId 
                   && c.IsApproved 
                   && (c.ExpiryDate == null || c.ExpiryDate > DateTime.UtcNow)))
    );
```

---

## Fase 4: Testing y Validación (2-3 días)

### Objetivos
- [ ] Unit tests de autorización
- [ ] Integration tests de aislamiento
- [ ] Security penetration testing
- [ ] Validación de cumplimiento normativo

### Tareas

#### 4.1 Unit Tests
- Tests de Query Filters
- Tests de Policy Authorization
- Tests de Consent Validation

#### 4.2 Integration Tests
- Crear datos de prueba (cuentas, clínicas, usuarios)
- Verificar aislamiento
- Verificar auditoría

#### 4.3 Security Testing
- SuperAdmin intentando ver Medical Records
- AccountAdmin intentando acceder a otra cuenta
- Doctor intentando acceder sin consentimiento

#### 4.4 Generación de Reporte de Cumplimiento
- Validación NOM-004
- Validación HIPAA
- Validación GDPR

---

## Resumen de Cambios por Archivo

| Archivo | Cambio | Fase |
|---------|--------|------|
| `Models/Account.cs` | CREAR | 1 |
| `Models/User.cs` | AGREGAR: AccountId, PrincipalClinicId | 1 |
| `Models/Clinic.cs` | AGREGAR: AccountId | 1 |
| `Models/Patient.cs` | AGREGAR: AccountId | 1 |
| `Models/MedicalHistory.cs` | AGREGAR: OwnerClinicId | 3 |
| `Models/PatientConsent.cs` | CREAR | 3 |
| `Models/MedicalRecordAccessLog.cs` | CREAR | 3 |
| `Enums/SystemRole.cs` | CREAR | 1 |
| `Services/ITenantContextService.cs` | CREAR | 2 |
| `Services/IPatientConsentService.cs` | CREAR | 3 |
| `Services/IAuditLogService.cs` | CREAR | 3 |
| `Data/AppDbContext.cs` | AGREGAR: Query Filters | 2-3 |
| `Authorization/Policies/` | CREAR: Nuevas políticas | 2 |
| `Migrations/` | CREAR: Fase 1, 3 | 1, 3 |

---

**Última actualización:** 12 de enero de 2026
