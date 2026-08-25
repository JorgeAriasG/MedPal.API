-- Add OriginalAppointmentId column for reschedule tracking
-- Cancel + create new pattern: new appointment links to the cancelled original

ALTER TABLE Appointments
ADD OriginalAppointmentId int NULL;

ALTER TABLE Appointments
ADD CONSTRAINT FK_Appointments_OriginalAppointment
FOREIGN KEY (OriginalAppointmentId) REFERENCES Appointments(Id);
