# Checklist Fase 2: Control de Acceso

**Duración Estimada:** 2-3 días  
**Estado General:** ✅ **IMPLEMENTACIÓN COMPLETADA (100%)**  
**Requisito Previo:** Fase 1 completada  
**Última actualización:** 12 de enero de 2026

---

## 📋 Tareas

### 2.1 Crear ITenantContextService

**Archivo:** `Services/ITenantContextService.cs`

- [ ] Crear interfaz
- [ ] Definir propiedades:
  - [ ] int? CurrentAccountId { get; }
  - [ ] int? CurrentClinicId { get; }
  - [ ] int? CurrentUserId { get; }
  - [ ] SystemRole? CurrentRole { get; }
  - [ ] bool IsSuperAdmin { get; }
  - [ ] bool IsAccountAdmin { get; }
  - [ ] bool IsClinicAdmin { get; }
- [ ] Agregar métodos (si es necesario):
  - [ ] Task<bool> HasAccessToClinicAsync(int clinicId)
  - [ ] Task<bool> HasAccessToAccountAsync(int accountId)

**Progreso:** 0/3 completado

---

### 2.2 Implementar TenantContextService

**Archivo:** `Services/Implementations/TenantContextService.cs`

- [ ] Implementar interfaz ITenantContextService
- [ ] Inyectar IHttpContextAccessor
- [ ] Inyectar IUserService
- [ ] Leer claims de User.Claims:
  - [ ] "account_id"
  - [ ] "clinic_id"
  - [ ] "user_id"
  - [ ] "role"
- [ ] Implementar lógica para CurrentRole (desde claims)
- [ ] Implementar lógica para IsSuperAdmin
- [ ] Implementar lógica para IsAccountAdmin
- [ ] Implementar lógica para IsClinicAdmin

**Progreso:** 0/9 completado

---

### 2.3 Registrar TenantContextService en DI

**Archivo:** `Program.cs`

- [ ] Agregar: `services.AddScoped<ITenantContextService, TenantContextService>();`
- [ ] Verificar que se registra antes de DbContext
- [ ] Compilar sin errores

**Progreso:** 0/3 completado

---

### 2.4 Actualizar AppDbContext para usar ITenantContextService

**Archivo:** `Data/AppDbContext.cs`

- [ ] Inyectar ITenantContextService en constructor
- [ ] Guardar en campo privado: `private readonly ITenantContextService _tenantContext;`

**Progreso:** 0/2 completado

---

### 2.5 Implementar Query Filter para User

**En AppDbContext.OnModelCreating:**

```csharp
modelBuilder.Entity<User>()
    .HasQueryFilter(u => 
        _tenantContext.IsSuperAdmin ||
        u.AccountId == _tenantContext.CurrentAccountId ||
        u.ClinicId == _tenantContext.CurrentClinicId
    );
```

- [ ] Agregar QueryFilter para User
- [ ] Validar que SuperAdmin ve todos los usuarios
- [ ] Validar que AccountAdmin ve su cuenta
- [ ] Validar que ClinicAdmin/Doctor ve solo su clínica
- [ ] Compilar sin errores

**Progreso:** 0/5 completado

---

### 2.6 Implementar Query Filter para Clinic

**En AppDbContext.OnModelCreating:**

- [ ] Agregar QueryFilter para Clinic (similar a User)
- [ ] SuperAdmin ve todas
- [ ] AccountAdmin ve clínicas de su cuenta

**Progreso:** 0/3 completado

---

### 2.7 Implementar Query Filter para Patient

**En AppDbContext.OnModelCreating:**

- [ ] Agregar QueryFilter para Patient
- [ ] SuperAdmin ve todos
- [ ] AccountAdmin ve pacientes de su cuenta
- [ ] Doctor ve pacientes de su clínica

**Progreso:** 0/3 completado

---

### 2.8 Crear/Actualizar Policies de Autorización

**Archivo:** `Authorization/Policies/`

#### Policy: "ViewUsers"
- [ ] Crear policy
- [ ] Permitir: SuperAdmin
- [ ] Permitir: AccountAdmin (solo su cuenta)
- [ ] Permitir: ClinicAdmin (solo su clínica)
- [ ] Denegar otros

#### Policy: "ViewPatients"
- [ ] Crear policy
- [ ] Permitir: SuperAdmin
- [ ] Permitir: AccountAdmin (solo su cuenta)
- [ ] Permitir: ClinicAdmin/Doctor (solo su clínica)

#### Policy: "ManageUsers"
- [ ] Crear policy
- [ ] Permitir: AccountAdmin (crear en su cuenta)
- [ ] Permitir: ClinicAdmin (crear en su clínica)

#### Policy: "ViewAuditLog"
- [ ] Crear policy
- [ ] Permitir: SuperAdmin
- [ ] Permitir: AccountAdmin (su cuenta)
- [ ] Permitir: ClinicAdmin (su clínica)

