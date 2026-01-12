# Arquitectura de Seguridad - MedPal Multi-Tenancy

## 1. Jerarquía de Roles

```
┌─────────────────────────────────────────────────────────────┐
│              SUPERADMIN (Sistema completo)                  │
│  • Gestión de cuentas, clínicas y usuarios                 │
│  • Auditoría y logs del sistema                            │
│  • ✗ NO acceso a Medical Records (NOM-004)                 │
│  • ✗ NO acceso a Datos de Pacientes                        │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│          ACCOUNTADMIN (Admin de Cuenta)                     │
│  • Gestión de múltiples clínicas en su cuenta              │
│  • Gestión de todos los usuarios de su cuenta              │
│  • Auditoría de su cuenta                                  │
│  • ✗ NO acceso directo a Medical Records                   │
│  • ✓ Meta-datos de pacientes (para reporte)                │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│         CLINICADMIN (Admin de Clínica) [OPCIONAL]          │
│  • Gestión de personal de su clínica                       │
│  • Auditoría de su clínica                                 │
│  • ✗ NO acceso a Medical Records                           │
│  • ✓ Meta-datos de pacientes                               │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│        DOCTOR / HEALTH_PROFESSIONAL                        │
│  • Acceso a Medical Records de su clínica                  │
│  • ✓ Registros de pacientes que atiende                    │
│  • ✓ Acceso con consentimiento de otras clínicas           │
│  • Auditoría automática de accesos                         │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│     RECEPTIONIST / SUPPORT                                 │
│  • Datos de contacto y citas                               │
│  • ✗ NO acceso a Medical Records                           │
│  • ✓ Ver pacientes de su clínica                           │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│              PATIENT (Paciente)                             │
│  • Acceso a sus propios registros médicos                  │
│  • Gestionar consentimientos                               │
│  • Ver historial de accesos a sus datos                    │
└─────────────────────────────────────────────────────────────┘
```

## 2. Conceptos Clave

### 2.1 Cuenta (Account)
- **Definición:** Grupo de clínicas, usuarios y pacientes bajo una organización
- **Ejemplos:** "Hospital XYZ Grupo", "Clínica Privada ABC"
- **Aislamiento:** Los datos de Cuenta A nunca se mezclan con Cuenta B
- **Admin:** AccountAdmin tiene acceso total a su cuenta

### 2.2 Clínica (Clinic)
- **Definición:** Ubicación física o entidad de salud
- **Pertenencia:** Una o más clínicas por cuenta
- **Isolamiento:** Los registros médicos de Clínica A no son visibles en Clínica B por defecto
- **Admin:** ClinicAdmin (opcional) gestiona su clínica

### 2.3 Paciente Multi-Clínica
```
Paciente Juan García (ID: 1001)
  ├── Clínica A (Cardiólogo)
  │   └── Medical Records (aislados)
  │       • Consulta cardiología - 2024-01-10
  │       • ECG - 2024-01-10
  │
  ├── Clínica B (Dentista)
  │   └── Medical Records (aislados)
  │       • Limpieza dental - 2024-02-15
  │       • Radiografía - 2024-02-15
  │
  └── Clínica C (Nutricionista)
      └── Medical Records (aislados)
          • Evaluación nutricional - 2024-03-01

Regla de Seguridad:
  • Cardiólogo (Clínica A) ✓ VE: sus registros
  • Cardiólogo (Clínica A) ✗ NO VE: registros Clínica B (sin consentimiento)
  • Paciente CONTROLA consentimientos de cada clínica
```

### 2.4 Consentimiento de Paciente
- **Tipo:** Explícito y revocable
- **Granular:** Por clínica, por tipo de dato, por tiempo
- **Ejemplos:**
  - "Cardiólogo de Clínica A puede ver diagnósticos (hasta 31 dic 2026)"
  - "Nutricionista puede ver solo recetas"
  - "Permitir acceso: Sí / No"

### 2.5 Auditoría de Acceso
```
Evento: Doctor Juan accede a Medical Record #5042
  ├── Timestamp: 2024-01-15 10:30:45 UTC
  ├── Usuario: Juan (ID: 203, Doctor)
  ├── Clínica Origen: Clínica A
  ├── Recurso: Medical Record #5042
  ├── Clínica Dueña: Clínica A
  ├── Paciente: García (ID: 1001)
  ├── Motivo: "Treatment" / "AuditCheck" / "SupportIntervention"
  ├── ¿Tuvo Consentimiento?: Sí / No
  └── IP: 192.168.1.100
  
⚠️ NUNCA se borra, se registra todo
```

## 3. Matriz de Permisos

