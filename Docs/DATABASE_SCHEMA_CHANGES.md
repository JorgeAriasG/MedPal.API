# Cambios en Base de Datos - Schema Evolution

**Documento de referencia para cambios en la estructura de la BD**  
**Última actualización:** 12 de enero de 2026

---

## Resumen de Cambios

Este documento detalla todos los cambios de schema que se harán en las 4 fases de implementación.

---

## Fase 1: Nuevas Tablas y Columnas

### Tabla Nueva: `Accounts`

```sql
CREATE TABLE Accounts (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE UNIQUE INDEX IX_Accounts_Name ON Accounts(Name);
CREATE INDEX IX_Accounts_IsActive ON Accounts(IsActive);
```

### Alteraciones: Tabla `Users`

```sql
ALTER TABLE Users ADD AccountId INT NULL;
ALTER TABLE Users ADD PrincipalClinicId INT NULL;
ALTER TABLE Users ADD CONSTRAINT FK_Users_AccountId 
    FOREIGN KEY (AccountId) REFERENCES Accounts(Id);

CREATE INDEX IX_Users_AccountId ON Users(AccountId);
CREATE INDEX IX_Users_PrincipalClinicId ON Users(PrincipalClinicId);
```

### Alteraciones: Tabla `Clinics`

```sql
ALTER TABLE Clinics ADD AccountId INT NULL;
ALTER TABLE Clinics ADD CONSTRAINT FK_Clinics_AccountId 
    FOREIGN KEY (AccountId) REFERENCES Accounts(Id);

CREATE INDEX IX_Clinics_AccountId ON Clinics(AccountId);
```

### Validación: Tabla `Patients`

**Decisión:** NO se agrega AccountId directo a Patients

**Razón:** Relación indirecta (Patient → Clinic → Account) evita:
- Redundancia de datos
- Inconsistencias de sincronización
- Violaciones de integridad referencial

**Cómo obtener AccountId de un paciente:**
```sql
-- Query SQL para obtener Account de un paciente
SELECT c.AccountId 
FROM Patients p
INNER JOIN Clinics c ON p.ClinicId = c.Id
WHERE p.Id = @PatientId;
```

**En C# (EF Core):**
```csharp
var patientAccount = patient.Clinic.Account;
```

**Índice existente:**
```sql
-- Ya debe existir (relación con Clinic)
CREATE INDEX IX_Patients_ClinicId ON Patients(ClinicId);
```

---

## Fase 3: Consentimiento y Auditoría

### Tabla Nueva: `PatientConsents`

```sql
CREATE TABLE PatientConsents (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PatientDetailsId INT NOT NULL,
    RequestingClinicId INT NOT NULL,
    OwnerClinicId INT NOT NULL,
    ConsentScope NVARCHAR(50) NOT NULL DEFAULT 'AllRecords',
    IsApproved BIT NOT NULL DEFAULT 0,
    ConsentDate DATETIME2 NOT NULL,
    ExpiryDate DATETIME2 NULL,
    ApprovedByUserId INT NOT NULL,
    
    -- Soft Delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedByUserId INT NULL,
    
    -- Auditoría
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedByUserId INT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    LastModifiedAt DATETIME2 NULL,
    LastModifiedByUserId INT NULL,
    
    CONSTRAINT FK_PatientConsents_PatientDetails 
        FOREIGN KEY (PatientDetailsId) REFERENCES PatientDetails(Id),
    CONSTRAINT FK_PatientConsents_RequestingClinic 
        FOREIGN KEY (RequestingClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_PatientConsents_OwnerClinic 
        FOREIGN KEY (OwnerClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_PatientConsents_ApprovedBy 
        FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_PatientConsents_PatientDetailsId ON PatientConsents(PatientDetailsId);
CREATE INDEX IX_PatientConsents_RequestingClinicId ON PatientConsents(RequestingClinicId);
CREATE INDEX IX_PatientConsents_OwnerClinicId ON PatientConsents(OwnerClinicId);
CREATE INDEX IX_PatientConsents_IsDeleted ON PatientConsents(IsDeleted);
CREATE INDEX IX_PatientConsents_IsApproved ON PatientConsents(IsApproved);
CREATE INDEX IX_PatientConsents_ExpiryDate ON PatientConsents(ExpiryDate);
```

### Tabla Nueva: `MedicalRecordAccessLogs`

