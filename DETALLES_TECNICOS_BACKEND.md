# 🔧 DETALLES TÉCNICOS: Implementación Backend

**Fecha:** 12 de Enero, 2026  
**Audiencia:** Desarrolladores Backend  
**Tipo:** Referencia técnica  

---

## 📋 Tabla de Contenidos

1. [Arquitectura de Autenticación](#arquitectura-de-autenticación)
2. [Sistema de Autorización](#sistema-de-autorización)
3. [Flujo de Claims en JWT](#flujo-de-claims-en-jwt)
4. [Políticas Implementadas](#políticas-implementadas)
5. [Query Filters Automáticos](#query-filters-automáticos)
6. [Errores Comunes](#errores-comunes)
7. [Testing Backend](#testing-backend)

---

## 🔐 Arquitectura de Autenticación

### 1. Componentes Principales

```
┌─────────────────────────────────────┐
│  Controllers                         │
│  [Authorize(Policy = "...")]         │
└────────────────┬────────────────────┘
                 ↓
┌─────────────────────────────────────┐
│  Authorization Handlers              │
│  - PermissionHandler                 │
│  - MedicalRecordAccessHandler        │
└────────────────┬────────────────────┘
                 ↓
┌─────────────────────────────────────┐
│  Permission Repository               │
│  UserHasPermissionAsync()            │
└────────────────┬────────────────────┘
                 ↓
┌─────────────────────────────────────┐
│  Database                            │
│  - Roles                             │
│  - Permissions                       │
│  - RolePermissions                   │
│  - UserRoles                         │
└─────────────────────────────────────┘
```

### 2. Flujo de Autenticación

```csharp
// 1. Usuario envía credenciales
POST /api/user/login
{
  "email": "doctor@clinic.com",
  "password": "securepass123"
}

// 2. Backend valida credenciales
UserRepository.ValidateUserAsync(email, password)
  → Busca usuario por email
  → Valida contraseña con BCrypt
  → Retorna User object

// 3. Backend obtiene rol
RoleRepository.GetUserRoleAsync(userId)
  → Busca en tabla UserRoles
  → Obtiene nombre del rol

// 4. Backend obtiene permisos
PermissionRepository.GetUserPermissionsAsync(userId)
  → Obtiene RolePermissions del rol
  → Retorna lista de Permission.Name

// 5. Backend genera JWT
JwtService.GenerateToken(user, role, permissions)
  → Agrega claims al token:
     - nameid: UserId
     - email: Email
     - role: RoleName
     - account_id: AccountId
     - clinic_id: ClinicId
     - permissions: [lista de permisos]
  → Retorna JWT firmado

// 6. Response al cliente
{
  "id": 1,
  "name": "Dr. Juan",
  "email": "doctor@clinic.com",
  "token": "eyJhbGc...",
  "role": "Doctor",
  "accountId": 5,
  "clinicId": 10,
  "permissions": ["Patients.ViewAll", "MedicalRecords.Create", ...]
}
```

---

## 🛡️ Sistema de Autorización

### 1. Política Basada en Permisos

```csharp
// En Program.cs
services.AddAuthorizationBuilder()
    .AddPolicy("Patients.ViewAll", policy => 
        policy.Requirements.Add(new PermissionRequirement("Patients.ViewAll")))
    .AddPolicy("Patients.Create", policy => 
        policy.Requirements.Add(new PermissionRequirement("Patients.Create")))
    // ... más políticas

// En endpoint
[HttpGet]
[Authorize(Policy = "Patients.ViewAll")]
public async Task<IActionResult> GetAllPatients()
{
    // Si usuario NO tiene permiso "Patients.ViewAll"
    // → Retorna 403 Forbidden (antes de entrar al método)
}
```

### 2. PermissionHandler

```csharp
// Authorization/PermissionHandler.cs
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // 1. Extraer userId del JWT
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Fail();
            return;
        }

        // 2. Extraer clinicId del request (si aplica)
        int? clinicId = GetClinicIdFromRequest();

        // 3. Verificar si usuario tiene permiso
        bool hasPermission = await _permissionRepository.UserHasPermissionAsync(
            userId,
            requirement.PermissionName,  // Ej: "Patients.ViewAll"
            clinicId
        );

        // 4. Permitir o denegar
        if (hasPermission)
        {
            context.Succeed(requirement);  // ✅ Permitir
        }
        else
        {
            context.Fail();  // ❌ Denegar (403)
        }
    }
}
```

### 3. Flujo Completo de una Solicitud

```
REQUEST: GET /api/patients
Headers: Authorization: Bearer eyJhbGc...

        ↓

┌──────────────────────────────────┐
│ 1. MIDDLEWARE: Autenticación      │
│ - Valida JWT                      │
│ - Extrae claims                   │
│ - Crea ClaimsPrincipal            │
└──────────────────────────────────┘

        ↓

┌──────────────────────────────────┐
│ 2. MIDDLEWARE: Autorización       │
│ - Lee [Authorize(Policy = "...")]│
│ - Ejecuta PermissionHandler       │
│ - Valida permisos en BD           │
└──────────────────────────────────┘
     ✓ PASA          ✗ FALLA
      ↓              ↓
   Continúa      403 Forbidden

        ↓

┌──────────────────────────────────┐
│ 3. CONTROLLER: Entra al método    │
│ PatientController.GetAllPatients()│
└──────────────────────────────────┘

        ↓

┌──────────────────────────────────┐
│ 4. QUERY FILTER: Filtra datos     │
│ - Obtiene userId del JWT          │
│ - Obtiene accountId del JWT       │
│ - Obtiene clinicId del JWT        │
│ - Aplica WHERE automático:        │
│   if (IsSuperAdmin)               │
│     return ALL                    │
│   else if (IsAccountAdmin)        │
│     return WHERE AccountId = X    │
│   else if (IsClinicAdmin)         │
│     return WHERE ClinicId = Y     │
└──────────────────────────────────┘

        ↓

┌──────────────────────────────────┐
│ 5. RESPUESTA: Datos seguros       │
│ {                                 │
│   "data": [filtrado],             │
│   "total": N,                     │
│   "status": "success"             │
│ }                                 │
└──────────────────────────────────┘
```

---

## 🎟️ Flujo de Claims en JWT

### Estructura del Token

```javascript
// Decodificado en jwt.io

HEADER:
{
  "alg": "HS256",
  "typ": "JWT"
}

PAYLOAD:
{
  "nameid": "1",                    // ← UserId (para identificar)
  "email": "doctor@clinic.com",
  "role": "Doctor",                 // ← Rol (para políticas simples)
  "account_id": "5",                // ← AccountId (para tenancy)
  "clinic_id": "10",                // ← ClinicId (para tenancy)
  "iss": "MedPalAPI",
  "aud": "MedPalApp",
  "exp": 1234567890,                // ← Expiración
  "iat": 1234567800
}

SIGNATURE:
HMACSHA256(base64UrlEncode(header) + "." + base64UrlEncode(payload), SECRET_KEY)
```

### Cómo se Extraen los Claims

```csharp
// En PermissionHandler
var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
var emailClaim = context.User.FindFirst(ClaimTypes.Email);
var roleClaim = context.User.FindFirst("role");

// En ITenantContextService
var accountIdClaim = context.User.FindFirst("account_id");
var clinicIdClaim = context.User.FindFirst("clinic_id");

// Resultado
int userId = int.Parse(userIdClaim.Value);           // 1
int accountId = int.Parse(accountIdClaim.Value);     // 5
int clinicId = int.Parse(clinicIdClaim?.Value ?? "0"); // 10 o 0
```

---

## 📋 Políticas Implementadas

### 1. Políticas de Rol Simple

```csharp
// ViewUsersPolicy: Solo administradores pueden ver usuarios
.AddPolicy("ViewUsersPolicy", policy =>
{
    policy.RequireAssertion(context =>
    {
        var roleClaim = context.User.FindFirst("role");
        return roleClaim?.Value switch
        {
            "SuperAdmin" => true,
            "AccountAdmin" => true,
            "ClinicAdmin" => true,
            _ => false
        };
    });
})

// ViewPatientsPolicy: Más roles pueden ver pacientes
.AddPolicy("ViewPatientsPolicy", policy =>
{
    policy.RequireAssertion(context =>
    {
        var roleClaim = context.User.FindFirst("role");
        return roleClaim?.Value switch
        {
            "SuperAdmin" => true,
            "AccountAdmin" => true,
            "ClinicAdmin" => true,
            "Doctor" => true,
            "HealthProfessional" => true,
            _ => false
        };
    });
})

// ViewAuditLogPolicy: Solo SuperAdmin y AccountAdmin
.AddPolicy("ViewAuditLogPolicy", policy =>
{
    policy.RequireAssertion(context =>
    {
        var roleClaim = context.User.FindFirst("role");
        return roleClaim?.Value switch
        {
            "SuperAdmin" => true,
            "AccountAdmin" => true,
            _ => false
        };
    });
})
```

### 2. Políticas de Permiso Granular

```csharp
// En Program.cs (auto-generadas)
.AddPolicy("Patients.ViewAll", policy =>
    policy.Requirements.Add(new PermissionRequirement("Patients.ViewAll")))
.AddPolicy("Patients.Create", policy =>
    policy.Requirements.Add(new PermissionRequirement("Patients.Create")))
.AddPolicy("Patients.Update", policy =>
    policy.Requirements.Add(new PermissionRequirement("Patients.Update")))
.AddPolicy("Patients.Delete", policy =>
    policy.Requirements.Add(new PermissionRequirement("Patients.Delete")))

// ... y más para cada recurso/acción
```

### 3. Uso en Endpoints

```csharp
[HttpGet]
[Authorize(Policy = "ViewUsersPolicy")]  // ← Política simple
public async Task<IActionResult> GetAllUsers()
{
    // Solo SuperAdmin, AccountAdmin, ClinicAdmin pueden llegar aquí
}

[HttpPost]
[Authorize(Policy = "Patients.Create")]  // ← Política de permiso
public async Task<IActionResult> CreatePatient([FromBody] PatientWriteDTO dto)
{
    // Solo usuarios con permiso "Patients.Create" pueden llegar aquí
    // El backend verificará en BD que tienen este permiso
}

[HttpDelete("{id}")]
[Authorize(Policy = "Patients.Delete")]
public async Task<IActionResult> DeletePatient(int id)
{
    // Requiere permiso "Patients.Delete"
}
```

---

## 🔍 Query Filters Automáticos

### Cómo Funciona

```csharp
// En AppDbContext.OnModelCreating()
modelBuilder.Entity<Patient>()
    .HasQueryFilter(p =>
        _tenantContext.IsSuperAdmin ||
        p.Clinic.Account.Id == _tenantContext.CurrentAccountId ||
        p.Clinic.Id == _tenantContext.CurrentClinicId
    );

// Traducción: Si el usuario es:
// - SuperAdmin: ve TODOS los pacientes
// - AccountAdmin: ve pacientes de su cuenta
// - ClinicAdmin/Doctor: ve pacientes de su clínica
```

### Ejemplo Práctico

```csharp
// Cuando llamas:
var patients = await _context.Patients.ToListAsync();

// El framework automáticamente añade:
// Si usuario es SuperAdmin:
SELECT * FROM Patients;

// Si usuario es AccountAdmin con AccountId=5:
SELECT * FROM Patients p
WHERE p.Clinic.Account.Id = 5;

// Si usuario es ClinicAdmin con ClinicId=10:
SELECT * FROM Patients p
WHERE p.Clinic.Id = 10;

// ¡Sin que escribas el WHERE en tu código!
```

### Saltarse el Filter (si es necesario)

```csharp
// Para obtener TODOS los pacientes (ignorar filter)
var allPatients = await _context.Patients
    .IgnoreQueryFilters()  // ← Ignorar filtros automáticos
    .ToListAsync();

// NOTA: Esto solo lo puede hacer SuperAdmin (verificado en la política)
```

---

## 📊 Matriz de Permisos por Rol

### SuperAdmin
```csharp
var superAdminPermissions = context.Permissions
    .ToList();  // TODOS los permisos
```

### AccountAdmin
```csharp
foreach (var permission in allPermissions)
{
    // Recibe TODOS los permisos excepto aquellos
    // que están explícitamente restringidos
    // (Como MedicalRecords si se requiere)
}
```

### ClinicAdmin
```csharp
var clinicAdminPermissions = new[]
{
    "Users.ViewAll", "Users.Create", "Users.Update",
    "Patients.ViewAll", "Patients.Create", "Patients.Update",
    "Appointments.ViewAll", "Appointments.Create", "Appointments.Update",
    "Clinics.View", "Clinics.Manage",
    "Roles.View", "Roles.Assign", "Roles.Revoke"
};
```

### Doctor
```csharp
var doctorPermissions = new[]
{
    "Patients.ViewAll", "Patients.Update",
    "Appointments.ViewAll", "Appointments.Create", "Appointments.Update",
    "MedicalRecords.ViewAssigned", "MedicalRecords.Create", "MedicalRecords.Update",
    "Prescriptions.Create",
    "Billing.View",
    "Clinics.View",
    "Roles.View"
};
```

---

## 🔗 Flujo Completo: Crear Paciente

```
┌─────────────────────────────────────┐
│ 1. REQUEST                          │
│ POST /api/patients                  │
│ Authorization: Bearer <token>       │
│ {                                   │
│   "name": "Juan Pérez",             │
│   "email": "juan@example.com",      │
│   "phone": "555-1234"               │
│ }                                   │
└─────────────────────────────────────┘
        ↓
┌─────────────────────────────────────┐
│ 2. JWT VALIDATION                   │
│ - Verifica firma del token          │
│ - Extrae userId=5                   │
│ - Extrae role="Doctor"              │
│ - Extrae clinicId=10                │
│ - Extrae accountId=2                │
└─────────────────────────────────────┘
        ↓
┌─────────────────────────────────────┐
│ 3. POLICY CHECK                     │
│ Endpoint: [Authorize(Policy = ...)] │
│ Policy = "Patients.Create"          │
│                                     │
│ PermissionHandler verifica:         │
│ ¿Usuario 5 tiene permiso            │
│  "Patients.Create"?                 │
│                                     │
│ Busca en BD:                        │
│ SELECT rp.Permission                │
│ FROM RolePermissions rp             │
│ JOIN UserRoles ur ON ...            │
│ WHERE ur.UserId = 5                 │
│ AND rp.Permission.Name = "Patients. │
│                        Create"       │
│                                     │
│ Resultado: SÍ tiene ✅              │
└─────────────────────────────────────┘
        ↓
┌─────────────────────────────────────┐
│ 4. CONTROLLER METHOD                │
│ PatientsController.CreatePatient()  │
│                                     │
│ Obtiene userId del claim:           │
│ var userId = User.FindFirst(        │
│   ClaimTypes.NameIdentifier)        │
│ → userId = 5                        │
│                                     │
│ Obtiene clinicId del JWT:           │
│ var clinicId = User.FindFirst(      │
│   "clinic_id")                      │
│ → clinicId = 10                     │
└─────────────────────────────────────┘
        ↓
┌─────────────────────────────────────┐
│ 5. VALIDACIÓN DE CONTEXTO           │
│ if (clinicId != requestClinicId)    │
│ {                                   │
│   return 403 Forbidden;             │
│ }                                   │
│                                     │
│ // Validar que el usuario puede     │
│ // crear pacientes en su clínica    │
│ (Y no en otra clínica)              │
└─────────────────────────────────────┘
        ↓
┌─────────────────────────────────────┐
│ 6. CREAR EN BD                      │
│ var patient = new Patient           │
│ {                                   │
│   Name = "Juan Pérez",              │
│   Email = "juan@example.com",       │
│   Phone = "555-1234",               │
│   ClinicId = 10,  // ← Auto        │
│   AccountId = 2,  // ← Auto        │
│   CreatedBy = 5   // ← Auto        │
│ };                                  │
│                                     │
│ await _context.Patients.Add...      │
└─────────────────────────────────────┘
        ↓
┌─────────────────────────────────────┐
│ 7. RESPONSE (201 Created)           │
│ {                                   │
│   "id": 42,                         │
│   "name": "Juan Pérez",             │
│   "email": "juan@example.com",      │
│   "clinicId": 10,                   │
│   "accountId": 2,                   │
│   "createdAt": "2026-01-12T..."     │
│ }                                   │
└─────────────────────────────────────┘
```

---

## 🚨 Errores Comunes

### Error 1: 401 Unauthorized
```
Causa: Token inválido o expirado
Síntomas: Cualquier solicitud retorna 401
Solución:
- Verificar que el token no expiró
- Verificar que el secret key es igual en frontend y backend
- Hacer login nuevamente
```

### Error 2: 403 Forbidden (Permission Denied)
```
Causa: Usuario no tiene el permiso necesario
Síntomas: Solicitud autorizada retorna 403
Solución:
- Verificar que el usuario tiene el rol correcto
- Verificar que el rol tiene el permiso asignado en BD
- Verificar que RolePermissions existe en BD
- Ejecutar AuthorizationSeeder para popular permisos
```

### Error 3: NullReferenceException en ITenantContextService
```
Causa: CurrentUserId es null
Síntomas: Excepción en LoggingBehavior o QueryFilters
Solución:
- Verificar que el usuario está autenticado (tiene token)
- Verificar que el token tiene el claim "nameid"
- Verificar que el usuario existe en la BD
```

### Error 4: Query Filter Muy Restrictivo
```
Causa: Usuario no ve datos que debería ver
Síntomas: Tablas vacías cuando no deberían
Solución:
- Verificar que CurrentAccountId está en el JWT
- Verificar que CurrentClinicId está en el JWT
- Verificar que IsSuperAdmin está bien calculado
- Usar .IgnoreQueryFilters() para debug
```

---

## 🧪 Testing Backend

### Test 1: Autenticación

```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var userService = new UserService(_userRepository);
    
    // Act
    var result = await userService.LoginAsync("doctor@clinic.com", "password");
    
    // Assert
    Assert.NotNull(result.Token);
    Assert.Equal("Doctor", result.Role);
    Assert.NotEmpty(result.Permissions);
}

[Fact]
public async Task Login_WithInvalidPassword_ReturnsForbidden()
{
    // Arrange
    var userService = new UserService(_userRepository);
    
    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedAccessException>(
        () => userService.LoginAsync("doctor@clinic.com", "wrongpassword")
    );
}
```

### Test 2: Autorización

```csharp
[Fact]
public async Task GetAllPatients_SuperAdmin_ReturnsAll()
{
    // Arrange
    var handler = new PermissionHandler(_permissionRepository, _httpContextAccessor);
    var context = new AuthorizationHandlerContext(
        new[] { new PermissionRequirement("Patients.ViewAll") },
        ClaimsPrincipal_SuperAdmin()
    );
    
    // Act
    await handler.HandleAsync(context);
    
    // Assert
    Assert.True(context.HasSucceeded);
}

[Fact]
public async Task GetAllPatients_Patient_ReturnsForbidden()
{
    // Arrange
    var handler = new PermissionHandler(_permissionRepository, _httpContextAccessor);
    var context = new AuthorizationHandlerContext(
        new[] { new PermissionRequirement("Patients.ViewAll") },
        ClaimsPrincipal_Patient()
    );
    
    // Act
    await handler.HandleAsync(context);
    
    // Assert
    Assert.True(context.HasFailed);
}
```

### Test 3: Query Filters

```csharp
[Fact]
public async Task GetPatients_ClinicAdmin_OnlyReturnsOwnClinic()
{
    // Arrange
    var tenantContext = new TenantContextService(...);
    tenantContext.CurrentClinicId = 10;  // Su clínica
    
    var dbContext = new AppDbContext(options, tenantContext);
    
    // Act
    var patients = await dbContext.Patients.ToListAsync();
    
    // Assert
    Assert.All(patients, p => Assert.Equal(10, p.ClinicId));
}

[Fact]
public async Task GetPatients_SuperAdmin_ReturnsAll()
{
    // Arrange
    var tenantContext = new TenantContextService(...);
    tenantContext.IsSuperAdmin = true;
    
    var dbContext = new AppDbContext(options, tenantContext);
    
    // Act
    var patients = await dbContext.Patients.ToListAsync();
    
    // Assert
    Assert.NotEmpty(patients);
    // Incluye pacientes de múltiples clínicas
}
```

---

## 📚 Referencias Rápidas

### Ubicación de Archivos Clave

```
Authorization/
├── PermissionHandler.cs          ← Verifica permisos
├── PermissionRequirement.cs      ← Define requerimiento
├── MedicalRecordAccessHandler.cs ← Acceso a medical records
└── Policies/
    └── AuthorizationPoliciesExtension.cs  ← Define políticas

Services/
├── ITokenService.cs              ← Genera JWT
├── ITenantContextService.cs      ← Contexto del usuario
└── UserService.cs                ← Lógica de autenticación

Repositories/
├── IPermissionRepository.cs       ← Consulta permisos
├── IUserRepository.cs             ← Obtiene usuarios
└── IRoleRepository.cs             ← Maneja roles

Models/
├── Authorization/
│   ├── Role.cs                    ← Modelo de rol
│   ├── Permission.cs              ← Modelo de permiso
│   ├── RolePermission.cs           ← Mapeo rol-permiso
│   └── UserRole.cs                ← Mapeo usuario-rol
└── User.cs, Account.cs, Clinic.cs
```

### Métodos Importantes

```csharp
// ITokenService
string GenerateToken(User user, string role, List<Permission> permissions)

// IPermissionRepository
Task<bool> UserHasPermissionAsync(int userId, string permissionName, int? clinicId)
Task<List<Permission>> GetUserPermissionsAsync(int userId)

// ITenantContextService
int? CurrentUserId { get; }
int? CurrentAccountId { get; }
int? CurrentClinicId { get; }
bool IsSuperAdmin { get; }
bool IsAccountAdmin { get; }
bool IsClinicAdmin { get; }

// IUserRepository
Task<User> ValidateUserAsync(string email, string password)
Task<User> GetUserByIdAsync(int id)
Task<User> CreateUserAsync(User user)
```

---

**Documento generado:** 12/01/2026  
**Versión:** 1.0 - Completo  
**Estado:** ✅ Listo para referencia
