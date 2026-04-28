# Guía Práctica: Verificación & Creación de Admin

**Objetivo**: Verificar que el backend funciona correctamente y crear un Super Admin  
**Tiempo**: 15-20 minutos  
**Prerequisitos**: 
- Visual Studio Code o Postman
- SQL Server corriendo
- Base de datos MedPal creada

---

## 🚀 Paso 1: Iniciar la Aplicación

### Opción A: Desde Terminal

```powershell
# Navegar a la carpeta del proyecto
cd "f:\PersonalProjects\SchedulingApp\Backend\Services\MedPalApi\MedPal.API"

# Restaurar dependencias
dotnet restore

# Ejecutar
dotnet run
```

### Opción B: Desde VS Code

1. Abre la carpeta del proyecto en VS Code
2. Terminal → New Terminal
3. Ejecuta `dotnet run`

**Esperado:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5126
      
Application started. Press Ctrl+C to stop.
```

---

## ✅ Paso 2: Verificar Acceso a Swagger

Abre en el navegador:
```
https://localhost:5126/swagger
```

Deberías ver la interfaz de Swagger con todos los endpoints disponibles.

---

## 📝 Paso 3: Crear Primer Usuario (Super Admin)

### Usando Swagger

1. **Ve a**: `POST /api/user/register`
2. **Click en "Try it out"**
3. **Pega este JSON**:

```json
{
  "name": "Admin Principal",
  "email": "admin@medpal.local",
  "password": "AdminPass123!",
  "specialty": "System Administrator",
  "professionalLicenseNumber": "ADMIN-001",
  "acceptPrivacyTerms": true
}
```

4. **Click en "Execute"**

### Esperado

```json
{
  "id": 1,
  "name": "Admin Principal",
  "email": "admin@medpal.local",
  "specialty": "System Administrator",
  "professionalLicenseNumber": "ADMIN-001",
  "isActive": true,
  "hasAcceptedPrivacyTerms": true,
  "createdAt": "2026-01-12T12:34:56.789Z",
  "updatedAt": "2026-01-12T12:34:56.789Z",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzd2IiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
  "message": "User registered successfully"
}
```

---

## 🔐 Paso 4: Decodificar el JWT

1. Ve a [jwt.io](https://jwt.io)
2. Copia el valor de `"token"` de la respuesta anterior
3. Pégalo en el campo de **"Encoded"**

### Esperado en Payload

```json
{
  "role": "Admin",
  "account_id": null,
  "clinic_id": null,
  "user_id": 1,
  "sub": "admin@medpal.local",
  "email": "admin@medpal.local",
  "iat": 1673528400,
  "exp": 1673531400
}
```

**Nota**: El rol es **"Admin"** automáticamente para el primer usuario.

---

## 🔑 Paso 5: Login y Obtener Nuevo Token

Si necesitas acceso después, usa el endpoint de login.

### Usando Swagger

1. **Ve a**: `POST /api/user/login`
2. **Click en "Try it out"**
3. **Pega este JSON**:

```json
{
  "email": "admin@medpal.local",
  "password": "AdminPass123!"
}
```

4. **Click en "Execute"**

### Esperado

```json
{
  "id": 1,
  "name": "Admin Principal",
  "email": "admin@medpal.local",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "isActive": true,
  "message": "Login successful"
}
```

---

## 👥 Paso 6: Crear Más Usuarios con Roles Diferentes

### Opción A: Registrar Otros Usuarios

```json
{
  "name": "Dr. Juan Pérez",
  "email": "juan.perez@medpal.local",
  "password": "JuanPass123!",
  "specialty": "Cardiología",
  "professionalLicenseNumber": "LIC-12345",
  "acceptPrivacyTerms": true
}
```

**Nota**: Estos usuarios tendrán rol `Admin` también (por defecto en el `Register` endpoint).

### Opción B: Crear Usuario y Asignar Rol Diferente

#### Paso 1: Como Admin, crea el usuario con `POST /api/user`

**Primero**, necesitas tu token de Admin. Click en el botón 🔓 "Authorize" en Swagger e ingresa:

```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

(Usa el token que obtuviste del login/register)

#### Paso 2: Ve a `POST /api/user` y crea usuario:

```json
{
  "name": "Dra. María García",
  "email": "maria.garcia@medpal.local",
  "password": "MariaPass123!",
  "specialty": "Neurología",
  "professionalLicenseNumber": "LIC-67890"
}
```

**Respuesta**: Se crea con ID = 2 (o siguiente)

#### Paso 3: Asigna rol diferentes

Ve a `PUT /api/role/assign`:

```json
{
  "userId": 2,
  "roleId": 2,
  "clinicId": null,
  "expiresAt": null
}
```

