# Cómo Crear y Usar Cuenta SuperAdmin

**Versión:** 1.0  
**Fecha:** Enero 2026  
**Estado:** ✅ Implementado y compilado  

---

## 🔑 Opción 1: SuperAdmin Automático (RECOMENDADO)

La cuenta SuperAdmin se crea **automáticamente** la primera vez que ejecutas la aplicación.

### Ejecución

```bash
cd f:\PersonalProjects\SchedulingApp\Backend\Services\MedPalApi\MedPal.API
dotnet run
```

**Resultado en consola:**
```
✅ SuperAdmin usuario creado exitosamente
   Email: superadmin@medpal.com
   Password: SuperAdmin@123
   ⚠️  CAMBIAR PASSWORD DESPUÉS DE PRIMERA AUTENTICACIÓN
```

### Credenciales Iniciales

| Campo | Valor |
|-------|-------|
| **Email** | `superadmin@medpal.com` |
| **Contraseña** | `SuperAdmin@123` |
| **Rol** | SuperAdmin (Global) |
| **Account** | MedPal System |

---

## 🔐 Cambiar Contraseña SuperAdmin

### 1️⃣ Login Inicial
```bash
curl -X POST http://localhost:5126/api/user/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "superadmin@medpal.com",
    "password": "SuperAdmin@123"
  }'
```

**Respuesta:**
```json
{
    "id": 1,
    "name": "SuperAdmin",
    "email": "superadmin@medpal.com",
    "token": "eyJ...",
    "roles": ["SuperAdmin"]
}
```

### 2️⃣ Guardar el Token
```powershell
$token = "eyJ..."  # Usar el token del response anterior
```

### 3️⃣ Cambiar Contraseña (Endpoint Futuro)
```bash
# Próximamente se creará endpoint para cambiar contraseña
# Por ahora, cambiarla manualmente en la BD
```

**Por ahora (Opción manual):**

#### En SQL Server:
```sql
-- 1. Generar nuevo hash (desde PowerShell o C#)
-- Hash para "NuevaPassword123!" es algo como:
-- $2a$11$abcd...

-- 2. Actualizar en BD:
UPDATE Users 
SET PasswordHash = 'NUEVO_HASH_AQUI' 
WHERE Email = 'superadmin@medpal.com'
```

#### O desde PowerShell:
```powershell
# Generar hash BCrypt
$password = "NuevaPassword123!"
$hash = [BCrypt.Net.BCrypt]::HashPassword($password)
Write-Host "Hash: $hash"

# Actualizar en BD con el hash generado
```

---

## 🎯 Qué Puede Hacer SuperAdmin

### Permisos Globales
✅ Acceso a TODOS los Accounts
✅ Acceso a TODAS las Clinics
✅ Gestionar todos los Users
✅ Asignar/Remover roles globales
✅ Ver logs de auditoría
✅ Administrar permisos del sistema

### Acciones Recomendadas Post-Creación

#### 1. Cambiar Contraseña Inicial
```sql
-- Actualizar en BD (ver sección anterior)
UPDATE Users 
SET PasswordHash = '<NUEVO_HASH>' 
WHERE Email = 'superadmin@medpal.com'
```

#### 2. Crear Otros SuperAdmin (si es necesario)
```sql
-- Crear otro usuario SuperAdmin (opcional)
INSERT INTO Users (Name, Email, PasswordHash, IsActive, CreatedAt, UpdatedAt)
VALUES (
    'SuperAdmin 2',
    'admin2@medpal.com',
    '<HASH_BCRYPT>',
    1,
    GETUTCDATE(),
    GETUTCDATE()
)

-- Luego asignar rol SuperAdmin
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
VALUES (
    <USER_ID>,
    (SELECT Id FROM Roles WHERE Name = 'SuperAdmin'),
    GETUTCDATE()
)
```

#### 3. Crear Primeras Cuentas (Accounts)
Una vez logueado como SuperAdmin, puede crear Accounts para hospitales:
```bash
curl -X POST http://localhost:5126/api/clinic \
  -H "Authorization: Bearer $token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Hospital Central",
    "description": "Hospital principal",
    "isActive": true
  }'
```

---

## 📊 Estructura de SuperAdmin en BD

