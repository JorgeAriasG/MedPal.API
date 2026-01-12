# Checklist Fase 4: Testing y Validación de Seguridad

**Duración Estimada:** 2-3 días  
**Estado General:** ⏳ Pendiente  
**Requisito Previo:** Fases 1, 2 y 3 completadas  
**Última actualización:** 12 de enero de 2026

---

## 📋 Tareas

### 4.1 Unit Tests - Query Filters

**Archivo:** `Tests/QueryFilters/UserQueryFilterTests.cs`

- [ ] Test: SuperAdmin ve todos los usuarios
  - [ ] Setup: Usuario SuperAdmin
  - [ ] Query: DbContext.Users.ToList()
  - [ ] Assert: Retorna usuarios de todas las cuentas
  
- [ ] Test: AccountAdmin ve solo su cuenta
  - [ ] Setup: Usuario AccountAdmin de Cuenta A
  - [ ] Query: DbContext.Users.ToList()
  - [ ] Assert: Retorna solo usuarios de Cuenta A
  
- [ ] Test: ClinicAdmin ve solo su clínica
  - [ ] Setup: Usuario ClinicAdmin de Clínica A
  - [ ] Query: DbContext.Users.ToList()
  - [ ] Assert: Retorna solo usuarios de Clínica A

- [ ] Test: Doctor ve solo su clínica
  - [ ] Setup: Usuario Doctor de Clínica A
  - [ ] Query: DbContext.Users.ToList()
  - [ ] Assert: Retorna solo usuarios de Clínica A

**Progreso:** 0/4 completado

---

### 4.2 Unit Tests - Query Filters (Patients)

**Archivo:** `Tests/QueryFilters/PatientQueryFilterTests.cs`

- [ ] Test: SuperAdmin ve todos los pacientes
- [ ] Test: AccountAdmin ve solo pacientes de su cuenta
- [ ] Test: ClinicAdmin ve solo pacientes de su clínica
- [ ] Test: Doctor ve solo pacientes de su clínica

**Progreso:** 0/4 completado

---

### 4.3 Unit Tests - Patient Consent

**Archivo:** `Tests/Services/PatientConsentServiceTests.cs`

- [ ] Test: Crear consentimiento válido
  - [ ] Setup: Paciente, dos clínicas
  - [ ] Action: CreateConsentAsync(...)
  - [ ] Assert: Consentimiento creado y guardado
  
- [ ] Test: IsConsentValidAsync retorna true para consentimiento válido
  - [ ] Setup: Consentimiento aprobado, no expirado
  - [ ] Action: IsConsentValidAsync(...)
  - [ ] Assert: Retorna true
  
- [ ] Test: IsConsentValidAsync retorna false para consentimiento no aprobado
  - [ ] Setup: Consentimiento no aprobado
  - [ ] Action: IsConsentValidAsync(...)
  - [ ] Assert: Retorna false
  
- [ ] Test: IsConsentValidAsync retorna false para consentimiento expirado
  - [ ] Setup: Consentimiento con ExpiryDate pasada
  - [ ] Action: IsConsentValidAsync(...)
  - [ ] Assert: Retorna false
  
- [ ] Test: Revocar consentimiento (soft delete)
  - [ ] Setup: Consentimiento existente
  - [ ] Action: RevokeConsentAsync(...)
  - [ ] Assert: IsDeleted = true

**Progreso:** 0/5 completado

---

### 4.4 Unit Tests - Medical Record Access

**Archivo:** `Tests/Services/MedicalRecordAccessTests.cs`

- [ ] Test: Doctor de Clínica A puede acceder a registros de Clínica A
  - [ ] Setup: MedicalRecord.OwnerClinicId = A, Usuario es Doctor de A
  - [ ] Action: Consultar registro
  - [ ] Assert: Acceso permitido
  
- [ ] Test: Doctor de Clínica B NO puede acceder a registros de Clínica A (sin consent)
  - [ ] Setup: MedicalRecord.OwnerClinicId = A, Usuario es Doctor de B
  - [ ] Action: Consultar registro
  - [ ] Assert: Acceso denegado (403)
  
- [ ] Test: Doctor de Clínica B PUEDE acceder si tiene consentimiento válido
  - [ ] Setup: MedicalRecord de A, Doctor de B con consentimiento aprobado
  - [ ] Action: Consultar registro
  - [ ] Assert: Acceso permitido
  
