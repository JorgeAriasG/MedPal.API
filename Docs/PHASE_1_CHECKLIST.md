# Checklist Fase 1: Estructura Base y Roles

**Duración Estimada:** 2-3 días  
**Estado General:** ⏳ Pendiente  
**Última actualización:** 12 de enero de 2026

---

## 📋 Tareas

### 1.1 Crear modelo Account

- [ ] Crear archivo `Models/Account.cs`
- [ ] Definir propiedades:
  - [ ] Id (PK)
  - [ ] Name
  - [ ] Description
  - [ ] IsActive
  - [ ] CreatedAt
  - [ ] UpdatedAt
- [ ] Agregar navegaciones:
  - [ ] ICollection<Clinic> Clinics
  - [ ] ICollection<User> Users
  - [ ] ICollection<Patient> Patients
- [ ] Implementar ISoftDelete (opcional para Fase 1)

**Progreso:** 0/2 completado

---

### 1.2 Crear enum SystemRole

- [ ] Crear archivo `Enums/SystemRole.cs`
- [ ] Definir valores:
  - [ ] SuperAdmin = 1
  - [ ] AccountAdmin = 2
  - [ ] ClinicAdmin = 3
  - [ ] Doctor = 4
  - [ ] HealthProfessional = 5
  - [ ] Receptionist = 6
  - [ ] Patient = 7
- [ ] Agregar documentación en XML

**Progreso:** 0/1 completado

---

### 1.3 Actualizar modelo User

**Archivo:** `Models/User.cs`

- [ ] Agregar propiedad: `public int? AccountId { get; set; }`
- [ ] Agregar propiedad: `public int? PrincipalClinicId { get; set; }`
- [ ] Agregar navegación: `public virtual Account Account { get; set; }`
- [ ] Agregar atributo ForeignKey: `[ForeignKey("Account")]`
- [ ] Verificar que no rompe referencias existentes
- [ ] Validar relación con UserClinic

**Progreso:** 0/5 completado

---

### 1.4 Actualizar modelo Clinic

**Archivo:** `Models/Clinic.cs`

- [ ] Agregar propiedad: `public int? AccountId { get; set; }`
- [ ] Agregar navegación: `public virtual Account Account { get; set; }`
- [ ] Agregar atributo ForeignKey: `[ForeignKey("Account")]`
- [ ] Validar que no afecta clínicas existentes
- [ ] Hacer AccountId requerido en futuro (nullable por ahora para datos existentes)

**Progreso:** 0/4 completado

---

### 1.5 Validar modelo Patient

**Archivo:** `Models/Patient.cs`

**Decisión de Arquitectura:** Patient usa relación INDIRECTA a Account:
```
Patient → Clinic → Account
```
Patient obtiene AccountId a través de `Patient.Clinic.Account`

- [ ] VERIFICAR: Patient tiene relación con Clinic ✓
- [ ] VERIFICAR: `public virtual Clinic Clinic { get; set; }` existe
- [ ] VERIFICAR: `public int ClinicId { get; set; }` existe
- [ ] NO agregar AccountId directo a Patient (evitar redundancia)
- [ ] Validar relación con PatientDetails

**Notas:**
- Ventaja: Sin redundancia de datos
- Ventaja: Integridad garantizada
- Ventaja: Relación simple y clara
- Query Performance: Usar `.Include(p => p.Clinic)` en queries

**Progreso:** 0/5 completado

---

### 1.6 Actualizar AppDbContext

**Archivo:** `Data/AppDbContext.cs`

- [ ] Agregar DbSet: `public DbSet<Account> Accounts { get; set; }`
- [ ] Configurar Account en OnModelCreating
- [ ] Validar relaciones:
  - [ ] Account → Clinics (1:Many)
  - [ ] Account → Users (1:Many)
  - [ ] Account → Patients (1:Many)

**Progreso:** 0/3 completado

---

### 1.7 Generar Migration

**Comando:**
```bash
dotnet ef migrations add Phase1_AccountAndRoles_Setup
```

