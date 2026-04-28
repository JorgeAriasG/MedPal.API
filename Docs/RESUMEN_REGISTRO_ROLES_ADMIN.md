# 📋 Resumen Rápido: Registro, Roles y Super Admin

## 🎯 Pregunta Principal

**Con toda la implementación lista:**
1. ¿De qué manera se puede verificar la funcionalidad?
2. ¿Cómo se crean un super admin?
3. ¿Al registrar un nuevo usuario, qué rol quedará?

---

## 📌 Respuesta Corta

| Pregunta | Respuesta |
|----------|-----------|
| **¿Cómo verificar?** | Ejecutar `dotnet run` → Entrar a Swagger → Registrar usuario → Ver JWT |
| **¿Crear super admin?** | **Opción 1**: Registrar primer usuario (automático Admin) 👑<br>**Opción 2**: Cambiar rol con `PUT /api/role/assign` |
| **¿Rol al registrar?** | **Admin** automáticamente en el endpoint `/api/user/register` |

---

## 🔄 Flujo de Registro (Diagrama)

```
┌─────────────────────────────────────────────┐
│  POST /api/user/register (Sin token)        │
│                                              │
│  {                                           │
│    "name": "Juan Pérez",                    │
│    "email": "juan@example.com",             │
│    "password": "Password123!",              │
│    "acceptPrivacyTerms": true               │
│  }                                           │
└─────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────┐
│  1. ✅ Validar DTO                          │
│  2. ✅ Verificar email no existe            │
│  3. ✅ Crear User en BD                     │
│  4. ✅ Obtener rol "Admin"                  │
│  5. ✅ Asignar rol al usuario               │
│  6. ✅ Generar JWT con rol="Admin"          │
│  7. ✅ Retornar token                       │
└─────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────┐
│  201 Created                                 │
│                                              │
│  {                                           │
│    "id": 1,                                 │
│    "name": "Juan Pérez",                    │
│    "email": "juan@example.com",             │
│    "token": "eyJhbGciOiJIUzI1NiI...",      │
│    "isActive": true,                        │
│    "createdAt": "2026-01-12T12:00:00Z"      │
│  }                                           │
└─────────────────────────────────────────────┘
```

---

## 🔐 JWT Generado Contiene

```
HEADER:
{
  "alg": "HS256",
  "typ": "JWT"
}

PAYLOAD:
{
  "role": "Admin",           ← ROL ASIGNADO
  "user_id": 1,              ← ID DEL USUARIO
  "sub": "juan@example.com",
  "email": "juan@example.com",
  "iat": 1673528400,          ← Emitido en
  "exp": 1673531400           ← Expira en (30 min)
}

SIGNATURE:
{
  "verified": true
}
```

---

## 👑 Crear Super Admin - Opciones

### ✅ Opción 1: Primer Registro (RECOMENDADO)

```bash
# El PRIMER usuario que se registre será Admin automáticamente
POST /api/user/register
{
  "name": "Super Admin",
  "email": "admin@medpal.com",
  "password": "AdminPass123!",
  "acceptPrivacyTerms": true
}

# Respuesta: 201 Created + JWT con rol="Admin"
```

**Ventaja**: No requiere nada, es automático.

---

### ✅ Opción 2: Cambiar Rol de Usuario Existente

```bash
# Paso 1: Como Admin, obtener token
POST /api/user/login
{
  "email": "admin@medpal.com",
  "password": "AdminPass123!"
}
# Respuesta: token (Admin JWT)

# Paso 2: Asignar otro rol a usuario
PUT /api/role/assign
Headers: Authorization: Bearer {ADMIN_TOKEN}
{
  "userId": 2,
  "roleId": 1,      # 1=Admin, 2=Doctor, 3=Nurse, 4=Receptionist, 5=Patient
  "clinicId": null
}

# Respuesta: 200 OK + Audit log creado
```

**Ventaja**: Flexible, puedes cambiar roles cuando quieras.

---

### ✅ Opción 3: Crear Seeder Personalizado (DEVELOPMENT)

En `Data/Seeders/AuthorizationSeeder.cs`, agregar:

