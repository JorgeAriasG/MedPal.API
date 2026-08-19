# Deuda técnica aceptada — AutoMapper 13.0.1 (NU1903 / CVE-2026-32933)

**Fecha decision:** 2026-08-07
**Estado:** Aceptado temporalmente (Opción B) — pendiente de re-evaluación

---

## Contexto de la vulnerabilidad

- **Advisory:** GHSA-rvv3-g6hj-g44x ([link](https://github.com/LuckyPennySoftware/AutoMapper/security/advisories/GHSA-rvv3-g6hj-g44x))
- **CVE:** CVE-2026-32933
- **Severity:** 7.5 HIGH (CVSS 3.1, vector `AV:N/AC:L/PR:N/UI:N/S:U/C:N/I:N/A:H`) — solo disponibilidad (DoS).
- **Versiones afectadas:** todas < 15.1.1 y 16.0.0–16.1.0.
- **Versiones con parche:** 15.1.1 / 15.1.2 / 15.1.3 y 16.1.1 (ninguna en la serie 13.x ni 14.x).
- **Mecanismo:** mapeo de grafos de objetos profundamente anidados (25,000+ niveles) sin límite de profundidad por defecto → `StackOverflowException` que tumba el proceso.
- **Fix del proveedor:** `MaxDepth = 64` por defecto para tipos auto-referenciales cuando `CheckForCycles` habilita `PreserveReferences`.

## Por qué aceptamos el riesgo

1. **Licencia:** desde 15.0.0 AutoMapper ya no es MIT. Pasa a licencia dual comercial/OSS y **exige license key** (`cfg.LicenseKey`) en runtime, obtenible en automapper.io. Las versiones < 15.0.0 se mantienen MIT y gratuitas para siempre. No hay versión **parcheada** que sea libre de licencia.
2. **Explotabilidad práctica en este proyecto:**
   - El API mapea entidades internas de EF Core (con lazy-loading proxies) hacia DTOs planos y acotados; no expone grafos de entrada controlables por el atacante con profundidad arbitraria.
   - `MappingProfile.cs` usa solo `CreateMap/ForMember/MapFrom/Ignore/ReverseMap` — sin recursion profunda en los contratos del API.
3. Superficie de uso mínima: 1 `Profile` (~90 mapas), `IMapper` inyectado, y `AddAutoMapper` (incluido en el propio `AutoMapper.dll`, no hay paquete de extensión separado).

## Exposición/visibilidad

- `dotnet build`/`dotnet test` emiten `warning NU1903` en cada build a propósito: mientras la excepción esté vigente, el warning se mantiene visible como recordatorio.
- No se añade `NuGetAuditSuppress` en el csproj mientras dure la aceptación.
- Si en el futuro se decide silenciar con retén documentada, entry: `<NuGetAuditSuppress Include="https://github.com/advisories/GHSA-rvv3-g6hj-g44x" />` en `MedPal.API.csproj`.

## Plan de remediación (cuando se puedan adquirir licencia/dec hombros)

1. Asegurar license key comercial (o admitir elegibilidad OSS) en automapper.io.
2. `MedPal.API.csproj`: `AutoMapper` 13.0.1 → **15.1.3**.
3. `Program.cs:95`: `builder.Services.AddAutoMapper(cfg => { cfg.LicenseKey = "<key>"; }, typeof(MappingProfile));`.
4. `MedPal.API.Tests/Mapping/AutoMapperTests.cs`: el ctor `MapperConfiguration(...)` en 15.x requiere `ILoggerFactory` → pasar `NullLoggerFactory.Instance`.
5. Verificar: `dotnet build -c Release` → `dotnet test -c Release` (esperado 52 pass / 2 rojos preexistentes) → smoke login + endpoints de pacientes.
6. Posible impacto de v14 (sealed classes) y v15 (`MaxDepth=64`) evaluado como no aplicable al uso actual.

## Cotejo rápido

| Item | Valor |
|------|-------|
| Package | `AutoMapper` 13.0.1 |
| Target framework | net8.0 |
| Advisory | GHSA-rvv3-g6hj-g44x |
| Owner decisión | Jorge (propietario) |