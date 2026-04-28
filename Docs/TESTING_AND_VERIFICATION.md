# Testing & Verification Guide - MedPal.API

**Estado**: ✅ Backend completo (Fases 1-3)  
**Última Actualización**: 12 de enero de 2026  
**Versión**: 1.0

---

## 📋 Tabla de Contenidos

1. [Quick Start - Verificación Rápida](#quick-start)
2. [Flujo de Registro y Roles](#flujo-de-registro-y-roles)
3. [Crear Super Admin](#crear-super-admin)
4. [Verificación de Funcionalidad](#verificación-de-funcionalidad)
5. [Testing por Endpoint](#testing-por-endpoint)
6. [Troubleshooting](#troubleshooting)

---

## ⚡ Quick Start - Verificación Rápida

### 1. Ejecutar la Aplicación

```bash
cd f:\PersonalProjects\SchedulingApp\Backend\Services\MedPalApi\MedPal.API
dotnet run
```

**Esperado:**
- La aplicación inicia en `https://localhost:5126`
- Swagger disponible en `https://localhost:5126/swagger`

### 2. Crear Primer Usuario (Super Admin)

**Endpoint**: `POST /api/user/register`

```bash
curl -X POST "https://localhost:5126/api/user/register" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Admin Principal",
    "email": "admin@medpal.com",
    "password": "AdminPassword123!",
    "acceptPrivacyTerms": true,
    "specialty": "System Administrator",
    "professionalLicenseNumber": "ADMIN-001"
  }'
```

**Respuesta (200 Created):**

```json
{
  "id": 1,
  "name": "Admin Principal",
  "email": "admin@medpal.com",
  "isActive": true,
  "hasAcceptedPrivacyTerms": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "createdAt": "2026-01-12T12:00:00Z",
  "updatedAt": "2026-01-12T12:00:00Z"
}
```

### 3. Login y Verificación

**Endpoint**: `POST /api/user/login`

```bash
curl -X POST "https://localhost:5126/api/user/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@medpal.com",
    "password": "AdminPassword123!"
  }'
```

### 4. Verificar Rol Asignado

Decodifica el JWT en [jwt.io](https://jwt.io) - Deberías ver:

```json
{
  "role": "Admin",
  "account_id": null,
  "clinic_id": null,
  "user_id": 1,
  "sub": "admin@medpal.com",
  "email": "admin@medpal.com",
  "iat": 1673528400,
  "exp": 1673531400
}
```

---

## 🔄 Flujo de Registro y Roles

### Diagrama del Flujo

```
┌─────────────────────────────────────────────────────────┐
│         POST /api/user/register (AllowAnonymous)        │
├─────────────────────────────────────────────────────────┤
│  1. Validar DTO (nombre, email, password, privacy)     │
│  2. Verificar email no existe                          │
│  3. Crear nuevo User con datos                         │
│  4. Guardar en BD                                      │
│  5. Obtener rol "Admin"                                │
│  6. Asignar rol al usuario (clinicId = null)          │
│  7. Generar JWT con rol "Admin"                        │
│  8. Retornar User + Token                              │
└─────────────────────────────────────────────────────────┘
```

### Lo Que Sucede en Segundo Plano

#### 1. **Creación de Usuario**

```csharp
// En UserController.Register()
var newUser = new User
{
    Name = registerDto.Name,
    Email = registerDto.Email,
    PasswordHash = registerDto.Password,  // Se hashea en AddUserAsync
    Specialty = registerDto.Specialty,
    ProfessionalLicenseNumber = registerDto.ProfessionalLicenseNumber,
    IsActive = true,
    IsDeleted = false,
    HasAcceptedPrivacyTerms = registerDto.AcceptPrivacyTerms,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
    // AccountId = null  (SIN CUENTA MULTI-TENANT ASIGNADA)
};
```

#### 2. **Asignación de Rol**

```csharp
// Obtener el rol "Admin"
var adminRole = await _roleRepository.GetRoleByNameAsync("Admin");

// Asignar GLOBALMENTE (sin clínica específica)
await _roleRepository.AssignRoleToUserAsync(
    createdUser.Id, 
    adminRole.Id, 
    clinicId: null,              // ← IMPORTANTE: null = acceso global
    expiresAt: null, 
    assignedByUserId: null       // ← Sin auditoría para primer admin
);
```

#### 3. **Generación de JWT**

```csharp
// En TokenService.GenerateToken()
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Email, user.Email),
    new Claim("sub", user.Email),
    new Claim("user_id", user.Id.ToString()),
    new Claim("role", userRole.Name)  // ← Rol: "Admin"
    // account_id: no incluido si user.AccountId es null
};
```

### Estados Posibles al Registrar

| Caso | AccountId | ClinicId | Rol | Acceso |
|------|-----------|----------|-----|--------|
| **Nuevo Usuario** | `null` | `null` | `Admin` | ✅ Global (todas las clínicas) |
| **Sin asignación** | `null` | `null` | Sin rol | ❌ Sin acceso |
| **Usuario Clínica** | `1` | `5` | `Doctor` | ⚠️ Solo esa clínica |

---

## 👑 Crear Super Admin

### Opción 1: Primer Registro (Automático)

El **PRIMER usuario registrado automáticamente es Admin**.

```bash
# Este usuario será Admin automáticamente
curl -X POST "https://localhost:5126/api/user/register" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Primera Persona Registrada",
    "email": "first@medpal.com",
    "password": "Password123!",
    "acceptPrivacyTerms": true
  }'
```

### Opción 2: Cambiar Rol de Usuario Existente

Una vez que tenemos un Admin, podemos cambiar el rol de otros usuarios.

**Endpoint**: `PUT /api/role/assign`

```bash
# El Admin asigna rol "Admin" a otro usuario
curl -X PUT "https://localhost:5126/api/role/assign" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 2,
    "roleId": 1,  # ID del rol Admin
    "clinicId": null
  }'
```

### Opción 3: Seeder Personalizado (Recomendado para Development)

Editar [AuthorizationSeeder.cs](AuthorizationSeeder.cs) para incluir super admin:

```csharp
// En Data/Seeders/AuthorizationSeeder.cs - Nuevo método
private static async Task SeedSuperAdminAsync(AppDbContext context)
{
    // Crear cuenta super admin
    var superAdminAccount = new Account
    {
        Name = "Super Admin Account",
        Description = "Cuenta del administrador del sistema",
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    if (!await context.Accounts.AnyAsync(a => a.Name == superAdminAccount.Name))
    {
        await context.Accounts.AddAsync(superAdminAccount);
        await context.SaveChangesAsync();
    }

    // Obtener la cuenta creada
    superAdminAccount = await context.Accounts
        .FirstOrDefaultAsync(a => a.Name == superAdminAccount.Name);

    // Crear usuario super admin
    var superAdmin = new User
    {
        Name = "System Super Admin",
        Email = "superadmin@medpal.local",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!"),
        IsActive = true,
        HasAcceptedPrivacyTerms = true,
        AccountId = superAdminAccount.Id,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    if (!await context.Users.AnyAsync(u => u.Email == superAdmin.Email))
    {
        await context.Users.AddAsync(superAdmin);
        await context.SaveChangesAsync();
    }

    // Asignar rol Admin
    var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
    if (adminRole != null)
    {
        var userRole = new UserRole
        {
            UserId = superAdmin.Id,
            RoleId = adminRole.Id,
            AssignedAt = DateTime.UtcNow
        };

        if (!await context.UserRoles.AnyAsync(ur => 
            ur.UserId == superAdmin.Id && ur.RoleId == adminRole.Id))
        {
            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();
        }
    }
}

// Llamar en SeedAsync()
public static async Task SeedAsync(AppDbContext context)
{
    await SeedRolesAsync(context);
    await SeedPermissionsAsync(context);
    await SeedRolePermissionsAsync(context);
    await SeedSuperAdminAsync(context);  // ← AGREGAR
    await context.SaveChangesAsync();
}
```

---

## ✅ Verificación de Funcionalidad

### Checklist Completo

#### 1. Base de Datos

- [ ] SQL Server corriendo en `(local)`
- [ ] Base de datos `MedPal` creada
- [ ] Todas las tablas existen (58+ migraciones aplicadas)
- [ ] Datos de seeder presentes:
  - [ ] 5 Roles: Admin, Doctor, Nurse, Receptionist, Patient
  - [ ] 28+ Permisos
  - [ ] RolePermissions mapeadas correctamente

```sql
-- Verificar en SQL Server Management Studio
SELECT COUNT(*) FROM Roles;                    -- Debe ser 5
SELECT COUNT(*) FROM Permissions;              -- Debe ser 28+
SELECT COUNT(*) FROM RolePermissions;          -- Debe ser 100+
SELECT COUNT(*) FROM Accounts;                 -- Debe ser 1 (Default Account)
```

#### 2. Autenticación

- [ ] Registro sin token (AllowAnonymous) funciona
- [ ] Login sin token (AllowAnonymous) funciona
- [ ] JWT generado contiene claims correctos
- [ ] Token expira según configuración (30 min por defecto)

```bash
# Decodificar JWT en https://jwt.io
# Verificar payload:
{
  "role": "Admin",           # ← Rol asignado
  "user_id": 1,              # ← ID del usuario
  "sub": "email@test.com",
  "email": "email@test.com",
  "iat": 1673528400,         # ← Emitido en
  "exp": 1673531400          # ← Expira en
}
```

#### 3. Autorización

- [ ] Endpoints protegidos requieren token válido
- [ ] Endpoints no protegidos funcionan sin token
- [ ] Permisos se verifican correctamente
- [ ] Usuarios sin permiso reciben 403 Forbidden

```bash
# SIN token → 401 Unauthorized
curl -X GET "https://localhost:5126/api/user" 

# CON token → 200 OK o 403 Forbidden según permisos
curl -X GET "https://localhost:5126/api/user" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### 4. Multi-Tenancy (Fase 1)

- [ ] Cuenta "Default Account" creada en el seeder
- [ ] Usuarios nuevos se asignan a Account (o null)
- [ ] Query filters aislan datos por Account
- [ ] Admin ve solo sus datos, no los de otras cuentas

```sql
-- Verificar query filter aplicado
SELECT u.Id, u.Name, u.AccountId 
FROM Users u 
WHERE u.AccountId = 1 OR u.AccountId IS NULL;
```

#### 5. Consentimiento y Auditoría (Fase 3)

- [ ] Tabla `PatientConsents` existe y funciona
- [ ] Tabla `MedicalRecordAccessLogs` existe (inmutable)
- [ ] ConsentService crea/actualiza consentimientos
- [ ] Acceso a registros médicos se audita

```sql
-- Verificar tablas Fase 3
SELECT COUNT(*) FROM PatientConsents;
SELECT COUNT(*) FROM MedicalRecordAccessLogs;
```

---

## 🧪 Testing por Endpoint

### 1. Autenticación

#### A. Registro

**Endpoint**: `POST /api/user/register`  
**Autenticación**: ❌ No requerida (AllowAnonymous)

```bash
curl -X POST "https://localhost:5126/api/user/register" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Juan Pérez",
    "email": "juan@example.com",
    "password": "JuanPass123!",
    "acceptPrivacyTerms": true,
    "specialty": "Cardiólogo",
    "professionalLicenseNumber": "LIC-12345"
  }'
```

**Esperado**: 201 Created + JWT Token

#### B. Login

**Endpoint**: `POST /api/user/login`  
**Autenticación**: ❌ No requerida

```bash
curl -X POST "https://localhost:5126/api/user/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "juan@example.com",
    "password": "JuanPass123!"
  }'
```

**Esperado**: 200 OK + JWT Token

### 2. Usuarios

#### A. Ver Todos (Requiere: Users.ViewAll)

**Endpoint**: `GET /api/user`  
**Autenticación**: ✅ Requerida (Rol: Admin)

```bash
curl -X GET "https://localhost:5126/api/user" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -H "Content-Type: application/json"
```

**Esperado**: 200 OK + Lista de usuarios

#### B. Ver Uno

**Endpoint**: `GET /api/user/{id}`  
**Autenticación**: ✅ Requerida

```bash
curl -X GET "https://localhost:5126/api/user/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Esperado**: 200 OK + Usuario

#### C. Crear Usuario

**Endpoint**: `POST /api/user`  
**Autenticación**: ✅ Requerida (Requiere: Users.Manage)

```bash
curl -X POST "https://localhost:5126/api/user" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "María García",
    "email": "maria@example.com",
    "password": "MariaPass123!",
    "specialty": "Neuróloga",
    "professionalLicenseNumber": "LIC-67890"
  }'
```

**Esperado**: 201 Created

### 3. Roles

#### A. Ver Roles

**Endpoint**: `GET /api/role`  
**Autenticación**: ✅ Requerida (Requiere: Roles.View)

```bash
curl -X GET "https://localhost:5126/api/role" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Respuesta esperada**:
```json
[
  {
    "id": 1,
    "name": "Admin",
    "description": "System administrator with full access",
    "isSystemRole": true,
    "isActive": true
  },
  {
    "id": 2,
    "name": "Doctor",
    "description": "Medical doctor with access to patient records",
    "isSystemRole": true,
    "isActive": true
  }
]
```

#### B. Asignar Rol

**Endpoint**: `PUT /api/role/assign`  
**Autenticación**: ✅ Requerida (Requiere: Users.ManageRoles)

```bash
curl -X PUT "https://localhost:5126/api/role/assign" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 2,
    "roleId": 2,
    "clinicId": null,
    "expiresAt": null
  }'
```

**Esperado**: 200 OK + Audit log creado

#### C. Ver Audit Log de Roles

**Endpoint**: `GET /api/role/audit-logs`  
**Autenticación**: ✅ Requerida (Requiere: Roles.ViewAudit)

```bash
curl -X GET "https://localhost:5126/api/role/audit-logs" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🔍 Verificación de Permisos

### Tabla de Permisos por Rol

| Permiso | Admin | Doctor | Nurse | Receptionist | Patient |
|---------|-------|--------|-------|--------------|---------|
| Patients.ViewAll | ✅ | ✅ | ❌ | ✅ | ❌ |
| Patients.ViewOwn | ✅ | ✅ | ❌ | ❌ | ✅ |
| Appointments.ViewAll | ✅ | ✅ | ✅ | ✅ | ❌ |
| Appointments.ViewOwn | ✅ | ✅ | ✅ | ✅ | ✅ |
| MedicalRecords.ViewAll | ✅ | ❌ | ❌ | ❌ | ❌ |
| MedicalRecords.ViewAssigned | ✅ | ✅ | ✅ | ❌ | ❌ |
| MedicalRecords.ViewOwn | ✅ | ✅ | ❌ | ❌ | ✅ |
| Users.Manage | ✅ | ❌ | ❌ | ❌ | ❌ |
| Roles.Assign | ✅ | ❌ | ❌ | ❌ | ❌ |

### Verificar Permisos en BD

```sql
-- Ver permisos de un rol
SELECT p.Name, p.Resource, p.Action
FROM RolePermissions rp
JOIN Permissions p ON rp.PermissionId = p.Id
JOIN Roles r ON rp.RoleId = r.Id
WHERE r.Name = 'Admin'
ORDER BY p.Resource;

-- Ver roles de un usuario
SELECT r.Name, r.Description
FROM UserRoles ur
JOIN Roles r ON ur.RoleId = r.Id
WHERE ur.UserId = 1;
```

---

## 🔧 Troubleshooting

### Problema: "El rol de administrador no está configurado"

**Causa**: El seeder no ejecutó correctamente.

**Solución**:

```bash
# 1. Eliminar las migraciones
dotnet ef database drop -f

# 2. Crear migraciones desde cero
dotnet ef migrations add InitMigration

# 3. Aplicar migraciones
dotnet ef database update

# 4. Ejecutar seeder manualmente
# En Program.cs se ejecuta automáticamente:
// Seed authorization data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AuthorizationSeeder.SeedAsync(context);
}

# 5. Reiniciar aplicación
dotnet run
```

### Problema: "Email ya está registrado"

**Causa**: El email ya existe en la BD.

**Solución**:

```sql
-- Verificar usuario existente
SELECT * FROM Users WHERE Email = 'admin@medpal.com';

-- Eliminar si es necesario (development only!)
DELETE FROM Users WHERE Email = 'admin@medpal.com';

-- O simplemente usar otro email en el registro
```

### Problema: "JWT inválido" o "Token expirado"

**Causa**: Token expirado o malformado.

**Solución**:

```bash
# 1. Verificar token en https://jwt.io
# 2. Decodificar y ver fecha de expiración
# 3. Si está expirado, hacer login nuevamente:

curl -X POST "https://localhost:5126/api/user/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@medpal.com",
    "password": "AdminPassword123!"
  }'

# 4. Usar el nuevo token
```

### Problema: "403 Forbidden" en endpoint protegido

**Causa**: Usuario no tiene permiso.

**Solución**:

```bash
# 1. Verificar JWT - decodificar y ver "role"
# 2. Verificar permiso requerido en el endpoint
# 3. Asignar permiso al rol del usuario:

# Primero, como Admin, asignar rol adecuado
curl -X PUT "https://localhost:5126/api/role/assign" \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 2,
    "roleId": 2,  # Cambiar a Doctor, Nurse, etc.
    "clinicId": null
  }'

# 4. Hacer login de nuevo para obtener nuevo JWT
```

### Problema: CORS error al llamar desde Angular

**Solución**: Verificar que `appsettings.Development.json` incluya:

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200"]
  }
}
```

Y en `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:4200")
                         .AllowAnyHeader()
                         .AllowAnyMethod());
});