- [ ] Ejecutar comando de migration
- [ ] Revisar archivo generado `Migrations/[timestamp]_Phase1_AccountAndRoles_Setup.cs`
- [ ] Verificar:
  - [ ] Tabla Accounts creada
  - [ ] Columnas AccountId en User, Clinic, Patient
  - [ ] Foreign keys configuradas correctamente
  - [ ] Sin errores de SQL

**Progreso:** 0/4 completado

---

### 1.8 Aplicar Migration

**Comando:**
```bash
dotnet ef database update
```

- [ ] Ejecutar comando
- [ ] Verificar en SQL Server:
  - [ ] Tabla MedPalDBDev.dbo.Accounts existe
  - [ ] Columnas AccountId en User, Clinic, Patient
  - [ ] Sin errores de ejecución

**Progreso:** 0/2 completado

---

### 1.9 Compilar y Verificar

- [ ] `dotnet build` sin errores
- [ ] `dotnet build` sin advertencias críticas
- [ ] Verificar que no hay referencias rotas
- [ ] Ejecutar tests existentes (si hay)

**Progreso:** 0/4 completado

---

### 1.10 Crear Script de Migración de Datos (Opcional)

**Archivo:** `Scripts/Phase1_MigrateExistingDataToAccount.sql`

- [ ] Crear script SQL para asignar AccountId a datos existentes
- [ ] Script debe ser idempotente (ejecutable múltiples veces)
- [ ] Documentar asignación de cuentas:
  - [ ] ¿Todos los usuarios en misma cuenta?
  - [ ] ¿Todas las clínicas en misma cuenta?
- [ ] Ejecutar y validar

**Nota:** Puede hacerse antes o después del deploy a BD

**Progreso:** 0/3 completado

---

### 1.11 Documentación

- [ ] Actualizar [README.md](README.md) con progreso
- [ ] Documentar decisiones de diseño tomadas
- [ ] Crear documento de "Datos de Prueba" para Fase 1
- [ ] Documentar cualquier desviación del plan

**Progreso:** 0/3 completado

---

## 📊 Resumen de Progreso

### Por Componente

| Componente | Estado | Progreso |
|------------|--------|----------|
| Account Model | ⏳ Pendiente | 0/2 |
| SystemRole Enum | ⏳ Pendiente | 0/1 |
| User Updates | ⏳ Pendiente | 0/5 |
| Clinic Updates | ⏳ Pendiente | 0/4 |
| Patient Validation | ⏳ Pendiente | 0/5 |
| DbContext | ⏳ Pendiente | 0/3 |
| Migration | ⏳ Pendiente | 0/4 |
| Database Update | ⏳ Pendiente | 0/2 |
| Build & Verify | ⏳ Pendiente | 0/4 |
| Data Migration | ⏳ Pendiente | 0/3 |
| Documentation | ⏳ Pendiente | 0/3 |

**Total:** 0/36 tareas completadas (0%)

**Cambios respecto al plan original:**
- 1.5: Patient NO recibe AccountId (relación indirecta vía Clinic)
- Permite arquitectura más limpia sin redundancia

---

## 🚀 Siguientes Pasos

Una vez completada la Fase 1:
1. Revisar [PHASE_2_CHECKLIST.md](PHASE_2_CHECKLIST.md)
2. Comenzar Fase 2: Control de Acceso
3. Implementar Query Filters

---

## 📝 Notas y Decisiones

### Decisión 1: AccountId Nullable
- **Razón:** Los datos existentes no tienen Account asignada
- **Acción:** Hacer nullable en Fase 1, requerido en Fase 2
- **Fecha:** 12 enero 2026

### Decisión 2: PrincipalClinicId en User
- **Razón:** Los usuarios solo tienen una clínica "principal"
- **Alternativa:** Usar relación directo con Clinic
- **Estado:** APROBADO

---

## ⚠️ Riesgos y Mitigaciones

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|-----------|
| Datos existentes sin Account | Alta | Medio | Script SQL de migración |
| Foreign key constraints | Media | Alto | Revisar migration antes de aplicar |
| Performance en queries | Media | Medio | Índices en AccountId |
| Datos rotos en migration | Baja | Crítico | Backup antes de aplicar |

---

**Última actualización:** 12 de enero de 2026  
**Responsable:** [Tu nombre]  
**Aprobado por:** [Nombre de aprobador]
