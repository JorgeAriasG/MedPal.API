-- Phase 1: Migración de Datos - Asignar Account a datos existentes
-- Fecha: 12 de enero de 2026
-- Script idempotente: se puede ejecutar múltiples veces sin problemas

-- Opción: Crear una cuenta "Default" para datos legacy
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Name = 'Default Account')
BEGIN
    INSERT INTO Accounts (Name, Description, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Default Account', 'Cuenta por defecto para datos legacy - Fase 1', 1, GETUTCDATE(), GETUTCDATE());
    
    PRINT 'Cuenta Default creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Cuenta Default ya existe';
END

-- Obtener el ID de la cuenta Default
DECLARE @AccountId INT;
SELECT @AccountId = Id FROM Accounts WHERE Name = 'Default Account';

IF @AccountId IS NOT NULL
BEGIN
    BEGIN TRANSACTION;
    
    -- Asignar AccountId a usuarios que no lo tengan
    UPDATE Users 
    SET AccountId = @AccountId 
    WHERE AccountId IS NULL;
    
    PRINT 'Usuarios actualizados: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
    
    -- Asignar AccountId a clínicas que no lo tengan
    UPDATE Clinics 
    SET AccountId = @AccountId 
    WHERE AccountId IS NULL;
    
    PRINT 'Clínicas actualizadas: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
    
    -- Asignar AccountId a pacientes que no lo tengan
    UPDATE Patients 
    SET AccountId = @AccountId 
    WHERE AccountId IS NULL;
    
    PRINT 'Pacientes actualizados: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
    
    COMMIT TRANSACTION;
    PRINT 'Migración de datos completada exitosamente';
END
ELSE
BEGIN
    PRINT 'ERROR: No se pudo obtener el ID de la cuenta Default';
END

-- Verificación: Contar registros asignados
SELECT 
    'Users' AS Entity, 
    COUNT(*) AS Total, 
    SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END) AS WithAccount,
    SUM(CASE WHEN AccountId IS NULL THEN 1 ELSE 0 END) AS WithoutAccount
FROM Users

UNION ALL

SELECT 
    'Clinics' AS Entity, 
    COUNT(*) AS Total, 
    SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END),
    SUM(CASE WHEN AccountId IS NULL THEN 1 ELSE 0 END)
FROM Clinics

UNION ALL

SELECT 
    'Patients' AS Entity, 
    COUNT(*) AS Total, 
    SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END),
    SUM(CASE WHEN AccountId IS NULL THEN 1 ELSE 0 END)
FROM Patients;