```csharp
private static async Task SeedSuperAdminAsync(AppDbContext context)
{
    // Crear cuenta
    var account = new Account { Name = "Super Admin Account", IsActive = true };
    await context.Accounts.AddAsync(account);
    await context.SaveChangesAsync();

    // Crear usuario
    var superAdmin = new User
    {
        Name = "System Super Admin",
        Email = "superadmin@medpal.local",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!"),
        IsActive = true,
        HasAcceptedPrivacyTerms = true,
        AccountId = account.Id,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    await context.Users.AddAsync(superAdmin);
    await context.SaveChangesAsync();

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
        await context.UserRoles.AddAsync(userRole);
        await context.SaveChangesAsync();
    }
}

// En SeedAsync(), agregar:
public static async Task SeedAsync(AppDbContext context)
{
    await SeedRolesAsync(context);
    await SeedPermissionsAsync(context);
    await SeedRolePermissionsAsync(context);
    await SeedSuperAdminAsync(context);  // ← AGREGAR ESTA LÍNEA
    await context.SaveChangesAsync();
}
```

**Ventaja**: Se crea automáticamente en cada `dotnet ef database update`

---

## 🧪 Verificación Completa (5 minutos)

### 1️⃣ Iniciar App

```bash
cd f:\PersonalProjects\SchedulingApp\Backend\Services\MedPalApi\MedPal.API
dotnet run
```

**✅ Esperado**: App corriendo en https://localhost:5126

---

### 2️⃣ Abrir Swagger

```
https://localhost:5126/swagger
```

**✅ Esperado**: Página Swagger con todos los endpoints

---

### 3️⃣ Registrar Admin

```
POST /api/user/register
{
  "name": "Admin Principal",
  "email": "admin@medpal.com",
  "password": "AdminPass123!",
  "acceptPrivacyTerms": true
}
```

**✅ Esperado**: 
```json
{
  "id": 1,
  "token": "eyJhbGciOiJIUzI1NiI...",
  "isActive": true
}
```

---

### 4️⃣ Decodificar JWT

1. Ve a https://jwt.io
2. Copia el `token` de la respuesta
3. Pégalo en el campo de JWT

**✅ Esperado**: En Payload ver `"role": "Admin"`

---

### 5️⃣ Crear Usuario (con Admin)

```
POST /api/user
Headers: Authorization: Bearer {TOKEN_DEL_ADMIN}
{
  "name": "Dr. Juan",
  "email": "juan@medpal.com",
  "password": "JuanPass123!",
  "specialty": "Cardiología"
}
```

**✅ Esperado**: 201 Created (Usuario ID=2)

---

### 6️⃣ Cambiar Rol a Doctor

```
PUT /api/role/assign
Headers: Authorization: Bearer {TOKEN_DEL_ADMIN}
{
  "userId": 2,
  "roleId": 2,    # 2 = Doctor
  "clinicId": null
}
```

**✅ Esperado**: 200 OK (Rol asignado)

---

### 7️⃣ Verificar en BD

```sql
-- SQL Server Management Studio
SELECT u.Id, u.Name, u.Email, r.Name AS Rol
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id;
```

**✅ Esperado**:
```
Id | Name            | Email              | Rol
1  | Admin Principal | admin@medpal.com   | Admin
2  | Dr. Juan        | juan@medpal.com    | Doctor
```

---

## 🎯 Estados Posibles

### Al Registrar Nuevo Usuario

| Escenario | AccountId | ClinicId | Rol | Resultado |
|-----------|-----------|----------|-----|-----------|
| **Primer usuario** | `null` | `null` | **Admin** | ✅ Acceso global |
| **Registro anónimo** | `null` | `null` | **Admin** | ✅ Acceso global |
| **Cambiar rol** | Igual | Igual | **Doctor** | ✅ Rol modificado |
| **Con clínica** | `1` | `5` | **Doctor** | ⚠️ Solo esa clínica |

---

## 📊 Tabla de Permisos por Rol