- [ ] Test: Acceso se registra en AuditLog
  - [ ] Setup: Doctor accede a MedicalRecord
  - [ ] Action: Consultar registro
  - [ ] Assert: Registro en MedicalRecordAccessLog creado

**Progreso:** 0/4 completado

---

### 4.5 Unit Tests - Audit Log

**Archivo:** `Tests/Services/AuditLogServiceTests.cs`

- [ ] Test: LogMedicalRecordAccessAsync registra acceso
  - [ ] Setup: Datos de acceso
  - [ ] Action: LogMedicalRecordAccessAsync(...)
  - [ ] Assert: Registro en BD
  
- [ ] Test: GetAccessHistoryAsync retorna historial
  - [ ] Setup: Múltiples accesos registrados
  - [ ] Action: GetAccessHistoryAsync(patientId)
  - [ ] Assert: Retorna todos los accesos
  
- [ ] Test: GetAccessHistoryByUserAsync
  - [ ] Setup: Usuario accede múltiples registros
  - [ ] Action: GetAccessHistoryByUserAsync(userId)
  - [ ] Assert: Retorna accesos del usuario

**Progreso:** 0/3 completado

---

### 4.6 Integration Tests - Autorización

**Archivo:** `Tests/Integration/AuthorizationTests.cs`

- [ ] Test: SuperAdmin accede a endpoint ViewUsers
  - [ ] Setup: Token SuperAdmin
  - [ ] Action: GET /api/users
  - [ ] Assert: 200 OK + todos los usuarios
  
- [ ] Test: AccountAdmin accede a endpoint ViewUsers
  - [ ] Setup: Token AccountAdmin de Cuenta A
  - [ ] Action: GET /api/users
  - [ ] Assert: 200 OK + solo usuarios de Cuenta A
  
- [ ] Test: ClinicAdmin NO accede a usuaros de otra clínica
  - [ ] Setup: Token ClinicAdmin de Clínica A, querystring con Clínica B
  - [ ] Action: GET /api/users?clinicId=B
  - [ ] Assert: 403 Forbidden
  
- [ ] Test: Doctor no puede crear usuarios
  - [ ] Setup: Token Doctor
  - [ ] Action: POST /api/users
  - [ ] Assert: 403 Forbidden

**Progreso:** 0/4 completado

---

### 4.7 Integration Tests - Consentimiento

**Archivo:** `Tests/Integration/ConsentFlowTests.cs`

- [ ] Test: Paciente puede crear consentimiento
  - [ ] Setup: Paciente, dos clínicas
  - [ ] Action: POST /api/consents (con token de paciente)
  - [ ] Assert: 201 Created
  
- [ ] Test: Doctor de otra clínica NO puede acceder sin consentimiento
  - [ ] Setup: MedicalRecord de A, Doctor de B sin consent
  - [ ] Action: GET /api/medicalhistories/{id}
  - [ ] Assert: 403 Forbidden
  
- [ ] Test: Doctor de otra clínica PUEDE acceder con consentimiento
  - [ ] Setup: Paciente creó consentimiento aprobado
  - [ ] Action: GET /api/medicalhistories/{id}
  - [ ] Assert: 200 OK + registro
  
- [ ] Test: Paciente puede revocar consentimiento
  - [ ] Setup: Consentimiento existente
  - [ ] Action: DELETE /api/consents/{id}
  - [ ] Assert: 204 No Content
  
- [ ] Test: Revocación impide acceso posterior
  - [ ] Setup: Consentimiento revocado
  - [ ] Action: GET /api/medicalhistories/{id} (doctor otra clínica)
  - [ ] Assert: 403 Forbidden

**Progreso:** 0/5 completado

---

### 4.8 Integration Tests - Auditoría

**Archivo:** `Tests/Integration/AuditLogTests.cs`

- [ ] Test: Acceso a registro se registra en AuditLog
  - [ ] Setup: Doctor accede a MedicalRecord
  - [ ] Action: GET /api/medicalhistories/{id}
  - [ ] Assert: Registro en BD + timestamp + userId
  