| Rol | Ver Usuarios | Ver Pacientes | Ver Medical Records | Ver Auditoría | Crear Usuarios |
|-----|--------------|---------------|---------------------|---------------|----------------|
| SuperAdmin | ✓ Todos | ✗ | ✗ | ✓ Total | ✓ |
| AccountAdmin | ✓ Su cuenta | ✓ Su cuenta | ✗ | ✓ Su cuenta | ✓ Su cuenta |
| ClinicAdmin | ✓ Su clínica | ✓ Su clínica | ✗ | ✓ Su clínica | ✓ Su clínica |
| Doctor | ✓ Su clínica | ✓ Su clínica | ✓ Su clínica + consent | ✓ Limitado | ✗ |
| Receptionist | ✓ Su clínica | ✓ Su clínica | ✗ | ✗ | ✗ |
| Patient | ✗ | ✓ Propio | ✓ Propio | ✓ Sus accesos | ✗ |

## 4. Flujos de Seguridad

### 4.1 Acceso a Medical Record (Autorización)

```
ENTRADA: User=Doctor, MedicalRecord=Cardio_Patient_123

PASO 1: ¿El usuario está autenticado?
  ✗ → Rechazar (401)

PASO 2: ¿El usuario tiene rol de Doctor?
  ✗ → Rechazar (403)

PASO 3: ¿La clínica dueña == clínica del doctor?
  ✓ → PERMITIR + Registrar acceso
  ✗ → Ir a PASO 4

PASO 4: ¿Existe PatientConsent válido?
  ✓ → PERMITIR + Registrar acceso
  ✗ → Rechazar (403)

PASO 5: ¿El consentimiento ha expirado?
  ✓ → Rechazar (403 - Consent expired)
  
FIN: Log inmediato del acceso
```

### 4.2 Consulta de Usuarios (Query Filter Automático)

```
ENTRADA: GET /api/users (SuperAdmin hace la solicitud)

APLICAR QUERY FILTERS:
  WHERE (
    -- SuperAdmin ve todos
    User.IsSuperAdmin == true
    OR
    -- AccountAdmin ve su cuenta
    (User.AccountId == CurrentUser.AccountId)
    OR
    -- ClinicAdmin ve su clínica
    (User.ClinicId == CurrentUser.ClinicId)
  )
  AND IsDeleted == false

RESULTADO: Automáticamente filtrado, seguro
```

## 5. Componentes de Implementación

### 5.1 Modelos (Cambios en BD)

```csharp
// Modelos nuevos
- Account (Cuenta)
- PatientConsent (Consentimiento)
- MedicalRecordAccessLog (Auditoría)
- SystemAuditLog (Auditoría general)

// Cambios en modelos existentes
- User: agregar AccountId, PrincipalClinicId
- Role: cambiar a enum SystemRole
- Clinic: agregar AccountId
- Patient: agregar AccountId
- MedicalHistory: agregar OwnerClinicId
```

### 5.2 Servicios

```csharp
- IAccountService (CRUD cuentas)
- IPatientConsentService (Gestionar consentimientos)
- IAuditLogService (Registrar accesos)
- IAuthorizationService (Mejorado con ABAC)
- ITenantContextService (Obtener tenant actual)
```

### 5.3 Políticas de Autorización

```csharp
- "ViewUsers" → Solo su scope (cuenta/clínica)
- "ViewMedicalRecord" → Con validación de clínica + consent
- "CreateUser" → Dentro de su scope
- "AccessAuditLog" → Según rol y scope
- "ManageConsent" → Solo paciente + AccountAdmin
```

### 5.4 Query Filters

```csharp
- User → Filtrar por Account/Clinic según rol
- Patient → Filtrar por Account/Clinic
- MedicalHistory → Filtrar por OwnerClinicId + consentimientos
- Clinic → Filtrar por AccountId
```

## 6. Estándares de Cumplimiento

| Estándar | Requisito | Implementación |
|----------|-----------|-----------------|
| **NOM-004 (México)** | Confidencialidad de datos médicos | Aislamiento por clínica + consentimiento |
| **HIPAA (USA)** | Acceso limitado y auditable | Logs de acceso inmediatos |
| **GDPR (Europa)** | Derecho a olvido + consentimiento | SoftDelete + consents revocables |
| **SOC 2** | Control de acceso granular | RBAC + ABAC implementation |

## 7. Decisiones Arquitectónicas

### ✅ SI: Consentimiento Explícito
Razón: Cumple NOM-004, permite privacidad del paciente, mejor UX

### ✅ SI: Auditoría en BD
Razón: No se puede borrar, evidencia legal, trazabilidad total

### ✓ OPCIONAL: ClinicAdmin
Razón: Escalabilidad en clínicas grandes, pero no obligatorio inicialmente

### ✗ NO: SuperAdmin viendo Medical Records directamente
Razón: Riesgo legal, viola NOM-004, usar logs de auditoría en su lugar

---

**Última actualización:** 12 de enero de 2026
