# Cambios de Implementación: Registro Multitenancy (Hospital/Clínica Auto-Registro)

**Fecha:** 2025  
**Versión:** 1.0  
**Estado:** ✅ Completado y compilado exitosamente  

---

## 📋 Resumen Ejecutivo

Se implementó el flujo de auto-registro para que hospitales y clínicas se registren automáticamente en el sistema, creando su propia **Account** (cuenta) y asignando al usuario registrador como **AccountAdmin** de esa Account.

### Cambio de Paradigma
- **Antes:** El endpoint `/api/user/register` creaba un usuario con rol "Admin" (global)
- **Ahora:** El endpoint crea una **Account nueva** + **User** con rol **AccountAdmin** (scoped a la Account)

---

## 🔧 Cambios Implementados

### 1. **UserController.cs** - Método `Register()`

**Archivo:** [Controllers/UserController.cs](Controllers/UserController.cs)

#### Cambios de Inyección de Dependencias
```csharp
// ANTES
public UserController(
    IUserRepository userRepository,
    IClinicRepository clinicRepository,
    IRoleRepository roleRepository,
    IMapper mapper,
    ITokenService tokenService,
    IUserService userService)

// AHORA
public UserController(
    IUserRepository userRepository,
    IClinicRepository clinicRepository,
    IRoleRepository roleRepository,
    AppDbContext context,        // ← NUEVO: Acceso directo al contexto para crear Account
    IMapper mapper,
    ITokenService tokenService,
    IUserService userService)
```

#### Nuevo Usings
```csharp
using MedPal.API.Data;  // ← Agregado para AppDbContext
```

#### Lógica del Método Register()
```csharp
[AllowAnonymous]
[HttpPost("register")]
public async Task<ActionResult<UserReadDTO>> Register([FromBody] UserRegisterDTO registerDto)
{
    // 1. Validar que el email no exista
    var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.Email);
    if (existingUser != null)
        return BadRequest(new { message = "El email ya está registrado en el sistema" });

    // 2. CREAR ACCOUNT AUTOMÁTICAMENTE
    var newAccount = new Account
    {
        Name = registerDto.Name,
        Description = $"Cuenta de {registerDto.Name} - Creada al registrarse",
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    await _context.Accounts.AddAsync(newAccount);
    await _context.SaveChangesAsync();
    
    if (newAccount.Id == 0)
        return BadRequest(new { message = "No se pudo crear la Account" });

    // 3. CREAR USUARIO CON ACCOUNTID ASIGNADO
    var newUser = new User
    {
        Name = registerDto.Name,
        Email = registerDto.Email,
        PasswordHash = registerDto.Password,
        Specialty = registerDto.Specialty,
        ProfessionalLicenseNumber = registerDto.ProfessionalLicenseNumber,
        IsActive = true,
        IsDeleted = false,
        HasAcceptedPrivacyTerms = registerDto.AcceptPrivacyTerms,
        AccountId = newAccount.Id,  // ← IMPORTANTE: Vincular usuario a la Account
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    var createdUser = await _userRepository.AddUserAsync(newUser);

    // 4. ASIGNAR ROLE ACCOUNTADMIN (no Admin)
    var accountAdminRole = await _roleRepository.GetRoleByNameAsync("AccountAdmin");
    if (accountAdminRole == null)
        return BadRequest(new { message = "El rol AccountAdmin no está configurado en el sistema" });
    
    await _roleRepository.AssignRoleToUserAsync(
        createdUser.Id, 
        accountAdminRole.Id, 
        clinicId: null,  // Global para la Account
        expiresAt: null, 
        assignedByUserId: null
    );

    // 5. GENERAR JWT (incluye AccountId automáticamente)
    var token = _tokenService.GenerateToken(createdUser);
    var userReadDTO = _mapper.Map<UserReadDTO>(createdUser);
    userReadDTO.Token = token;
    
    return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, userReadDTO);
}
```

---

### 2. **RoleController.cs** - Reemplazar "Admin" con "SuperAdmin"

**Archivo:** [Controllers/RoleController.cs](Controllers/RoleController.cs)

Reemplazadas **4 instancias** donde se valida que el usuario tenga rol "Admin":

| Línea | Ubicación | Cambio |
|-------|-----------|--------|
| 114 | AssignRole (clinic validation) | `"Admin"` → `"SuperAdmin"` |
| 158 | AssignRole (system role check) | `"Admin"` → `"SuperAdmin"` |
| 244 | RemoveRole (clinic validation) | `"Admin"` → `"SuperAdmin"` |
| 291 | RemoveRole (system role check) | `"Admin"` → `"SuperAdmin"` |

