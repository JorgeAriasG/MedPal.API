# ADR-004: Tenant-first geospatial clinic discovery

Status: Accepted

Persist permitted spatial data as SQL Server geography with SRID 4326 and a spatial index. Resolve patient-authorized accounts before active-clinic, location-quality, radius, and availability predicates. Nearby searches use local SQL distance and never call Google per search. Missing location never falls back to all clinics.

Approved decisions: persist Place ID as the canonical clinic geocode; persist exact patient coordinates only on explicit opt-in; provider access via adapter (`IGeocodingProvider`), never from search/booking paths; radius policy per `../Work-Packages/T02-decisions.md` (A2, A4).