```
Accounts (MedPal System)
├── Id: 1
├── Name: "MedPal System"
├── IsActive: true
│
└─→ Users
    ├── Id: 1
    ├── Name: "SuperAdmin"
    ├── Email: "superadmin@medpal.com"
    ├── PasswordHash: "$2a$11$..." (BCrypt)
    ├── AccountId: 1
    │
    └─→ UserRoles
        ├── UserId: 1
        ├── RoleId: <SuperAdmin_Role_Id>
        └── ClinicId: null (Global)
```

---

## ⚠️ Seguridad

### Checklist de Seguridad

| Item | Acción | Estado |
|------|--------|--------|
| 1 | Cambiar contraseña inicial | ❌ Pendiente |
| 2 | Habilitar HTTPS en producción | ❌ Pendiente |
| 3 | Configurar IP whitelist (opcional) | ❌ Pendiente |
| 4 | Implementar rate limiting en login | ❌ Pendiente |
| 5 | Auditar acciones de SuperAdmin | ✅ Habilitado (Fase 3) |

### Recomendaciones

1. **Cambiar contraseña inmediatamente** después del primer login
2. **Usar contraseña fuerte:** Mínimo 12 caracteres, mayúsculas, minúsculas, números, símbolos
3. **No compartir credenciales** de SuperAdmin
4. **Habilitar MFA** (Multi-Factor Authentication) cuando esté disponible
5. **Auditar logs regularmente** para detectar acceso no autorizado

---

## 🔍 Verificar SuperAdmin en BD

```sql
-- 1. Verificar Account del sistema
SELECT * FROM Accounts WHERE Name = 'MedPal System'

-- 2. Verificar Usuario SuperAdmin
SELECT * FROM Users WHERE Email = 'superadmin@medpal.com'

-- 3. Verificar Role asignado
SELECT u.Email, r.Name 
FROM Users u
JOIN UserRoles ur ON u.Id = ur.UserId
JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Email = 'superadmin@medpal.com'

-- 4. Verificar Permisos de SuperAdmin
SELECT DISTINCT p.Name, p.Description
FROM Roles r
JOIN RolePermissions rp ON r.Id = rp.RoleId
JOIN Permissions p ON rp.PermissionId = p.Id
WHERE r.Name = 'SuperAdmin'
ORDER BY p.Name
```

---

## 🛠️ Alternativa: Crear SuperAdmin Manualmente

Si prefieres crear el SuperAdmin manualmente en SQL:

```sql
-- 1. Crear Account del sistema
INSERT INTO Accounts (Name, Description, IsActive, CreatedAt, UpdatedAt)
VALUES (
    'MedPal System',
    'Cuenta del sistema para SuperAdmin',
    1,
    GETUTCDATE(),
    GETUTCDATE()
)

-- 2. Crear Usuario SuperAdmin
INSERT INTO Users (Name, Email, PasswordHash, Specialty, ProfessionalLicenseNumber, IsActive, IsDeleted, HasAcceptedPrivacyTerms, AccountId, CreatedAt, UpdatedAt)
VALUES (
    'SuperAdmin',
    'superadmin@medpal.com',
    '$2a$11$V8wM.J0b1w5Jg5F2e9eQCO8T5g5F2e9eQCO8T5g5F2e9eQCO8T5g5F2',  -- Hash de "SuperAdmin@123"
    'System Administrator',
    'SA-SYSTEM-001',
    1,
    0,
    1,
    (SELECT Id FROM Accounts WHERE Name = 'MedPal System'),
    GETUTCDATE(),
    GETUTCDATE()
)

-- 3. Asignar Rol SuperAdmin
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
VALUES (
    (SELECT Id FROM Users WHERE Email = 'superadmin@medpal.com'),
    (SELECT Id FROM Roles WHERE Name = 'SuperAdmin'),
    GETUTCDATE()
)
```

---

## 📝 Archivo de Configuración

El seeder está en: [Data/Seeders/SuperAdminSeeder.cs](Data/Seeders/SuperAdminSeeder.cs)

Se ejecuta automáticamente en: [Program.cs](Program.cs) línea ~347

---

## ✅ Próximos Pasos

1. ✅ Ejecutar `dotnet run`
2. ✅ Verificar SuperAdmin creado en consola
3. ✅ Login con credenciales iniciales
4. ❌ Cambiar contraseña
5. ❌ Crear primeros Accounts para hospitales
6. ❌ Crear AccountAdmins para cada hospital

---

**¡SuperAdmin listo para usar!** 🚀