**Donde:**
- `roleId: 1` = Admin
- `roleId: 2` = Doctor
- `roleId: 3` = Nurse
- `roleId: 4` = Receptionist
- `roleId: 5` = Patient

---

## 🎯 Paso 7: Verificar Roles y Permisos

### Listar Todos los Roles

`GET /api/role`

**Esperado:**

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
  },
  {
    "id": 3,
    "name": "Nurse",
    "description": "Nurse with limited access to patient information",
    "isSystemRole": true,
    "isActive": true
  },
  {
    "id": 4,
    "name": "Receptionist",
    "description": "Receptionist managing appointments",
    "isSystemRole": true,
    "isActive": true
  },
  {
    "id": 5,
    "name": "Patient",
    "description": "Patient with access to own medical records",
    "isSystemRole": true,
    "isActive": true
  }
]
```

### Ver Permisos de un Rol

`GET /api/role/{roleId}`

```
GET /api/role/1
```

**Esperado**: Detalles del rol Admin con lista de permisos

---

## 🔍 Paso 8: Verificar en SQL Server

Abre **SQL Server Management Studio** y ejecuta:

```sql
-- Ver usuarios creados
SELECT Id, Name, Email, IsActive, CreatedAt FROM Users;

-- Ver roles de usuarios
SELECT 
    u.Id, u.Name, u.Email,
    r.Name AS RoleName
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id;

-- Ver todos los roles
SELECT * FROM Roles;

-- Ver permiso de Admin
SELECT p.Name, p.Resource, p.Action
FROM RolePermissions rp
JOIN Permissions p ON rp.PermissionId = p.Id
WHERE rp.RoleId = 1
ORDER BY p.Name;
```

### Esperado

```
Id | Name                | Email                      | RoleName
1  | Admin Principal     | admin@medpal.local         | Admin
2  | Dra. María García   | maria.garcia@medpal.local  | Doctor
```

---

## 🧪 Paso 9: Probar Permisos

### Test 1: Admin puede ver todos los usuarios

**Endpoint**: `GET /api/user`  
**Token**: Admin JWT  
**Esperado**: 200 OK - Lista de usuarios

```json
[
  {
    "id": 1,
    "name": "Admin Principal",
    "email": "admin@medpal.local",
    "isActive": true
  },
  {
    "id": 2,
    "name": "Dra. María García",
    "email": "maria.garcia@medpal.local",
    "isActive": true
  }
]
```

### Test 2: Doctor puede ver pacientes

**Endpoint**: `GET /api/patients`  
**Token**: Doctor JWT  
**Esperado**: 200 OK - Lista de pacientes (si existen)

### Test 3: Sin token no se puede acceder

**Endpoint**: `GET /api/user`  
**Token**: (ninguno)  
**Esperado**: 401 Unauthorized

```json
{
  "message": "Unauthorized"
}
```

---

## 📊 Tabla de Verificación

Marca cada verificación a medida que completes:

### Configuración Inicial

- [ ] Aplicación ejecutándose en https://localhost:5126
- [ ] Swagger accesible en https://localhost:5126/swagger
- [ ] SQL Server conectando correctamente
- [ ] Base de datos MedPal existe

### Usuarios y Roles

- [ ] Primer usuario registrado como Admin
- [ ] Token JWT generado correctamente
- [ ] JWT contiene rol "Admin"
- [ ] Segundo usuario creado con rol Doctor
- [ ] Rol asignado correctamente en BD

### Permisos

- [ ] Admin puede ver todos los usuarios
- [ ] Doctor no puede crear usuarios (403)
- [ ] Sin token recibe 401
- [ ] Permisos verifican correctamente

### Base de Datos

- [ ] 5 roles existen en tabla Roles
- [ ] 28+ permisos en tabla Permissions
- [ ] RolePermissions están mapeados
- [ ] Usuarios creados en tabla Users

---

## 🆘 Problemas Comunes

### "Puede que 'AccountId' sea null"

**No es un problema**, es correcto. El primer usuario no tiene Account asignada (multi-tenancy).

### "El rol de administrador no está configurado"

```bash
# Reconstruir desde cero
dotnet ef database drop -f
dotnet ef database update
dotnet run
```

### "JWT token is not configured"

En `appsettings.Development.json`, verifica:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-here-min-32-chars",
    "Issuer": "MedPal",
    "Audience": "MedPalUsers"
  }
}
```

### "Connection string issue"

En `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MedPal;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

---

## ✨ Siguiente Paso

Una vez verificado que todo funciona:

1. **Leer**: [ANGULAR_PROJECT_CONTEXT.md](ANGULAR_PROJECT_CONTEXT.md)
2. **Implementar**: Frontend Angular (Phases 1-3a)
3. **Testing**: Integración con backend

---

**¿Listo?** El backend está completamente funcional. ¡A implementar el frontend! 🚀
