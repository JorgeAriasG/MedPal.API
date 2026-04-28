# 🤖 Backend Agents - Reference Guide

> Medical Scheduling App API | .NET 8 Backend Development

---

## Quick Access to Agents

Este workspace contiene **agentes especializados** que pueden ayudarte con tareas específicas:

⭐ **NEW: Para automatización completa, usa:**
```
@orchestrationagent [feature]
```
Esto coordina TODOS los agentes automáticamente sin intervención manual.
Ver: `scheduling.ui/.github/AUTOMATION_GUIDE.md`

---

**Agentes Individuales:**

| Agente | Especialidad | Comando | Ubicación |
|--------|--------------|---------|----------|
| **@orchestrationagent** | Automatización completa | `@orchestrationagent [feature]` | Workspace Frontend |
| **@backendagent** | .NET / Entity Framework / SQL | `@backendagent [tarea]` | `.github/agents/backendagent.agent.md` |
| **@qaagent** | Testing / xUnit / Integration Tests | `@qaagent [tarea]` | Workspace Frontend |
| **@secopsagent** | Seguridad / JWT / RBAC / Auditoría | `@secopsagent [tarea]` | Workspace Frontend |
| **@scrummaster** | Planificación / Sprints / Coordinación | `@scrummaster [tarea]` | Workspace Frontend |
| **@archagent** | Frontend Angular / NgRx | `@archagent [tarea]` | Workspace Frontend |

---

## Cómo Utilizarlos

### En VS Code Chat:
1. Abre la chat de Copilot
2. Escribe `@` y selecciona el agente
3. Describe tu tarea específica
4. El agente especializado responderá

### Ejemplos de Uso:

**Para tareas Backend:**
```
@backendagent crear un nuevo endpoint POST para crear appointments
@backendagent implementar validación de datos con FluentValidation
@backendagent hacer migration para tabla de prescriptions
@backendagent debuggear error de multi-tenancy en queries
```

**Para Testing:**
```
@qaagent escribir unit tests para AppointmentService
@qaagent crear test plan para feature de pacientes
@qaagent análisis de cobertura de código
```

**Para Seguridad:**
```
@secopsagent auditoría de JWT configuration
@secopsagent revisar RBAC en endpoints
@secopsagent validar multi-tenancy isolation
```

**Para Coordinación:**
```
@scrummaster crear sprint plan para fase 4
@scrummaster identificar dependencias entre frontend y backend
@scrummaster reporte de progreso semanal
```

---

## Características de Cada Agente

### @backendagent 🔧
- Entity Framework Core (migrations, models, configs)
- Controllers REST API
- Services & business logic
- Repositories & data access
- FluentValidation rules
- AutoMapper configurations
- JWT / Authorization
- Multi-tenancy patterns
- Performance optimization

### @archagent 🎨
- Angular components (smart/dumb)
- NgRx state management
- Material Design 3
- Reactive forms
- Services & HTTP calls
- Routing & Guards
- TypeScript strict mode issues

### @qaagent 🧪
- xUnit testing (backend)
- Jasmine testing (frontend)
- Integration tests
- Test plans & coverage
- Bug reporting
- Test case design

### @secopsagent 🔐
- JWT validation
- CORS hardening
- RBAC/Permission validation
- Multi-tenancy isolation
- OWASP compliance
- Dependency scanning
- Secrets management

### @scrummaster 📋
- Sprint planning
- Task breakdown (epics → stories → tasks)
- Dependency mapping
- Risk assessment
- Velocity tracking
- Progress reporting
- Release planning

---

## Flujo de Trabajo Recomendado

```
┌─────────────────────────────────────────────┐
│  @scrummaster                               │
│  Planifica feature → tasks                  │
│  Identifica dependencias                    │
└────────────────┬────────────────────────────┘
                 │
        ┌────────┴────────┐
        ▼                 ▼
   ┌──────────────┐  ┌──────────────┐
   │ @backendagent│  │ @archagent   │
   │ Implementa   │  │ Implementa   │
   │ API/Services │  │ UI/Components│
   │ En paralelo  │  │ En paralelo  │
   └──────┬───────┘  └──────┬───────┘
          │                 │
          └────────┬────────┘
                   ▼
          ┌──────────────────┐
          │ @qaagent         │
          │ Testing completo │
          │ Unit + Integration
          └──────┬───────────┘
                 ▼
          ┌──────────────────┐
          │ @secopsagent     │
          │ Security review  │
          │ Antes del release│
          └──────┬───────────┘
                 ▼
          ┌──────────────────┐
          │ @scrummaster     │
          │ Marca como done  │
          │ Planifica next   │
          └──────────────────┘
```

