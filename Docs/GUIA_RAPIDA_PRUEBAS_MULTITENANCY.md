# Guía Rápida de Pruebas: Auto-Registro Multitenancy

**Versión:** 1.0  
**Estado:** Listo para probar  

---

## 🧪 Pruebas Rápidas

### 1️⃣ Verificar Build
```powershell
cd f:\PersonalProjects\SchedulingApp\Backend\Services\MedPalApi\MedPal.API
dotnet build
# Esperado: Build succeeded
```

### 2️⃣ Iniciar API
```powershell
dotnet run
# Esperado: App running at http://localhost:5126
```

### 3️⃣ Probar Auto-Registro (Hospital)
```bash
curl -X POST http://localhost:5126/api/user/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Hospital ABC",
    "email": "admin@hospitalabc.com",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "specialty": "Hospital Management",
    "professionalLicenseNumber": "HAB-2025-001",
    "acceptPrivacyTerms": true
  }'
```

**Respuesta Esperada:**
```json
{
    "id": 1,
    "name": "Hospital ABC",
    "email": "admin@hospitalabc.com",
    "accountId": 1,
    "specialty": "Hospital Management",
    "token": "eyJ...",
    "roles": ["AccountAdmin"]
}
```

### 4️⃣ Verificar en Base de Datos
```sql
-- Verificar Account creada
SELECT * FROM Accounts WHERE Name = 'Hospital ABC'
-- Esperado: 1 fila con IsActive = 1

-- Verificar User con AccountId
SELECT Id, Email, AccountId FROM Users WHERE Email = 'admin@hospitalabc.com'
-- Esperado: AccountId = 1 (ID del Account)

-- Verificar Role asignado
SELECT u.Email, r.Name 
FROM Users u
JOIN UserRoles ur ON u.Id = ur.UserId
JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@hospitalabc.com'
-- Esperado: Role Name = 'AccountAdmin'
```

### 5️⃣ Decodificar JWT (opcional)
Usa [jwt.io](https://jwt.io) o este comando:

```powershell
# Extraer payload del token (reemplaza TOKEN_AQUI)
$token = "eyJ..."
$parts = $token.Split('.')
[System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($parts[1] + '==')) | ConvertFrom-Json
```

**Verificar que contiene:**
```json
{
    "sub": "1",
    "email": "admin@hospitalabc.com",
    "name": "Hospital ABC",
    "role": "AccountAdmin",
    "account_id": "1",
    "clinic_id": null,
    "user_id": "1"
}
```

---

## ✅ Checklist de Verificación

| Paso | Verificación | ✓ |
|------|-------------|---|
| 1 | Build compila sin errores | |
| 2 | API inicia correctamente | |
| 3 | Endpoint `/register` accesible | |
| 4 | Account creada automáticamente | |
| 5 | User tiene AccountId asignado | |
| 6 | Role AccountAdmin asignado | |
| 7 | JWT incluye `account_id` | |
| 8 | TokenService genera correctamente | |
| 9 | RoleController usa SuperAdmin | |
| 10 | Seeder no incluye rol Admin | |

---

## 🔧 Troubleshooting

### "No se pudo crear la Account"
**Causa:** AppDbContext no registrado en DI  
**Solución:** Verificar `Program.cs` tiene `services.AddDbContext<AppDbContext>()`

### "El rol AccountAdmin no está configurado"
**Causa:** Seeder no ejecutado después de eliminar Admin  
**Solución:** 
```powershell
dotnet ef database drop
dotnet ef database update
# Recrea DB con nuevo seeder
```

### "The type or namespace name 'AppDbContext' could not be found"
**Causa:** Falta using `using MedPal.API.Data;`  
**Solución:** Verificar UserController.cs tiene el using agregado

### Token sin `account_id`
**Causa:** TokenService no incluye AccountId en claims  
**Solución:** Verificar `TokenService.GenerateToken()` construye claims correctamente

---

## 📊 Datos de Prueba Recomendados

```json
{
    "name": "Clínica San Pedro",
    "email": "admin@clinicasanpedro.com",
    "password": "ClínicaSP2025!",
    "confirmPassword": "ClínicaSP2025!",
    "specialty": "Medicina General",
    "professionalLicenseNumber": "CSP-2025-001",
    "acceptPrivacyTerms": true
}
```

---

## 🎯 Próximas Acciones Post-Prueba

1. ✅ Confirmar build exitoso
2. ✅ Probar auto-registro
3. ✅ Verificar Account + User + Role
4. ✅ Validar JWT con account_id
5. 📝 Ejecutar migrations (si es necesario)
6. 🚀 Desplegar cambios

---

**¡Listo para pruebas!** 🎉
