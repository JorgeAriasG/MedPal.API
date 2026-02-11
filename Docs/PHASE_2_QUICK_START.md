# 🎯 Phase 2 - Control de Acceso (Guía Rápida)

**Fase Previa:** ✅ Phase 1 COMPLETADA  
**Estado Actual:** ⏳ Pendiente de Inicio  
**Duración Estimada:** 2-3 días  

---

## 📋 Visión General de Phase 2

### Objetivo Principal
Implementar **Query Filters** y **Políticas de Autorización** para garantizar que cada usuario solo puede acceder a datos de su Account/Clinic según su rol.

### Cambios Principales
1. **Query Filters** en AppDbContext (aislamiento de datos automático)
2. **ITenantContextService** (extraer contexto del usuario)
3. **Políticas de Autorización** (permisos granulares)
4. **Validaciones en Controllers** (seguridad en capas)

---

## 🔧 Componentes a Crear

### 1. ITenantContextService
**Propósito:** Extraer contexto de tenancy del usuario (Account, Clinic, Role)

**Ubicación:** `Services/ITenantContextService.cs`

**Responsabilidades:**
- Extraer `account_id` del JWT
- Extraer `clinic_id` del JWT (si aplica)
- Extraer `user_role` del JWT
- Validar consistencia entre tokens y DB
- Throw excepciones si tokens son inválidos

**Métodos esperados:**
```csharp
int? GetAccountId();
int? GetClinicId();
SystemRole GetUserRole();
int? GetUserId();
bool HasAccountAccess(int accountId);
bool HasClinicAccess(int clinicId);
```

### 2. TenantContextService (Implementación)
**Ubicación:** `Services/Implementations/TenantContextService.cs`

**Inyección:** 
- `IHttpContextAccessor` (para acceder a Claims)
- `AppDbContext` (para validaciones)
- `ILogger<TenantContextService>` (logging)

### 3. Query Filters (Global)
**Ubicación:** `Data/AppDbContext.cs` → método `OnModelCreating`

**Filtros a Implementar:**

```csharp
// Para User: mostrar solo usuarios de su Account
modelBuilder.Entity<User>()
    .HasQueryFilter(u => u.Account == null || u.AccountId == currentAccountId);

// Para Clinic: mostrar solo clínicas de su Account
modelBuilder.Entity<Clinic>()
    .HasQueryFilter(c => c.Account == null || c.AccountId == currentAccountId);

// Para Patient: mostrar solo pacientes de su Account/Clinic
// ↓ Depende del rol del usuario
```

**Nota:** Query Filters requieren `DbContextFactory` para acceder a scope actual

---

## 📍 Tareas Específicas (Por Subsección)

### 2.1 Crear ITenantContextService
- [ ] Crear interfaz `ITenantContextService.cs`
- [ ] Definir métodos de extracción de contexto
- [ ] Documentar cada método con XML
- [ ] Registrar en Dependency Injection

### 2.2 Implementar TenantContextService
- [ ] Crear clase `TenantContextService.cs`
- [ ] Implementar lógica de extracción de JWT claims
- [ ] Agregar validaciones de integridad
- [ ] Añadir logging para auditoría
- [ ] Manejar casos edge (usuario sin Account asignada)

### 2.3 Agregar Query Filters
- [ ] Implementar filtro para `User` (por Account)
- [ ] Implementar filtro para `Clinic` (por Account)
- [ ] Implementar filtro para `Patient` (por Account + Clinic)
- [ ] Implementar filtro para `Appointment` (por Account/Clinic/Patient)
- [ ] Pruebas: verificar que queries filtren correctamente

### 2.4 Crear Políticas de Autorización
- [ ] Policy: `ViewUsersPolicy` (scope-aware)
- [ ] Policy: `ViewPatientsPolicy` (scope-aware)
- [ ] Policy: `ViewAppointmentsPolicy` (scope-aware)
- [ ] Policy: `ManageUsersPolicy` (ClinicAdmin+)
- [ ] Policy: `ViewAuditLogPolicy` (AccountAdmin+)

### 2.5 Actualizar Controllers
- [ ] Agregar `[Authorize(Policy = "...")]` a métodos
- [ ] Inyectar `ITenantContextService`
- [ ] Validar contexto en operaciones CRUD
- [ ] Documentar cambios de seguridad

### 2.6 Tests de Seguridad
- [ ] User no puede ver otros Accounts
- [ ] ClinicAdmin no puede ver otros Clinics
- [ ] Patient no puede ver otros Patients
- [ ] SuperAdmin puede ver todo
- [ ] Query Filters funcionan correctamente

---

## 🔌 Puntos de Integración

### Dependency Injection (Startup)
**Archivo:** `Program.cs`

```csharp
// Después de registrar AppDbContext:
services.AddScoped<ITenantContextService, TenantContextService>();
services.AddAuthorizationPolicies(); // Método helper para crear políticas
```

### JWT Claims Esperados
```
{
  "account_id": 1,
  "clinic_id": 3,
  "user_id": 42,
  "role": "ClinicAdmin"
}
```

### Authorization Attributes
```csharp
[Authorize(Policy = "ViewPatientsPolicy")]
public IActionResult GetPatients() { ... }
```

---

## 📊 Arquitectura Phase 2

```
Request con JWT
    ↓
TenantContextService.Extract()
    ↓
Query Filters (AppDbContext)
    ↓
[Authorize(Policy = "...")] 
    ↓
Controller.ValidateContext()
    ↓
Response (solo datos permitidos)
```

---

## ⚠️ Consideraciones

1. **Query Filters Globales:** Afectan TODAS las queries (cuidado con admin operations)
2. **Performance:** Índices en AccountId y ClinicId son críticos
3. **Testing:** Tests deben validar aislamiento de datos
4. **Legacy Data:** Algunos datos pueden no tener AccountId aún
5. **Auditoría:** Loguear cambios de permisos

---

## 📚 Referencias Relacionadas

- [PHASE_2_CHECKLIST.md](PHASE_2_CHECKLIST.md) - Plan detallado
- [SECURITY_ARCHITECTURE.md](SECURITY_ARCHITECTURE.md) - Arquitectura general
- [PHASE_1_COMPLETION_SUMMARY.md](PHASE_1_COMPLETION_SUMMARY.md) - Lo que se completó

---

**Estado Actual:** Phase 1 ✅ → Phase 2 ⏳ → Listos para comenzar

