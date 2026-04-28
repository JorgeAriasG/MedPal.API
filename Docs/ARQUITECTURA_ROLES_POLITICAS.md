# 🚀 RESUMEN ARQUITECTURA: Roles, Políticas y Multi-Tenancy

**Fecha:** Enero 12, 2026  
**Versión:** 1.0  
**Estado:** ✅ Completado y funcional  

---

## 📋 Tabla de Contenidos

1. [Jerarquía de Roles](#jerarquía-de-roles)
2. [Estructura de Multi-Tenancy](#estructura-de-multi-tenancy)
3. [Sistema de Permisos](#sistema-de-permisos)
4. [Políticas de Autorización](#políticas-de-autorización)
5. [Flujo de Autenticación](#flujo-de-autenticación)
6. [Guía de Integración Frontend](#guía-de-integración-frontend)

---

## 🎯 Jerarquía de Roles

### 1. **SuperAdmin** (Sistema Completo)
```
├─ Acceso: TODOS los datos del sistema
├─ Alcance: Global (sin restricciones de Account/Clinic)
├─ Operaciones:
│  ├─ Gestión de Cuentas (Account)
│  ├─ Gestión de Clínicas
│  ├─ Gestión de todos los usuarios
│  ├─ Auditoría global del sistema
│  └─ Todas las operaciones administrativas
│
├─ ❌ RESTRICCIONES:
│  ├─ NO acceso a Medical Records (NOM-004 - Confidencialidad)
│  ├─ NO acceso a datos privados de pacientes
│  └─ NO puede crear roles médicos (Doctor, Nurse, etc.)
│
└─ Permisos: ✅ TODOS (100%)
```

**Caso de uso:** Administrador del sistema MedPal

---

### 2. **AccountAdmin** (Admin de Cuenta)
```
├─ Acceso: TODA su cuenta + todas sus clínicas
├─ Alcance: Restringido a su Account
├─ Operaciones:
│  ├─ Crear/Editar clínicas en su cuenta
│  ├─ Crear/Editar usuarios en su cuenta
│  ├─ Asignar roles (excepto SuperAdmin)
│  ├─ Ver auditoría de su cuenta
│  └─ Gestionar pacientes de su cuenta
│
├─ ❌ RESTRICCIONES:
│  ├─ NO acceso a Medical Records
│  ├─ NO ver cuentas ajenas
│  ├─ NO crear SuperAdmins
│  └─ Solo su Account (su AccountId)
│
└─ Permisos: ✅ TODOS menos MedicalRecords (95%)
```

**Caso de uso:** Director ejecutivo de una clínica o red de clínicas

---

### 3. **ClinicAdmin** (Admin de Clínica)
```
├─ Acceso: Solo su clínica
├─ Alcance: Restringido a su Clinic
├─ Operaciones:
│  ├─ Crear/Editar personal de su clínica
│  ├─ Gestionar pacientes de su clínica
│  ├─ Ver auditoría de su clínica
│  ├─ Asignar roles clínicos (Doctor, Nurse, etc.)
│  └─ Ver reportes de su clínica
│
├─ ❌ RESTRICCIONES:
│  ├─ NO acceso a Medical Records
│  ├─ NO ver otras clínicas
│  ├─ NO crear AccountAdmins
│  └─ Solo su Clinic (su ClinicId)
│
└─ Permisos: ✅ Todos menos MedicalRecords (95%)
```

**Caso de uso:** Director de una clínica específica

---

### 4. **Doctor** (Rol Clínico)
```
├─ Acceso: Pacientes y registros de su clínica
├─ Alcance: Restringido a su Clinic
├─ Operaciones:
│  ├─ Ver pacientes de su clínica
│  ├─ Crear/Actualizar Medical Records
│  ├─ Ver citas de su clínica
│  ├─ Crear/Actualizar prescripciones
│  └─ Ver auditoría limitada
│
├─ ❌ RESTRICCIONES:
│  ├─ NO gestionar usuarios
│  ├─ NO ver otras clínicas
│  ├─ Solo registros de SU clínica
│  └─ NO acceso a datos financieros
│
└─ Permisos: ✅ Clínicos y consulta (60%)
```

**Caso de uso:** Médico tratante

---

### 5. **HealthProfessional** (Profesional de Salud)
```
├─ Acceso: Similar a Doctor, más limitado
├─ Alcance: Restringido a su Clinic
├─ Operaciones:
│  ├─ Ver pacientes asignados
│  ├─ Crear notas de seguimiento
│  ├─ Ver citas de su clínica
│  └─ Consultar Medical Records asignados
│
├─ ❌ RESTRICCIONES:
│  ├─ NO crear Medical Records
│  ├─ NO ver otras clínicas
│  ├─ Solo registros asignados
│  └─ NO gestionar usuarios
│
└─ Permisos: ✅ Clínicos limitados (45%)
```

**Caso de uso:** Enfermera, Psicólogo, Fisioterapeuta

---

### 6. **Receptionist** (Recepcionista)
```
├─ Acceso: Datos de pacientes y citas
├─ Alcance: Restringido a su Clinic
├─ Operaciones:
│  ├─ Crear/Editar pacientes
│  ├─ Gestionar citas
│  ├─ Ver datos demográficos
│  └─ Consultar disponibilidad
│
├─ ❌ RESTRICCIONES:
│  ├─ NO acceso a Medical Records
│  ├─ NO ver otras clínicas
│  ├─ NO crear usuarios
│  └─ NO acceso a datos sensibles
│
└─ Permisos: ✅ Operacionales (30%)
```

**Caso de uso:** Personal administrativo de clínica

---

### 7. **Patient** (Paciente)
```
├─ Acceso: Sus propios registros solamente
├─ Alcance: Personal (Solo datos propios)
├─ Operaciones:
│  ├─ Ver su perfil
│  ├─ Ver sus citas
│  ├─ Ver sus Medical Records
│  ├─ Autorizar accesos
│  └─ Ver auditoría de sus datos
│
├─ ❌ RESTRICCIONES:
│  ├─ NO ver otros pacientes
│  ├─ NO ver datos de clínica
│  ├─ NO crear usuarios
│  └─ NO acceso a datos administrativos
│
└─ Permisos: ✅ Personales (20%)
```

**Caso de uso:** Usuario paciente

---

## 🏢 Estructura de Multi-Tenancy

### Conceptos Clave

```
┌─────────────────────────────────────────────────────────┐
│              SISTEMA MEDPAL                             │
└─────────────────────────────────────────────────────────┘
        ↓
┌─────────────────────────────────────────────────────────┐
│    ACCOUNT (Cuenta/Organización)                        │
│  "Hospital XYZ Grupo"                                   │
│  ├─ ID: Identificador único                             │
│  ├─ Name: Nombre de la organización                     │
│  ├─ IsSuperAdmin: false (normal account)                │
│  └─ Users: Todos los usuarios de esta cuenta            │
└─────────────────────────────────────────────────────────┘
        ↓
   ┌────┴────┐
   ↓         ↓
┌──────────────┐  ┌──────────────┐
│  CLINIC      │  │  CLINIC      │
│  (Ubicación) │  │  (Ubicación) │
│  "Centro 1"  │  │  "Centro 2"  │
├──────────────┤  ├──────────────┤
│ Users: N     │  │ Users: M     │
│ Patients: K  │  │ Patients: L  │
└──────────────┘  └──────────────┘
   ↓         ↓       ↓         ↓
 Users    Patients Users    Patients
```

### Aislamiento de Datos

| Nivel | Descripción | Aislamiento | Ejemplo |
|-------|-------------|------------|---------|
| **SuperAdmin** | Sin restricciones | Ve TODO el sistema | User logs auditoría global |
| **AccountAdmin** | Por Account | Solo su Account | "Hospital XYZ" ve sus clínicas |
| **ClinicAdmin/Doctor** | Por Clinic | Solo su Clinic | "Centro 1" no ve "Centro 2" |
| **Patient** | Personal | Solo su data | Solo su perfil y registros |

---

## 🔐 Sistema de Permisos

### Formato de Permisos

Todos los permisos siguen el patrón: **`Resource.Action`**

```
Ejemplo: "Patients.ViewAll", "MedicalRecords.Create", "Users.Manage"
       └─────┬─────┘  └────┬────┘
         Recurso       Acción
```

### Permisos Disponibles

#### 👥 Usuarios (Users)
```javascript
{
  "Users.ViewAll": "Ver todos los usuarios",
  "Users.ViewOwn": "Ver solo el usuario actual",
  "Users.Create": "Crear nuevos usuarios",
  "Users.Update": "Editar usuarios",
  "Users.Delete": "Eliminar usuarios",
  "Users.Manage": "Gestión completa de usuarios"
}
```

#### 🏥 Pacientes (Patients)
```javascript
{
  "Patients.ViewAll": "Ver todos los pacientes",
  "Patients.ViewOwn": "Ver solo pacientes propios",
  "Patients.Create": "Crear nuevos pacientes",
  "Patients.Update": "Editar pacientes",
  "Patients.Delete": "Eliminar pacientes"
}
```

#### 📋 Citas (Appointments)
```javascript
{
  "Appointments.ViewAll": "Ver todas las citas",
  "Appointments.ViewOwn": "Ver solo citas propias",
  "Appointments.Create": "Crear citas",
  "Appointments.Update": "Editar citas",
  "Appointments.Cancel": "Cancelar citas"
}
```

#### 📄 Registros Médicos (MedicalRecords)
```javascript
{
  "MedicalRecords.ViewAll": "Ver todos (SuperAdmin)",
  "MedicalRecords.ViewOwn": "Ver propios (pacientes)",
  "MedicalRecords.ViewAssigned": "Ver asignados (médicos)",
  "MedicalRecords.Create": "Crear registros",
  "MedicalRecords.Update": "Editar registros"
}
```

#### 💊 Prescripciones (Prescriptions)
```javascript
{
  "Prescriptions.Create": "Crear prescripciones",
  "Prescriptions.View": "Ver prescripciones",
  "Prescriptions.Update": "Actualizar prescripciones"
}
```

#### 🏢 Clínicas (Clinics)
```javascript
{
  "Clinics.View": "Ver clínicas",
  "Clinics.Manage": "Gestionar clínicas"
}
```

#### 👔 Roles (Roles)
```javascript
{
  "Roles.View": "Ver roles",
  "Roles.Assign": "Asignar roles a usuarios",
  "Roles.Revoke": "Revocar roles",
  "Roles.ViewAudit": "Ver auditoría de roles"
}
```

#### 💰 Facturación (Billing)
```javascript
{
  "Billing.View": "Ver datos de facturación",
  "Billing.Manage": "Gestionar facturación"
}
```

#### 📊 Reportes (Reports)
```javascript
{
  "Reports.Generate": "Generar reportes",
  "Reports.View": "Ver reportes"
}
```

---

## 🛡️ Políticas de Autorización

Las políticas controlan el **acceso a nivel de endpoint**.

### Políticas Implementadas

#### 1. **Políticas Basadas en Permisos**
```csharp
// Los endpoints usan estas políticas
[Authorize(Policy = "Patients.ViewAll")]
[Authorize(Policy = "Users.Manage")]
[Authorize(Policy = "MedicalRecords.Create")]
```

#### 2. **Políticas de Multi-Tenancy**

```javascript
// ViewUsersPolicy
- SuperAdmin: ✅ Ve TODOS los usuarios
- AccountAdmin: ✅ Ve usuarios de su Account
- ClinicAdmin: ✅ Ve usuarios de su Clinic
- Otros: ❌ Acceso denegado

// ViewPatientsPolicy
- SuperAdmin: ✅ Metadata de todos
- AccountAdmin: ✅ Su Account
- ClinicAdmin: ✅ Su Clinic
- Doctor: ✅ Su Clinic
- HealthProfessional: ✅ Su Clinic
- Otros: ❌ Acceso denegado

// ViewAppointmentsPolicy
- SuperAdmin: ✅ Todos
- AccountAdmin: ✅ Su Account
- ClinicAdmin: ✅ Su Clinic
- Doctor: ✅ Su Clinic
- Receptionist: ✅ Su Clinic
- Otros: ✅ Propias citas

// ManageUsersPolicy
- SuperAdmin: ✅ Crear cualquier usuario
- AccountAdmin: ✅ Crear en su Account
- ClinicAdmin: ✅ Crear en su Clinic
- Otros: ❌ Acceso denegado

// ManagePatientsPolicy
- SuperAdmin: ✅ Todos
- AccountAdmin: ✅ Su Account
- ClinicAdmin: ✅ Su Clinic
- Doctor: ✅ Su Clinic
- Otros: ❌ Acceso denegado

// ViewAuditLogPolicy
- SuperAdmin: ✅ Auditoría global
- AccountAdmin: ✅ Auditoría de su Account
- Otros: ❌ Acceso denegado

// AdministerAccountPolicy
- SuperAdmin: ✅ Administrar cualquier cuenta
- AccountAdmin: ✅ Administrar su Account
- Otros: ❌ Acceso denegado
```

---

## 🔑 Flujo de Autenticación

### 1. **Registro (Sign Up)**

```
POST /api/user/register
{
  "name": "John Doe",
  "email": "john@hospital.com",
  "password": "SecurePass123!",
  "acceptPrivacyTerms": true
}
        ↓
┌──────────────────────────────────┐
│ Backend procesa registro         │
├──────────────────────────────────┤
│ 1. Valida email único            │
│ 2. Hashea contraseña (BCrypt)    │
│ 3. Crea User en Account "default"│
│ 4. Asigna rol "AccountAdmin"     │
│ 5. Genera JWT Token              │
└──────────────────────────────────┘
        ↓
{
  "id": 1,
  "name": "John Doe",
  "email": "john@hospital.com",
  "token": "eyJhbGc...",
  "role": "AccountAdmin"
}
```

### 2. **Login (Sign In)**

```
POST /api/user/login
{
  "email": "john@hospital.com",
  "password": "SecurePass123!"
}
        ↓
┌──────────────────────────────────┐
│ Backend valida credenciales      │
├──────────────────────────────────┤
│ 1. Busca usuario por email       │
│ 2. Valida contraseña (BCrypt)    │
│ 3. Obtiene rol del usuario       │
│ 4. Obtiene permisos del rol      │
│ 5. Genera JWT Token              │
└──────────────────────────────────┘
        ↓
{
  "id": 1,
  "name": "John Doe",
  "email": "john@hospital.com",
  "token": "eyJhbGc...",
  "role": "AccountAdmin",
  "permissions": ["Users.ViewAll", "Patients.Create", ...]
}
```

### 3. **JWT Token Structure**

```javascript
Header: {
  "alg": "HS256",
  "typ": "JWT"
}

Payload: {
  "nameid": "1",                    // UserId
  "email": "john@hospital.com",
  "role": "AccountAdmin",            // Rol principal
  "account_id": "5",                 // AccountId
  "clinic_id": "10",                 // ClinicId (si aplica)
  "iss": "MedPalAPI",
  "aud": "MedPalApp",
  "exp": 1234567890
}

Signature: HMACSHA256(...)
```

---

## 📱 Guía de Integración Frontend

### Paso 1: Configurar Interceptores HTTP

```typescript
// auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (token) {
    // Agregar token a headers
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req);
};
```

### Paso 2: Crear Servicio de Autenticación

```typescript
// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5126/api';
  
  private currentUserSubject = new BehaviorSubject(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadUserFromStorage();
  }

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/user/register`, data).pipe(
      tap(response => this.saveUser(response))
    );
  }

  login(email: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/user/login`, { email, password }).pipe(
      tap(response => this.saveUser(response))
    );
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('token');
    this.currentUserSubject.next(null);
  }

  private saveUser(response: any): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('currentUser', JSON.stringify(response));
    this.currentUserSubject.next(response);
  }

  private loadUserFromStorage(): void {
    const user = localStorage.getItem('currentUser');
    if (user) {
      this.currentUserSubject.next(JSON.parse(user));
    }
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getCurrentUser(): any {
    return this.currentUserSubject.value;
  }

  getRole(): string | null {
    const user = this.currentUserSubject.value;
    return user?.role || null;
  }

  hasRole(role: string): boolean {
    return this.getRole() === role;
  }

  hasPermission(permission: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.permissions?.includes(permission) || false;
  }

  isAuthenticated(): boolean {
    return !!this.getToken() && !!this.currentUserSubject.value;
  }
}
```

### Paso 3: Crear Guards de Autenticación

```typescript
// auth.guard.ts
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};

// role.guard.ts
export const roleGuard = (expectedRoles: string[]) => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    
    const currentRole = authService.getRole();
    if (expectedRoles.includes(currentRole)) {
      return true;
    }

    router.navigate(['/unauthorized']);
    return false;
  };
};
```

### Paso 4: Configurar Rutas

```typescript
// app.routes.ts
const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  
  {
    path: 'admin',
    component: AdminComponent,
    canActivate: [authGuard, roleGuard(['SuperAdmin', 'AccountAdmin', 'ClinicAdmin'])]
  },
  
  {
    path: 'medical-records',
    component: MedicalRecordsComponent,
    canActivate: [authGuard, roleGuard(['Doctor', 'HealthProfessional'])]
  }
];
```

### Paso 5: Usar en Componentes

```typescript
// dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-dashboard',
  template: `
    <div *ngIf="currentUser$ | async as user">
      <h1>Bienvenido, {{ user.name }}</h1>
      <p>Rol: <strong>{{ user.role }}</strong></p>
      
      <button *ngIf="authService.hasRole('SuperAdmin')">
        Administrar Sistema
      </button>
      
      <button *ngIf="authService.hasPermission('Users.Manage')">
        Gestionar Usuarios
      </button>
      
      <button *ngIf="authService.hasPermission('Patients.ViewAll')">
        Ver Pacientes
      </button>
      
      <button (click)="logout()">Cerrar Sesión</button>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  currentUser$ = this.authService.currentUser$;

  constructor(public authService: AuthService) {}

  logout(): void {
    this.authService.logout();
  }
}
```

### Paso 6: Ejemplo de Llamadas API

```typescript
// patient.service.ts
@Injectable({ providedIn: 'root' })
export class PatientService {
  private apiUrl = 'http://localhost:5126/api/patients';

  constructor(private http: HttpClient) {}

  // Endpoint con permiso
  getAllPatients(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
    // Automáticamente incluye: Authorization: Bearer <token>
    // Backend valida: ¿Usuario tiene "Patients.ViewAll"?
  }

  getPatientById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  createPatient(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
    // Backend valida: ¿Usuario tiene "Patients.Create"?
  }

  updatePatient(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data);
    // Backend valida: ¿Usuario tiene "Patients.Update"?
  }

  deletePatient(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
    // Backend valida: ¿Usuario tiene "Patients.Delete"?
  }
}
```

---

## 📊 Matriz de Roles vs Permisos

| Rol | Users | Patients | Appointments | Medical<br/>Records | Billing | Clinics | Roles |
|-----|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **SuperAdmin** | ✅✅ | ⚠️ | ⚠️ | ❌ | ✅ | ✅ | ✅ |
| **AccountAdmin** | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ |
| **ClinicAdmin** | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ |
| **Doctor** | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| **HealthProf** | ❌ | ✅ | ✅ | ⚠️ | ❌ | ✅ | ❌ |
| **Receptionist** | ❌ | ✅ | ✅ | ❌ | ✅ | ✅ | ❌ |
| **Patient** | ❌ | ⚠️ | ⚠️ | ⚠️ | ❌ | ❌ | ❌ |

**Leyenda:**
- ✅ = Acceso completo
- ⚠️ = Acceso limitado (solo propio o su scope)
- ❌ = Sin acceso

---

## 🔄 Flujo Completo: Usuario Realizando Acción

```
┌─────────────────────────────────────────────────────────┐
│ 1. USUARIO EN FRONTEND                                  │
│    Hace click en "Ver Pacientes"                         │
└─────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────┐
│ 2. FRONTEND                                              │
│    GET /api/patients                                     │
│    Headers: Authorization: Bearer <token>               │
└─────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────┐
│ 3. BACKEND: Authentication Middleware                   │
│    ✓ Valida JWT token                                   │
│    ✓ Extrae UserId del token                            │
│    ✓ Extrae Role del token                              │
│    ✓ Extrae AccountId del token                         │
└─────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────┐
│ 4. BACKEND: Authorization Policy                        │
│    Endpoint: [Authorize(Policy = "Patients.ViewAll")]   │
│                                                          │
│    Policy verifica:                                      │
│    ¿Usuario tiene permiso "Patients.ViewAll"?           │
│                                                          │
│    PermissionHandler:                                    │
│    1. Obtiene permisos del usuario                       │
│    2. Comprueba si tiene "Patients.ViewAll"             │
│    3. Si ✓ continúa, si ✗ retorna 403                   │
└─────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────┐
│ 5. BACKEND: Multi-Tenancy Query Filter                  │
│    AppDbContext aplicará filter automático:             │
│                                                          │
│    if (IsSuperAdmin)                                     │
│      return ALL patients                                │
│    else if (IsAccountAdmin)                             │
│      return patients WHERE AccountId = CurrentAccountId │
│    else if (IsClinicAdmin)                              │
│      return patients WHERE ClinicId = CurrentClinicId   │
│                                                          │
│    Solo devuelve datos que el usuario puede ver         │
└─────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────┐
│ 6. BACKEND: Respuesta                                   │
│    {                                                     │
│      "data": [patient1, patient2, ...],                 │
│      "total": 15,                                        │
│      "status": "success"                                 │
│    }                                                     │
└─────────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────────┐
│ 7. FRONTEND: Procesa Respuesta                          │
│    Muestra lista de pacientes en tabla                   │
└─────────────────────────────────────────────────────────┘
```

---

## ⚡ Casos de Uso Prácticos

### Caso 1: SuperAdmin Viendo Auditoría Global

```
SuperAdmin intenta: GET /api/audit/logs

1. ✓ Autenticación OK (token válido)
2. ✓ Autorización OK (Policy = ViewAuditLogPolicy → SuperAdmin ✓)
3. ✓ Query Filter (IsSuperAdmin = true → ve TODO)
4. Respuesta: Auditoría de TODOS los usuarios, cuentas, clínicas
```

### Caso 2: AccountAdmin Creando Usuario en Su Cuenta

```
AccountAdmin intenta: POST /api/user/create
Body: { email: "newuser@clinic.com", ... }

1. ✓ Autenticación OK
2. ✓ Autorización OK (Policy = ManageUsersPolicy → AccountAdmin ✓)
3. ✓ Validación: El usuario nuevo está en su Account
4. ✓ Crea usuario en su Account (AccountId = CurrentAccountId)
5. ✓ NO puede crear en otra Account
6. Respuesta: Usuario creado exitosamente
```

### Caso 3: ClinicAdmin Viendo Solo Sus Pacientes

```
ClinicAdmin intenta: GET /api/patients

1. ✓ Autenticación OK
2. ✓ Autorización OK (Policy = ViewPatientsPolicy → ClinicAdmin ✓)
3. Query Filter automático:
   WHERE ClinicId = CurrentClinicId (su clínica)
4. Respuesta: Solo pacientes de su clínica
```

### Caso 4: Doctor Creando Medical Record

```
Doctor intenta: POST /api/medical-records
Body: { patientId: 42, diagnosis: "...", ... }

1. ✓ Autenticación OK
2. ✓ Autorización OK (Policy = MedicalRecords.Create → Doctor ✓)
3. ✓ Validación: Paciente está en su clínica
4. ✓ Crea registro (se asocia a su clínica)
5. Respuesta: Registro creado
```

### Caso 5: Patient Intentando Ver Paciente Ajeno

```
Patient intenta: GET /api/patients/42

1. ✓ Autenticación OK
2. ❌ Autorización FALLA
   - Policy = ViewPatientsPolicy
   - Patient no está en la lista permitida
3. Respuesta: 403 Forbidden
```

---

## 🚨 Errores Comunes y Soluciones

### Error 1: "401 Unauthorized"
```
Causa: Token ausente o expirado
Solución:
- Verificar que AuthInterceptor agrega token
- Verificar que token se guardó en localStorage
- Hacer login nuevamente
```

### Error 2: "403 Forbidden"
```
Causa: Usuario no tiene permiso
Solución:
- Verificar rol del usuario en BD
- Verificar permiso está asignado al rol
- Verificar Policy en endpoint
```

### Error 3: "Ver datos de otra clínica"
```
Causa: Query Filter no aplicado correctamente
Solución:
- Verificar ITenantContextService está inyectado
- Verificar Query Filter en AppDbContext.OnModelCreating
- Verificar CurrentClinicId se obtiene del JWT
```

---

## 📞 Contacto y Soporte

Para preguntas sobre:
- **Roles y Permisos**: Revisar matriz de permisos arriba
- **Integración Frontend**: Ver guía de integración
- **Errores de Autenticación**: Verificar JWT token en jwt.io
- **Problemas Multi-Tenancy**: Verificar CurrentAccountId y CurrentClinicId

---

**Documento generado:** 12/01/2026  
**Autor:** Backend Team - MedPal  
**Versión:** 1.0 - Completo y Testeable
