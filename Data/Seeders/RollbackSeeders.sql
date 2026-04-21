-- =======================================================
-- RollbackSeeders.sql
-- Description: Elimina todos los datos sintéticos creados por DummyDataSeeder
-- =======================================================

BEGIN TRANSACTION;

-- 1. Eliminar Items de Recetas
DELETE FROM PrescriptionItems WHERE PrescriptionId IN (
    SELECT Id FROM Prescriptions WHERE DoctorId IN (
        SELECT Id FROM Users WHERE Email = 'doctor1@medpal.com'
    )
);

-- 2. Eliminar Recetas
DELETE FROM Prescriptions WHERE DoctorId IN (
    SELECT Id FROM Users WHERE Email = 'doctor1@medpal.com'
);

-- 3. Eliminar Invoices y Pagos ligados a esas citas (si las hubiera)
DELETE FROM Payments WHERE InvoiceId IN (
    SELECT Id FROM Invoices WHERE AppointmentId IN (
        SELECT Id FROM Appointments WHERE Notes LIKE '%Revisión mensual%' OR Notes LIKE '%Seguimiento%'
    )
);

DELETE FROM Invoices WHERE AppointmentId IN (
    SELECT Id FROM Appointments WHERE Notes LIKE '%Revisión mensual%' OR Notes LIKE '%Seguimiento%'
);

-- 4. Eliminar Citas
DELETE FROM Appointments WHERE Notes LIKE '%Revisión mensual%' OR Notes LIKE '%Seguimiento%';

-- 5. Eliminar Historial Medico
DELETE FROM MedicalHistories WHERE Diagnosis = 'Chequeo General';

-- 6. Eliminar Detalles de Paciente
DELETE FROM PatientDetails WHERE Id IN (
    SELECT PatientDetailsId FROM Patients WHERE Email LIKE 'paciente%@medpal.com'
);

-- 7. Eliminar Pacientes
DELETE FROM Patients WHERE Email LIKE 'paciente%@medpal.com';

-- 8. Eliminar Dummy Doctor
DELETE FROM UserRoles WHERE UserId IN (SELECT Id FROM Users WHERE Email = 'doctor1@medpal.com');
DELETE FROM Users WHERE Email = 'doctor1@medpal.com';

COMMIT;

PRINT 'Rollback Completado Satisfactoriamente.';
