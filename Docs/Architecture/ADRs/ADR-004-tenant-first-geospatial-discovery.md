# ADR-004: Tenant-first geospatial clinic discovery

Status: Proposed

Persist permitted spatial data as SQL Server geography with SRID 4326 and a spatial index. Resolve patient-authorized accounts before active-clinic, location-quality, radius, and availability predicates. Nearby searches use local SQL distance and never call Google per search. Missing location never falls back to all clinics.