```
┌──────────────┬────────┬────────┬───────┬──────────────┬────────┐
│ Permiso      │ Admin  │ Doctor │ Nurse │ Receptionist │ Patient│
├──────────────┼────────┼────────┼───────┼──────────────┼────────┤
│ Users.ViewAll│   ✅   │   ❌   │  ❌   │      ❌      │   ❌   │
│ Users.Manage │   ✅   │   ❌   │  ❌   │      ❌      │   ❌   │
│ Roles.Assign │   ✅   │   ❌   │  ❌   │      ❌      │   ❌   │
│ Roles.View   │   ✅   │   ✅   │  ✅   │      ✅      │   ✅   │
│ Patients.*   │   ✅   │   ✅   │  ❌   │      ✅      │   ✅   │
│ MedicalRec.* │   ✅   │   ✅   │  ✅   │      ❌      │   ✅   │
│ Appts.*      │   ✅   │   ✅   │  ✅   │      ✅      │   ✅   │
│ Reports.*    │   ✅   │   ✅   │  ❌   │      ❌      │   ❌   │
└──────────────┴────────┴────────┴───────┴──────────────┴────────┘
```

---

## 🔑 Clave Importante: Account y Multi-Tenancy

### Sin Account (Primer Usuario Admin)

```json
{
  "id": 1,
  "name": "Admin",
  "email": "admin@medpal.com",
  "accountId": null,        ← SIN CUENTA
  "principalClinicId": null,
  "token": "..."
}

// JWT contiene:
{
  "role": "Admin",
  "user_id": 1,
  "account_id": null,       ← NO SE INCLUYE EN JWT
  "clinic_id": null
}
```

**Resultado**: Acceso GLOBAL (todas las clínicas, sin restricción de Account)

---

### Con Account (Usuario Normal)

```json
{
  "id": 2,
  "name": "Dr. Juan",
  "email": "juan@medpal.com",
  "accountId": 1,           ← ASIGNADO A CUENTA
  "principalClinicId": 5,
  "token": "..."
}

// JWT contiene:
{
  "role": "Doctor",
  "user_id": 2,
  "account_id": 1,          ← SE INCLUYE EN JWT
  "clinic_id": 5
}
```

**Resultado**: Acceso LIMITADO (solo a su Account y Clínica)

---

## 🚀 Próximas Acciones

### Backend
- ✅ Verificar que funciona (ver guía anterior)
- ✅ Crear Super Admin
- ✅ Crear usuarios con diferentes roles

### Frontend (Después)
- 📋 Leer [ANGULAR_PROJECT_CONTEXT.md](ANGULAR_PROJECT_CONTEXT.md)
- 📋 Implementar Phase 1 (Modelos y Store)
- 📋 Implementar Phase 2 (Guards y Servicios)
- 📋 Implementar Phase 3a (Audit Log UI)

---

## ❓ Preguntas Frecuentes

### P: ¿Qué rol tiene un usuario recién registrado?
**R**: Admin automáticamente, en el endpoint `/api/user/register`

### P: ¿Puedo cambiar el rol después?
**R**: Sí, con `PUT /api/role/assign` (requiere rol Admin)

### P: ¿Qué es AccountId?
**R**: Es para multi-tenancy. Admin no lo tiene (acceso global). Los demás lo tienen (acceso limitado a su Account)

### P: ¿El JWT expira?
**R**: Sí, en 30 minutos. Debes hacer login nuevamente.

### P: ¿Puedo crear múltiples super admins?
**R**: Sí, cualquiera que sea Admin puede asignar el rol Admin a otros usuarios.

### P: ¿Cómo verifico que los permisos funcionan?
**R**: Intenta acceder a un endpoint sin tener el permiso. Deberías recibir 403 Forbidden.

---

## 📚 Documentos Relacionados

1. **[TESTING_AND_VERIFICATION.md](TESTING_AND_VERIFICATION.md)** - Guía completa de testing
2. **[GUIA_PRACTICA_VERIFICACION.md](GUIA_PRACTICA_VERIFICACION.md)** - Pasos prácticos
3. **[ANGULAR_PROJECT_CONTEXT.md](ANGULAR_PROJECT_CONTEXT.md)** - Contexto del frontend
4. **[Phase1_MigrateExistingDataToAccount.sql](Scripts/Phase1_MigrateExistingDataToAccount.sql)** - Script SQL

---

**¿Listo para empezar?** 🚀 Ejecuta `dotnet run` y abre Swagger.