---

## Configuración de Workspace

### Backend (.NET) - Estructura
```
MedPal.API/
├── .github/
│   └── copilot-instructions.md    ← Lee esto primero
├── Controllers/                    → @backendagent
├── Services/
├── Repositories/
├── Models/
├── DTOs/
├── Data/                          → Migrations, DbContext
├── Validation/                    → FluentValidators
├── Authorization/
└── Program.cs                     → Dependency Injection
```

### Frontend (Angular) - Estructura
```
scheduling.ui/
├── .github/
│   ├── copilot-instructions.md    ← Lee esto primero
│   ├── AGENTS.md                  ← Documentación de agentes
│   └── agents/
│       ├── archagent.agent.md     → @archagent
│       ├── backendagent.agent.md  → @backendagent
│       ├── qaagent.agent.md       → @qaagent
│       ├── secopsagent.agent.md   → @secopsagent
│       └── scrummaster.agent.md   → @scrummaster
└── src/app/
    ├── components/                → @archagent
    ├── services/
    ├── store/
```

---

## Acceso Rápido a Información

### Documentación Backend
- **Instrucciones de Copilot**: `MedPal.API/.github/copilot-instructions.md`
- **Arquitectura**: `MedPal.API/ANALISIS_ARQUITECTURA_COMPLETO.md`
- **Entity Framework**: `MedPal.API/Data/ApplicationDbContext.cs`
- **Multi-tenancy**: `MedPal.API/GUIA_RAPIDA_PRUEBAS_MULTITENANCY.md`

### Documentación Frontend
- **Instrucciones de Copilot**: `scheduling.ui/.github/copilot-instructions.md`
- **Guía de Componentes**: `scheduling.ui/COMPONENT_LIBRARY.md`
- **Sistema de Diseño**: `scheduling.ui/DESIGN_SYSTEM.md`
- **Implementation Guide**: `scheduling.ui/Docs/ANGULAR_IMPLEMENTATION_GUIDE.md`

### Documentación Agentes
- **Todos los agentes**: `scheduling.ui/.github/AGENTS.md` (este archivo en Frontend)

---

## ✅ Checklist: Antes de Empezar a Codear

- [ ] He leído `.github/copilot-instructions.md` (Backend o Frontend según aplique)
- [ ] He identificado qué agente necesito para mi tarea
- [ ] Entiendo el flujo de trabajo (scrum master → dev → testing → security)
- [ ] Conozco la estructura de carpetas de mi workspace
- [ ] He revisado las guías de arquitectura relevantes

---

## 🎯 Próximos Pasos

### Para Desarrollo Backend:
```
@backendagent crear CRUD endpoints para appointments
@backendagent implementar AppointmentService con business logic
@backendagent crear migration para tabla de appointments
@qaagent escribir tests para AppointmentService
@secopsagent auditar seguridad del endpoint de appointments
```

### Para Desarrollo Frontend:
```
@archagent crear AppointmentComponent con formulario
@archagent integrar con API de appointments
@qaagent escribir tests para AppointmentComponent
@secopsagent validar autenticación en requests
```

### Para Coordinación:
```
@scrummaster crear sprint plan para appointment feature
@scrummaster identificar dependencias frontend-backend
@scrummaster reportar progreso diario
```

---

## 💡 Tips Importantes

1. **Especificidad**: Describe bien tu tarea → respuestas mejores
2. **Contexto**: Menciona si es frontend, backend, testing, etc.
3. **Validación**: Siempre pide @secopsagent revisar antes de release
4. **Testing**: @qaagent debe revisar código antes de merge
5. **Coordinación**: @scrummaster debe aprobar tareas críticas

---

## 🔗 Enlaces Útiles

- **Documentación de Agentes**: `scheduling.ui/.github/AGENTS.md`
- **Instrucciones Backend**: `MedPal.API/.github/copilot-instructions.md`
- **Instrucciones Frontend**: `scheduling.ui/.github/copilot-instructions.md`
- **Workspace**: Multi-root (Backend + Frontend)

---

**Última actualización**: March 22, 2026  
**Versión**: 1.0  
**Agentes disponibles**: 5 (archagent, backendagent, qaagent, secopsagent, scrummaster)