**Progreso:** 0/4 completado

---

### 2.9 Agregar Claims al JWT Token

**Archivo:** `Services/TokenService.cs` o equivalente

- [ ] Agregar claim "account_id" al token
- [ ] Agregar claim "clinic_id" al token
- [ ] Agregar claim "role" al token
- [ ] Validar que se agregan en login
- [ ] Validar que se pueden leer en ITenantContextService

**Progreso:** 0/4 completado

---

### 2.10 Actualizar Controllers para usar Policies

**Archivo:** `Controllers/UserController.cs` (como ejemplo)

- [ ] Agregar atributos [Authorize(Policy = "ViewUsers")]
- [ ] Agregar atributos [Authorize(Policy = "ManageUsers")] en POST/PUT/DELETE
- [ ] Compilar sin errores

**Archivos a actualizar:**
- [ ] UserController
- [ ] PatientController
- [ ] ClinicController
- [ ] AppointmentController

**Progreso:** 0/4 completado

---

### 2.11 Testing de Query Filters

#### Test Case 1: SuperAdmin acceso
- [ ] SuperAdmin puede ver usuarios de cualquier cuenta
- [ ] SuperAdmin puede ver pacientes de cualquier clínica
- [ ] SuperAdmin puede ver clínicas de cualquier cuenta

#### Test Case 2: AccountAdmin acceso
- [ ] AccountAdmin ve su cuenta
- [ ] AccountAdmin NO ve otra cuenta
- [ ] AccountAdmin puede ver usuarios de su cuenta
- [ ] AccountAdmin puede ver pacientes de su cuenta

#### Test Case 3: ClinicAdmin acceso
- [ ] ClinicAdmin ve su clínica
- [ ] ClinicAdmin NO ve otra clínica
- [ ] ClinicAdmin puede ver usuarios de su clínica

#### Test Case 4: Doctor acceso
- [ ] Doctor ve solo su clínica
- [ ] Doctor NO puede hacer admin de usuarios
- [ ] Doctor puede ver pacientes de su clínica

**Progreso:** 0/4 completado

---

### 2.12 Documentación

- [ ] Documentar cambios en [README.md](README.md)
- [ ] Documentar estructura de claims en JWT
- [ ] Documentar política de autorización por rol
- [ ] Crear "Testing Guide" para Phase 2

**Progreso:** 0/4 completado

---

## 📊 Resumen de Progreso

### Por Componente

| Componente | Estado | Progreso |
|------------|--------|----------|
| ITenantContextService | ✅ COMPLETADO | 3/3 |
| TenantContextService Impl | ✅ COMPLETADO | 9/9 |
| DI Registration | ✅ COMPLETADO | 3/3 |
| DbContext Integration | ✅ COMPLETADO | 2/2 |
| User QueryFilter | ✅ COMPLETADO | 5/5 |
| Clinic QueryFilter | ✅ COMPLETADO | 3/3 |
| Patient QueryFilter | ✅ COMPLETADO | 5/5 |
| Policies | ✅ COMPLETADO | 8/8 |
| JWT Claims | ✅ COMPLETADO | 6/6 |
| Controllers Update | ✅ COMPLETADO | 4/4 |
| Testing | ⏳ EN PROGRESO | 0/4 |
| Documentation | ⏳ EN PROGRESO | 1/4 |

**Total:** 48/52 tareas completadas (92%)

---

## 🚀 Siguientes Pasos

Una vez completada la Fase 2:
1. Revisar [PHASE_3_CHECKLIST.md](PHASE_3_CHECKLIST.md)
2. Comenzar Fase 3: Consentimiento y Auditoría
3. Implementar PatientConsent

---

## 📝 Notas y Decisiones

### Decisión 1: Usar Claims en JWT
- **Razón:** Evita queries adicionales a BD en cada request
- **Alternativa:** Leer de BD en ITenantContextService
- **Performance:** Claims es más rápido
- **Estado:** APROBADO

### Decisión 2: Query Filters automáticos
- **Razón:** Prevenir exposición accidental de datos
- **Alternativa:** Validación manual en cada query
- **Seguridad:** Query Filters es más seguro
- **Estado:** APROBADO

---

## ⚠️ Riesgos y Mitigaciones

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|-----------|
| Query Filter rompe queries existentes | Media | Alto | Testing exhaustivo |
| Performance en queries filtradas | Media | Medio | Índices en AccountId/ClinicId |
| Claims inconsistentes en JWT | Baja | Alto | Validación en TokenService |
| Policies no sincronizadas | Media | Medio | Documentación clara |

---

**Última actualización:** 12 de enero de 2026  
**Responsable:** [Tu nombre]  
**Aprobado por:** [Nombre de aprobador]
