-- Verification Script: Authorization System Tables
-- Run this in SQL Server Management Studio or Azure Data Studio

USE [MedPalDB]; -- Change to your database name
GO

-- ========== 1. VERIFY TABLES WERE CREATED ==========
PRINT '========== Checking if authorization tables exist =========='
SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_NAME IN ('Roles', 'Permissions', 'UserRoles', 'RolePermissions')
ORDER BY TABLE_NAME;
GO

-- ========== 2. VERIFY COLUMNS IN ROLES TABLE ==========
PRINT '========== Roles Table Structure =========='
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Roles'
ORDER BY ORDINAL_POSITION;
GO

-- ========== 3. VERIFY COLUMNS IN PERMISSIONS TABLE ==========
PRINT '========== Permissions Table Structure =========='
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Permissions'
ORDER BY ORDINAL_POSITION;
GO

-- ========== 4. VERIFY COLUMNS IN USERROLES TABLE ==========
PRINT '========== UserRoles Table Structure =========='
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'UserRoles'
ORDER BY ORDINAL_POSITION;
GO

-- ========== 5. VERIFY FOREIGN KEY CONSTRAINTS ==========
PRINT '========== Foreign Key Constraints =========='
SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn,
    fk.delete_referential_action_desc AS DeleteAction
FROM sys.foreign_keys AS fk
INNER JOIN sys.tables AS tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables AS tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns AS cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns AS cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
WHERE tp.name IN ('UserRoles', 'RolePermissions')
ORDER BY tp.name, fk.name;
GO

-- ========== 6. VERIFY UNIQUE INDEXES ==========
PRINT '========== Unique Indexes =========='
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    c.name AS ColumnName,
    i.is_unique AS IsUnique
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('Roles', 'Permissions')
    AND i.is_unique = 1
    AND i.is_primary_key = 0
ORDER BY t.name, i.name;
GO

-- ========== 7. VERIFY PRIMARY KEYS ==========
PRINT '========== Primary Keys =========='
SELECT 
    t.name AS TableName,
    i.name AS PrimaryKeyName,
    STRING_AGG(c.name, ', ') AS KeyColumns
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('Roles', 'Permissions', 'UserRoles', 'RolePermissions')
    AND i.is_primary_key = 1
GROUP BY t.name, i.name
ORDER BY t.name;
GO

PRINT '========== Verification Complete =========='
PRINT 'If all queries returned results, your authorization system tables are correctly configured!'