**Impacto:** Solo SuperAdmin puede validar acciones globales en la gestión de roles. Los AccountAdmin siguen siendo válidos para sus Accounts específicas.

---

### 3. **AuthorizationSeeder.cs** - Eliminar Rol "Admin" Deprecado

**Archivo:** [Data/Seeders/AuthorizationSeeder.cs](Data/Seeders/AuthorizationSeeder.cs)

#### Removido
1. **Definición del rol** (líneas 61-70):
   ```csharp
   // ANTES
   new Role
   {
       Name = "Admin",
       Description = "System administrator with full access to all features",
       IsSystemRole = true,
       IsActive = true,
       CreatedAt = DateTime.UtcNow,
       UpdatedAt = DateTime.UtcNow
   },
   
   // AHORA
   // (removido completamente)
   ```

2. **Variable local** en SeedPermissionsAsync:
   ```csharp
   // ANTES
   var admin = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
   
   // AHORA
   // (removido)
   ```

3. **Null check**:
   ```csharp
   // ANTES
   if (superAdmin == null || accountAdmin == null || clinicAdmin == null || admin == null || ...)
   
   // AHORA
   if (superAdmin == null || accountAdmin == null || clinicAdmin == null || ...)
   ```

4. **Asignación de permisos** para Admin:
   ```csharp
   // ANTES
   foreach (var permission in allPermissions)
   {
       if (!await context.RolePermissions.AnyAsync(rp => rp.RoleId == admin.Id && rp.PermissionId == permission.Id))
       {
           // Asignar todo
       }
   }
   
   // AHORA
   // (removido)
   ```

#### Roles Remanentes (8 en lugar de 9)
```
✅ SuperAdmin        - Global, todas las permissions
✅ AccountAdmin      - Por Account, todas las Account permissions
✅ ClinicAdmin       - Por Clinic, todas las Clinic permissions
✅ HealthProfessional - Clinical role con 11 permissions
✅ Doctor           - Clinical role con 14 permissions
✅ Nurse            - Clinical role con 7 permissions
✅ Receptionist     - Clinical role con 9 permissions
✅ Patient          - Patient role con 5 permissions
```

---

## 🏗️ Arquitectura Resultante

### Flujo de Auto-Registro

```
Hospital/Clínica registra
    ↓
POST /api/user/register
    ↓
✓ Crea Account nueva (nombre del hospital)
    ↓
✓ Crea User con AccountId = newAccount.Id
    ↓
✓ Asigna rol AccountAdmin al User (global para la Account)
    ↓
✓ Genera JWT con claims incluyendo account_id
    ↓
✓ Retorna UserReadDTO + Token
```

### Modelo de Roles Post-Implementación

```
SuperAdmin (Global)
├─ Acceso a TODOS los Accounts/Clinics/Users
├─ Puede asignar/remover roles globales
└─ Usado solo por administración del sistema

    ↓

AccountAdmin (Account-scoped)
├─ Acceso a su Account completo
├─ Puede gestionar Clinics dentro de su Account
├─ Puede gestionar Users dentro de su Account
├─ NUEVO: Asignado automáticamente en /register
└─ Puede asignar roles ClinicAdmin/Doctor/etc

    ↓

ClinicAdmin (Clinic-scoped)
├─ Acceso a su Clinic completo
├─ Puede gestionar Users/Pacientes de su Clinic
└─ Supervisa la clínica

    ↓

Clinical Roles (Specific permissions)
├─ Doctor: 14 permissions
├─ HealthProfessional: 11 permissions
├─ Nurse: 7 permissions
├─ Receptionist: 9 permissions
└─ Patient: 5 permissions
```

---

## 📊 Impacto en Endpoints

### `/api/user/register` (Ahora Multitenancy)
```
Request:
POST /api/user/register
{
    "name": "Hospital San José",
    "email": "admin@hospitalsj.com",
    "password": "Secure123!",
    "confirmPassword": "Secure123!",
    "specialty": "Hospital Management",
    "professionalLicenseNumber": "HSJ-2025-001",
    "acceptPrivacyTerms": true
}

Response (201 Created):
{
    "id": 1,
    "name": "Hospital San José",
    "email": "admin@hospitalsj.com",
    "accountId": 1,  // ← NUEVO: Asignado automáticamente
    "specialty": "Hospital Management",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "roles": ["AccountAdmin"]  // ← Rol asignado automáticamente
}
```