- [ ] Test: Paciente puede ver su historial de accesos
  - [ ] Setup: Múltiples accesos registrados
  - [ ] Action: GET /api/audit/my-access-history
  - [ ] Assert: 200 OK + lista de accesos
  
- [ ] Test: Doctor NO puede ver historial de otro usuario
  - [ ] Setup: Token Doctor A, querystring con Doctor B
  - [ ] Action: GET /api/audit/users/{userId}/access-history
  - [ ] Assert: 403 Forbidden
  
- [ ] Test: AccountAdmin PUEDE ver historial de su cuenta
  - [ ] Setup: Token AccountAdmin, accesos en su cuenta
  - [ ] Action: GET /api/audit/account/access-history
  - [ ] Assert: 200 OK + accesos de su cuenta
  
- [ ] Test: Logs no se pueden borrar
  - [ ] Setup: AuditLog existente
  - [ ] Action: DELETE /api/audit/{logId}
  - [ ] Assert: 403 Forbidden o método no existe

**Progreso:** 0/5 completado

---

### 4.9 Security Penetration Testing

**Manual Testing Checklist:**

- [ ] **Test SuperAdmin Lateral Movement**
  - [ ] SuperAdmin intenta ver Medical Records directamente
  - [ ] Resultado esperado: ✗ NO puede (sin estar en policy)
  - [ ] Pero sí ve logs de auditoría
  
- [ ] **Test AccountAdmin Lateral Movement**
  - [ ] AccountAdmin de Cuenta A intenta acceder a Cuenta B
  - [ ] Resultado esperado: ✗ Query filter bloquea
  
- [ ] **Test Doctor Privilege Escalation**
  - [ ] Doctor intenta cambiar su ClinicId
  - [ ] Resultado esperado: ✗ API valida cambios
  
- [ ] **Test Consent Tampering**
  - [ ] Doctor intenta crear consentimiento falso
  - [ ] Resultado esperado: ✗ Solo paciente puede crear
  
- [ ] **Test AuditLog Tampering**
  - [ ] Usuario intenta borrar AuditLog
  - [ ] Resultado esperado: ✗ NO existe endpoint DELETE
  
- [ ] **Test Token Manipulation**
  - [ ] Modificar claims en token JWT
  - [ ] Resultado esperado: ✗ Validación rechaza

**Progreso:** 0/6 completado

---

### 4.10 Performance Testing

**Archivo:** `Tests/Performance/QueryPerformanceTests.cs`

- [ ] Test: Query de usuarios con 1000 registros
  - [ ] Setup: 1000 usuarios en 10 cuentas
  - [ ] Action: GET /api/users (sin consentimiento)
  - [ ] Assert: < 500ms respuesta
  
- [ ] Test: Query de Medical Records con filtro de consentimiento
  - [ ] Setup: 100 registros + 5 consentimientos
  - [ ] Action: GET /api/medicalhistories
  - [ ] Assert: < 300ms respuesta
  
- [ ] Test: AuditLog insert debe ser rápido
  - [ ] Setup: Acceso a 10 registros seguidos
  - [ ] Action: Múltiples accesos
  - [ ] Assert: Cada log < 50ms

**Progreso:** 0/3 completado

---

### 4.11 Data Consistency Testing

**Manual Checks:**

- [ ] Verificar que AccountId está asignado a:
  - [ ] Todos los Users
  - [ ] Todas las Clinics
  - [ ] Todos los Patients
  
- [ ] Verificar que OwnerClinicId está asignado a:
  - [ ] Todos los MedicalHistories
  
- [ ] Verificar que no hay orfandades:
  - [ ] Ningún User sin Account (si es requerido)
  - [ ] Ningún Clinic sin Account (si es requerido)
  
- [ ] Verificar integridad de relaciones:
  - [ ] UserClinic.ClinicId existe en Clinics
  - [ ] PatientConsent.RequestingClinicId existe
  - [ ] PatientConsent.OwnerClinicId existe

**Progreso:** 0/4 completado

---

### 4.12 Compliance Validation

**NOM-004 (México) - Protección de Datos Médicos**

- [ ] ✓ Confidencialidad: Solo usuarios autorizados acceden a Medical Records
- [ ] ✓ Integridad: Auditoría registra todos los accesos
- [ ] ✓ Disponibilidad: Sistemas sin downtime intencional
- [ ] ✓ Consentimiento: Paciente controla compartición de datos
- [ ] ✓ Derecho a revisar: Paciente ve quién accedió sus datos

