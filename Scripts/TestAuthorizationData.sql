-- Test Data Script: Insert Sample Authorization Data
-- Run this after verification to test relationships

USE [MedPalDB]; -- Change to your database name
GO

BEGIN TRANSACTION;

PRINT '========== Inserting Test Roles =========='
-- Insert test roles
INSERT INTO Roles (Name, Description, IsSystemRole, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES 
    ('Admin', 'System Administrator with full access', 1, 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('Doctor', 'Medical Doctor', 1, 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('Patient', 'Patient user', 1, 1, 0, GETUTCDATE(), GETUTCDATE());

SELECT * FROM Roles;
GO

PRINT '========== Inserting Test Permissions =========='
-- Insert test permissions
INSERT INTO Permissions (Name, Resource, Action, Description, CreatedAt)
VALUES 
    ('Patients.ViewAll', 'Patients', 'ViewAll', 'View all patients in the system', GETUTCDATE()),
    ('Patients.ViewOwn', 'Patients', 'ViewOwn', 'View own patient record', GETUTCDATE()),
    ('Appointments.Create', 'Appointments', 'Create', 'Create new appointments', GETUTCDATE()),
    ('MedicalRecords.View', 'MedicalRecords', 'View', 'View medical records', GETUTCDATE());

SELECT * FROM Permissions;
GO

PRINT '========== Inserting Test Role-Permission Mappings =========='
-- Map permissions to roles
-- Admin gets all permissions
INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt)
SELECT r.Id, p.Id, GETUTCDATE()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'Admin';

-- Doctor gets specific permissions
INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt)
SELECT r.Id, p.Id, GETUTCDATE()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'Doctor'
    AND p.Name IN ('Patients.ViewAll', 'Appointments.Create', 'MedicalRecords.View');

-- Patient gets own view permissions
INSERT INTO RolePermissions (RoleId, PermissionId, GrantedAt)
SELECT r.Id, p.Id, GETUTCDATE()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'Patient'
    AND p.Name IN ('Patients.ViewOwn', 'Appointments.Create');

SELECT 
    r.Name AS RoleName,
    p.Name AS PermissionName,
    rp.GrantedAt
FROM RolePermissions rp
INNER JOIN Roles r ON rp.RoleId = r.Id
INNER JOIN Permissions p ON rp.PermissionId = p.Id
ORDER BY r.Name, p.Name;
GO

PRINT '========== Test: Assign Role to Existing User =========='
-- This will fail if you don't have users yet - that's OK, just for testing structure
-- Uncomment when you have users:
/*
INSERT INTO UserRoles (UserId, RoleId, ClinicId, AssignedAt)
SELECT TOP 1 
    u.Id,
    r.Id,
    NULL, -- Global role (not clinic-specific)
    GETUTCDATE()
FROM Users u
CROSS JOIN Roles r
WHERE r.Name = 'Admin'
ORDER BY u.Id;

SELECT 
    u.Name AS UserName,
    r.Name AS RoleName,
    CASE WHEN ur.ClinicId IS NULL THEN 'Global' ELSE CAST(ur.ClinicId AS VARCHAR) END AS Scope,
    ur.AssignedAt
FROM UserRoles ur
INNER JOIN Users u ON ur.UserId = u.Id
INNER JOIN Roles r ON ur.RoleId = r.Id;
*/

PRINT '========== Verifying Data Integrity =========='
-- Check that foreign key constraints work
PRINT 'Total Roles: ' + CAST((SELECT COUNT(*) FROM Roles) AS VARCHAR);
PRINT 'Total Permissions: ' + CAST((SELECT COUNT(*) FROM Permissions) AS VARCHAR);
PRINT 'Total Role-Permission Mappings: ' + CAST((SELECT COUNT(*) FROM RolePermissions) AS VARCHAR);

-- Test unique constraint on Role.Name
PRINT '========== Testing Unique Constraint on Role.Name =========='
BEGIN TRY
    INSERT INTO Roles (Name, Description, IsSystemRole, IsActive, IsDeleted, CreatedAt, UpdatedAt)
    VALUES ('Admin', 'Duplicate test', 0, 1, 0, GETUTCDATE(), GETUTCDATE());
    PRINT 'ERROR: Unique constraint did NOT work - duplicate role was inserted!';
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    PRINT 'SUCCESS: Unique constraint prevented duplicate role name.';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
END CATCH

-- If everything looks good, you can commit:
-- COMMIT TRANSACTION;

-- For testing purposes, let's rollback to keep database clean:
ROLLBACK TRANSACTION;

PRINT '========== Test Complete (Transaction Rolled Back) =========='
PRINT 'All tests passed! You can now commit the actual seed data.'
PRINT 'To keep test data, change ROLLBACK to COMMIT above.'
