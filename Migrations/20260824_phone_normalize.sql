-- Phone normalization migration
-- Normalizes all phone numbers to E.164 format without '+' prefix (e.g., 529831800294)
-- Run against MedPalDB (production) and MedPalDBDev (local)

-- 1. Normalize patients.Phone: 10-digit numbers → prepend '52'
UPDATE Patients
SET Phone = '52' + Phone
WHERE LEN(Phone) = 10
  AND Phone NOT LIKE '52%'
  AND Phone LIKE '[0-9]%'
  AND IsDeleted = 0;

-- 2. Normalize patients.Phone: 13-digit numbers starting with '521' → remove the '1'
UPDATE Patients
SET Phone = '52' + SUBSTRING(Phone, 4, LEN(Phone))
WHERE LEN(Phone) = 13
  AND Phone LIKE '521%'
  AND IsDeleted = 0;

-- 3. Strip leading '+' if any
UPDATE Patients
SET Phone = SUBSTRING(Phone, 2, LEN(Phone))
WHERE Phone LIKE '+%'
  AND IsDeleted = 0;

-- 4. Normalize notification_recipients too
UPDATE NotificationMessages
SET Recipient = '52' + Recipient
WHERE LEN(Recipient) = 10
  AND Recipient NOT LIKE '52%'
  AND Recipient LIKE '[0-9]%';

UPDATE NotificationMessages
SET Recipient = '52' + SUBSTRING(Recipient, 4, LEN(Recipient))
WHERE LEN(Recipient) = 13
  AND Recipient LIKE '521%';

UPDATE NotificationMessages
SET Recipient = SUBSTRING(Recipient, 2, LEN(Recipient))
WHERE Recipient LIKE '+%';

-- Verify results
SELECT Phone, COUNT(*) AS Cnt
FROM Patients
WHERE IsDeleted = 0
GROUP BY Phone
ORDER BY Cnt DESC;