**Checklist:**
- [ ] Documentar cumplimiento NOM-004
- [ ] Documentar cumplimiento HIPAA (si aplica)
- [ ] Documentar cumplimiento GDPR (si aplica)
- [ ] Crear reporte de validación

**Progreso:** 0/4 completado

---

### 4.13 Testing Data Setup

**Archivo:** `Tests/Fixtures/TestDataFixture.cs`

- [ ] Crear datos de prueba:
  - [ ] 3 Cuentas (Account)
  - [ ] 2 Clínicas por cuenta
  - [ ] 5 Usuarios por cuenta con roles variados
  - [ ] 10 Pacientes por cuenta
  - [ ] 10 MedicalHistories distribuidos
  - [ ] 5 Consentimientos entre clínicas
  
- [ ] Crear métodos helper:
  - [ ] GetTestUser(role, accountId)
  - [ ] GetTestPatient(accountId)
  - [ ] GetTestMedicalRecord(clinicId)
  - [ ] CreateTestConsent(...)

**Progreso:** 0/1 completado

---

### 4.14 Test Execution

- [ ] Ejecutar todos los unit tests
  ```bash
  dotnet test --filter "FullyQualifiedName~Tests"
  ```
- [ ] Resultado esperado: Todos pasan
- [ ] Coverage mínimo: 80% en servicios críticos
- [ ] Documentar resultados

**Progreso:** 0/1 completado

---

### 4.15 Documentation y Reportes

- [ ] Crear "Security Testing Report"
  - [ ] Resumen de tests ejecutados
  - [ ] Resultados
  - [ ] Vulnerabilidades encontradas (si hay)
  - [ ] Recomendaciones
  
- [ ] Crear "Compliance Report"
  - [ ] NOM-004 Checklist completado
  - [ ] HIPAA Checklist completado
  - [ ] GDPR Checklist completado
  
- [ ] Actualizar [README.md](README.md)
  - [ ] Fase 4 completada
  - [ ] Link a reportes

**Progreso:** 0/3 completado

---

## 📊 Resumen de Progreso

### Por Categoría

| Categoría | Estado | Progreso |
|-----------|--------|----------|
| Unit Tests - Filters | ⏳ Pendiente | 0/8 |
| Unit Tests - Services | ⏳ Pendiente | 0/8 |
| Integration Tests | ⏳ Pendiente | 0/9 |
| Security Testing | ⏳ Pendiente | 0/6 |
| Performance | ⏳ Pendiente | 0/3 |
| Data Consistency | ⏳ Pendiente | 0/4 |
| Compliance | ⏳ Pendiente | 0/4 |
| Test Data Setup | ⏳ Pendiente | 0/1 |
| Test Execution | ⏳ Pendiente | 0/1 |
| Documentation | ⏳ Pendiente | 0/3 |

**Total:** 0/47 tareas completadas (0%)

---

## 🚀 Después de Fase 4

✅ **Implementación completada**

Próximos pasos:
1. Deploy a ambiente de staging
2. UAT (User Acceptance Testing) con cliente
3. Training de usuarios
4. Deploy a producción

---

## 📝 Notas y Decisiones

### Decisión 1: Coverage mínimo 80%
- **Razón:** Servicios críticos de seguridad requieren alta cobertura
- **Alternativa:** 100% coverage
- **Decisión:** 80% es práctico y mantiene velocidad
- **Estado:** APROBADO

### Decisión 2: Manual penetration testing
- **Razón:** Algunas vulnerabilidades son difíciles de automatizar
- **Alternativa:** Solo tests automatizados
- **Estado:** APROBADO

---

## ⚠️ Riesgos y Mitigaciones

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|-----------|
| Tests no encuentran bugs | Media | Alto | Code review + manual testing |
| Performance issues encontrados | Media | Medio | Optimización de índices |
| Cumplimiento no validado | Baja | Crítico | Auditoría externa (opcional) |

---

**Última actualización:** 12 de enero de 2026  
**Responsable:** [Tu nombre]  
**Aprobado por:** [Nombre de aprobador]