```sql
CREATE TABLE MedicalRecordAccessLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    MedicalHistoryId INT NOT NULL,
    PatientDetailsId INT NOT NULL,
    AccessTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Purpose NVARCHAR(50) NOT NULL,  -- Treatment, Audit, Administration
    AccessingClinicId INT NOT NULL,
    MedicalRecordOwnerClinicId INT NOT NULL,
    HadValidConsent BIT NOT NULL,
    Reason NVARCHAR(500) NULL,
    IpAddress NVARCHAR(45) NULL,  -- IPv4 o IPv6
    
    CONSTRAINT FK_AccessLogs_User 
        FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_AccessLogs_MedicalHistory 
        FOREIGN KEY (MedicalHistoryId) REFERENCES MedicalHistories(Id),
    CONSTRAINT FK_AccessLogs_Patient 
        FOREIGN KEY (PatientDetailsId) REFERENCES PatientDetails(Id),
    CONSTRAINT FK_AccessLogs_AccessingClinic 
        FOREIGN KEY (AccessingClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_AccessLogs_OwnerClinic 
        FOREIGN KEY (MedicalRecordOwnerClinicId) REFERENCES Clinics(Id)
);

-- Índices críticos para búsquedas rápidas
CREATE INDEX IX_AccessLogs_PatientDetailsId ON MedicalRecordAccessLogs(PatientDetailsId);
CREATE INDEX IX_AccessLogs_UserId ON MedicalRecordAccessLogs(UserId);
CREATE INDEX IX_AccessLogs_MedicalHistoryId ON MedicalRecordAccessLogs(MedicalHistoryId);
CREATE INDEX IX_AccessLogs_AccessTime ON MedicalRecordAccessLogs(AccessTime);
CREATE INDEX IX_AccessLogs_AccessingClinicId ON MedicalRecordAccessLogs(AccessingClinicId);
CREATE INDEX IX_AccessLogs_Purpose ON MedicalRecordAccessLogs(Purpose);

-- Índice compuesto para búsquedas típicas
CREATE INDEX IX_AccessLogs_Patient_Time 
    ON MedicalRecordAccessLogs(PatientDetailsId, AccessTime DESC);
```

### Alteraciones: Tabla `MedicalHistories`

```sql
ALTER TABLE MedicalHistories ADD OwnerClinicId INT NOT NULL;
ALTER TABLE MedicalHistories ADD CONSTRAINT FK_MedicalHistories_OwnerClinic 
    FOREIGN KEY (OwnerClinicId) REFERENCES Clinics(Id);

CREATE INDEX IX_MedicalHistories_OwnerClinicId ON MedicalHistories(OwnerClinicId);
```

---

## Consideraciones de Migración de Datos

### Datos Existentes

**Problema:** Los datos existentes no tienen Account asignada

**Solución:**

```sql
-- Script para asignar Account a datos existentes
-- OPCIÓN 1: Todos en una sola cuenta (si es un único cliente)
BEGIN TRANSACTION;

-- Crear cuenta "Default" si no existe
INSERT INTO Accounts (Name, Description, IsActive)
VALUES ('Default Account', 'Cuenta por defecto para datos legacy', 1);

-- Asignar a todos los usuarios
UPDATE Users SET AccountId = 1 WHERE AccountId IS NULL;

-- Asignar a todas las clínicas
UPDATE Clinics SET AccountId = 1 WHERE AccountId IS NULL;

-- Asignar a todos los pacientes
UPDATE Patients SET AccountId = 1 WHERE AccountId IS NULL;

-- Asignar a todos los medical histories
UPDATE MedicalHistories SET OwnerClinicId = Clinics.Id
FROM MedicalHistories
INNER JOIN PatientDetails ON MedicalHistories.PatientDetailsId = PatientDetails.Id
INNER JOIN Patients ON PatientDetails.PatientId = Patients.Id
INNER JOIN Clinics ON Patients.ClinicId = Clinics.Id
WHERE MedicalHistories.OwnerClinicId IS NULL;

COMMIT;
```

**OPCIÓN 2:** Si tienes múltiples clientes independientes
```sql
-- Crear cuenta por cada cliente/organización
-- Mapear usuarios/clínicas/pacientes según su organización actual
-- (Requiere lógica de negocio específica)
```

---

## Índices Recomendados

### Para Performance de Queries Frecuentes

```sql
-- Búsquedas por Account
CREATE INDEX IX_Users_AccountId_IsDeleted 
    ON Users(AccountId, IsDeleted);
CREATE INDEX IX_Clinics_AccountId_IsDeleted 
    ON Clinics(AccountId, IsDeleted);
CREATE INDEX IX_Patients_AccountId_IsDeleted 
    ON Patients(AccountId, IsDeleted);

-- Búsquedas de consentimiento
CREATE INDEX IX_PatientConsents_Lookup 
    ON PatientConsents(PatientDetailsId, RequestingClinicId, OwnerClinicId, IsDeleted);

-- Búsquedas de auditoría (MÁS IMPORTANTE)
CREATE INDEX IX_AccessLogs_PatientID_Desc 
    ON MedicalRecordAccessLogs(PatientDetailsId, AccessTime DESC);
CREATE INDEX IX_AccessLogs_UserID_Desc 
    ON MedicalRecordAccessLogs(UserId, AccessTime DESC);
CREATE INDEX IX_AccessLogs_RecordID_Desc 
    ON MedicalRecordAccessLogs(MedicalHistoryId, AccessTime DESC);
```

