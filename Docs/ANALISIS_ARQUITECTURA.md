# Análisis de Redundancia Arquitectónica - MedicalHistory vs Prescription

## Problema Identificado

Existe una **redundancia clara** entre `MedicalHistory` y `Prescription`:

### 1. **PrescribedMedications en MedicalHistory**
   - **MedicalHistory.PrescribedMedications**: String simple (redundante)
   - **Prescription + PrescriptionItems**: Estructura completa y detallada para medicamentos
   - **Conclusión**: ELIMINAR - Los medicamentos deben gestionarse SOLO via Prescription/PrescriptionItems

### 2. **TreatmentPlan en MedicalHistory**
   - **Actual**: Campo string genérico que duplica funcionalidad
   - **Mejor enfoque**: Debería vincularse a través de Prescription
   - **Conclusión**: TRANSFORMAR - Hacer que MedicalHistory referencie Prescription (si existe)

### 3. **TreatmentStatus en MedicalHistory**
   - **Actual**: String en MedicalHistory + PrescriptionStatus en Prescription
   - **Problema**: Duplicación de estados de tratamiento
   - **Conclusión**: ELIMINAR - Usar PrescriptionStatus como fuente única de verdad

### 4. **Diagnosis Duplicado**
   - **MedicalHistory.Diagnosis**: Diagnóstico de la consulta
   - **Prescription.Diagnosis**: Diagnóstico para la receta
   - **Conclusión**: MANTENER SEPARADOS pero aclarar propósito (son contextos diferentes)

## Arquitectura Propuesta

### Responsabilidad Limpia:

**MedicalHistory** (Registro clínico puro):
- Id
- PatientDetailsId (FK)
- SpecialtyType (Cardiología, Odontología, etc)
- SpecialtyData (JSON polimórfico para datos específicos)
- Diagnosis (diagnóstico de la consulta médica)
- DiagnosisDate
- ClinicalNotes (notas del médico)
- HealthcareProfessionalId (quién realizó el diagnóstico)
- FollowUpDate (próxima consulta)
- IsConfidential
- CreatedAt, UpdatedAt, LastModifiedByUserId
- **NUEVA RELACIÓN**: Prescription (opcional - si se prescribió algo)

**Prescription** (Gestión de medicamentos):
- Id
- UniqueCode
- DoctorId
- PatientId
- Diagnosis (propósito de la prescripción)
- Notes
- IssuedAt
- ExpiresAt
- **Status** (fuente única para estado de medicación)
- Items[] (PrescriptionItem con medicamentos)
- **NUEVA RELACIÓN**: MedicalHistoryId (FK opcional - vinculado a la consulta)

**PrescriptionItem** (Medicamentos individuales):
- Medicación específica con dosis, frecuencia, duración
- (Sin cambios requeridos)

## Cambios Recomendados

### ❌ ELIMINAR de MedicalHistory:
1. `PrescribedMedications` (string)
2. `TreatmentStatus` (string)
3. `TreatmentPlan` (string) - o transformar a una relación

### ✅ AGREGAR a MedicalHistory:
1. `PrescriptionId` (FK opcional - nullable) - relación con Prescription si se prescribió

### ✅ AGREGAR a Prescription:
1. `MedicalHistoryId` (FK opcional - nullable) - referencias de qué consulta originó esta prescripción

### ✅ MANTENER:
1. `Diagnosis` en ambos (contextos diferentes)
2. `ClinicalNotes` en MedicalHistory
3. `FollowUpDate` en MedicalHistory

## Flujo de Datos Resultante

```
Paciente
  └── PatientDetails
       ├── MedicalHistory (diagnóstico + notas clínicas)
       │    └── Prescription (opcional - si se prescribe)
       │         └── PrescriptionItems (medicamentos)
       └── Allergies (alergias del paciente)
```

## Impacto

- ✅ Una sola fuente de verdad para medicamentos
- ✅ Una sola fuente de verdad para estado de medicación
- ✅ MedicalHistory enfocado en registro clínico
- ✅ Prescription enfocado en gestión de medicamentos
- ✅ Auditoría más clara (cada entidad tiene sus propios timestamps)