app.UseCors("AllowSpecificOrigin");
```

---

## 📊 Resumen de Estado

### Backend: ✅ COMPLETADO

| Fase | Estado | Detalles |
|------|--------|----------|
| **Phase 1** | ✅ | Multi-tenancy, Account model, Migrations |
| **Phase 2** | ✅ | 8 Authorization Policies, JWT Claims |
| **Phase 3** | ✅ | PatientConsent, MedicalRecordAccessLog, ConsentService |

### Desarrollo

- ✅ Base de datos: SQL Server con 58+ migraciones
- ✅ Autenticación: JWT con claims (role, account_id, clinic_id, user_id)
- ✅ Autorización: 8 políticas, 28+ permisos, 5 roles
- ✅ Multi-Tenancy: Account model con query filters
- ✅ Auditoría: Logs inmutables para acceso a registros médicos (NOM-004)
- ✅ Consentimiento: PatientConsent con soft delete

### Próximos Pasos

1. **Frontend Angular** (30-40 horas)
   - Phase 1: Modelos y Store
   - Phase 2: Guards y Servicios
   - Phase 3a: Audit Log UI
   - Phase 3b: Consent UI (MOBILE APP)

2. **Testing**
   - Unit tests para servicios
   - Integration tests para endpoints
   - E2E tests para flujos completos

3. **Deployment**
   - Azure App Service
   - SQL Server en la nube
   - SSL/HTTPS en producción

---

## 📚 Recursos Útiles

- **Swagger**: https://localhost:5126/swagger
- **JWT Decoder**: https://jwt.io
- **API Documentation**: Ver [ANGULAR_PROJECT_CONTEXT.md](ANGULAR_PROJECT_CONTEXT.md)
- **Code Examples**: Ver [ANGULAR_CODE_PATTERNS.md](ANGULAR_CODE_PATTERNS.md)

---

**¿Preguntas?** Consulta el documento correspondiente o revisa el código fuente.