---

## Consideraciones de Backup y Rollback

### Antes de Aplicar Migrations

1. **Backup completo de base de datos**
   ```sql
   BACKUP DATABASE MedPalDBDev 
   TO DISK = 'C:\Backups\MedPalDBDev_Pre_Phase1.bak'
   WITH INIT, COMPRESSION;
   ```

2. **Script de rollback** (guardar para cada fase)
   - Guardar estructura anterior
   - Documentar pasos de reversión

### Rollback de Cambios

Si es necesario revertir:

```sql
-- Rollback Fase 1 (DELETE order matter para FKs)
ALTER TABLE Users DROP CONSTRAINT FK_Users_AccountId;
ALTER TABLE Users DROP COLUMN AccountId, PrincipalClinicId;
ALTER TABLE Clinics DROP CONSTRAINT FK_Clinics_AccountId;
ALTER TABLE Clinics DROP COLUMN AccountId;
ALTER TABLE Patients DROP CONSTRAINT FK_Patients_AccountId;
ALTER TABLE Patients DROP COLUMN AccountId;
DROP TABLE Accounts;
```

---

## Validación Post-Migration

### Checklist después de aplicar cada migration

- [ ] Tablas nuevas existen
- [ ] Columnas nuevas existen con tipo correcto
- [ ] Foreign keys están configuradas
- [ ] Índices están creados
- [ ] Datos migrados correctamente
- [ ] No hay violaciones de constraints
- [ ] Queries funcionan sin timeout
- [ ] Performance es aceptable
- [ ] Backups actualizados

### Verificación SQL

```sql
-- Verificar tablas creadas
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Accounts', 'PatientConsents', 'MedicalRecordAccessLogs');

-- Verificar columnas
SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME IN ('AccountId', 'PrincipalClinicId');

-- Verificar índices
SELECT * FROM sys.indexes 
WHERE object_id = OBJECT_ID('dbo.MedicalRecordAccessLogs');

-- Verificar foreign keys
SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
WHERE TABLE_NAME IN ('PatientConsents', 'MedicalRecordAccessLogs')
AND CONSTRAINT_TYPE = 'FOREIGN KEY';

-- Contar registros migrados
SELECT 
    'Users' AS TableName, COUNT(*) AS TotalRecords, 
    SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END) AS WithAccount
FROM Users
UNION ALL
SELECT 
    'Clinics', COUNT(*), 
    SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END)
FROM Clinics
UNION ALL
SELECT 
    'Patients', COUNT(*), 
    SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END)
FROM Patients
UNION ALL
SELECT 
    'MedicalHistories', COUNT(*), 
    SUM(CASE WHEN OwnerClinicId IS NOT NULL THEN 1 ELSE 0 END)
FROM MedicalHistories;
```

---

## Información de Tamaño y Storage

### Estimación de Crecimiento

```
PatientConsents:
  - Estimado: 1-5 por paciente
  - Tamaño por fila: ~100 bytes
  - Con 10,000 pacientes: ~500 KB - 5 MB

MedicalRecordAccessLogs:
  - Estimado: 5-20 accesos por medical record
  - Tamaño por fila: ~150 bytes
  - Con 100,000 registros médicos: ~75 MB - 300 MB
  - NOTA: Crece constantemente, archivo en 6 meses: ~1-2 GB
  - RECOMENDACIÓN: Implementar archivado anual
```

### Estrategia de Archivado

```sql
-- Script para archivar logs antiguos (> 1 año)
CREATE TABLE MedicalRecordAccessLogs_Archive LIKE MedicalRecordAccessLogs;

-- Mover logs viejos
INSERT INTO MedicalRecordAccessLogs_Archive
SELECT * FROM MedicalRecordAccessLogs
WHERE AccessTime < DATEADD(YEAR, -1, GETUTCDATE());

-- Eliminar de tabla principal
DELETE FROM MedicalRecordAccessLogs
WHERE AccessTime < DATEADD(YEAR, -1, GETUTCDATE());
```

---

## Recursos Relacionados

- [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) - Plan de implementación
- [PHASE_1_CHECKLIST.md](PHASE_1_CHECKLIST.md) - Checklist Fase 1
- [PHASE_3_CHECKLIST.md](PHASE_3_CHECKLIST.md) - Checklist Fase 3
- [SECURITY_ARCHITECTURE.md](SECURITY_ARCHITECTURE.md) - Arquitectura

---

**Última actualización:** 12 de enero de 2026  
**Autor:** Sistema de Documentación  
**Estado:** Referencia para Implementación