### JWT Claims (TokenService incluye automáticamente)
```json
{
    "sub": "1",
    "email": "admin@hospitalsj.com",
    "name": "Hospital San José",
    "role": "AccountAdmin",
    "account_id": "1",      // ← NUEVO en el JWT
    "clinic_id": null,
    "user_id": "1",
    "iat": 1704067200,
    "exp": 1704153600
}
```

---

## ✅ Verificación de Cambios

### Archivos Modificados
| Archivo | Cambios | Estado |
|---------|---------|--------|
| [UserController.cs](Controllers/UserController.cs) | Constructor + Register() | ✅ Compilado |
| [RoleController.cs](Controllers/RoleController.cs) | 4x "Admin" → "SuperAdmin" | ✅ Compilado |
| [AuthorizationSeeder.cs](Data/Seeders/AuthorizationSeeder.cs) | Rol Admin removido | ✅ Compilado |

### Compilación
```
Status: ✅ BUILD SUCCEEDED
Errors: 0
Warnings: ~40 (nullability warnings en DTOs, no afectan funcionamiento)
Output: bin/Debug/net8.0/MedPal.API.dll
```

---

## 🚀 Próximos Pasos

### 1. Aplicar Migración (si es necesaria)
```bash
cd MedPal.API
dotnet ef migrations add "RemoveAdminRole"
dotnet ef database update
```

**Nota:** No se requiere migración de base de datos porque:
- La tabla `Roles` ya tiene SuperAdmin
- El rol "Admin" será descartado pero no afecta usuarios existentes
- AccountId ya existe en tabla Users

### 2. Pruebas Recomendadas

#### Prueba 1: Auto-Registro de Hospital
```bash
curl -X POST http://localhost:5126/api/user/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Hospital Test",
    "email": "test@hospital.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "specialty": "General",
    "professionalLicenseNumber": "TEST-001",
    "acceptPrivacyTerms": true
  }'
```

**Verificar:**
- ✓ Account creada en BD con nombre "Hospital Test"
- ✓ User creada con AccountId = nuevo Account ID
- ✓ Role AccountAdmin asignado al user
- ✓ JWT contiene account_id en claims
- ✓ User puede acceder a Account management features

#### Prueba 2: RoleController con SuperAdmin
```bash
# Solo SuperAdmin debe poder asignar roles globales
curl -X POST http://localhost:5126/api/role/assign \
  -H "Authorization: Bearer {SUPERADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 2,
    "roleId": 3
  }'
```

**Verificar:**
- ✓ SuperAdmin puede asignar roles
- ✓ AccountAdmin NO puede asignar roles globales (solo en su Account)
- ✓ AccountAdmin CAN asignar roles dentro de su Account

### 3. Migración de Datos (si hay usuarios Admin existentes)
```sql
-- Si hay usuarios con rol Admin registrados, migrarlos a AccountAdmin
-- 1. Crear Account genérica si no existe
INSERT INTO Accounts (Name, IsActive, CreatedAt, UpdatedAt)
VALUES ('Legacy Admin Account', 1, GETUTCDATE(), GETUTCDATE())

-- 2. Asignar AccountId a users existentes sin Account
UPDATE Users 
SET AccountId = {NEW_ACCOUNT_ID} 
WHERE AccountId IS NULL

-- 3. Cambiar rol Admin a AccountAdmin
UPDATE UserRoles
SET RoleId = (SELECT Id FROM Roles WHERE Name = 'AccountAdmin')
WHERE RoleId = (SELECT Id FROM Roles WHERE Name = 'Admin')
```

---

## 📝 Changelog

### v1.0 (Actual)
- ✅ Implementado auto-registro de Account
- ✅ Usuario registrado como AccountAdmin
- ✅ Removido rol "Admin" deprecado
- ✅ Reemplazado "Admin" con "SuperAdmin" en RoleController
- ✅ Build compilado sin errores

---

## 🔗 Referencias Relacionadas

- [RESPUESTA_TUS_PREGUNTAS.md](RESPUESTA_TUS_PREGUNTAS.md) - Documentación inicial
- [RESUMEN_EJECUTIVO.md](RESUMEN_EJECUTIVO.md) - Resumen de arquitectura
- [ACTUALIZACION_SEEDER_ROLES.md](ACTUALIZACION_SEEDER_ROLES.md) - Actualización previa del seeder
- [TESTING_AND_VERIFICATION.md](TESTING_AND_VERIFICATION.md) - Guía de pruebas

---

**Fin del Documento**
