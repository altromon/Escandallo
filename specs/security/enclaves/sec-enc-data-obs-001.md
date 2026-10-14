---
id: SEC-ENC-DATA-OBS-001
type: security-enclave
title: "Enclave de Persistencia Multi-Tenant e Infraestructura de Observabilidad"
status: draft
version: "1.1.0"
schema-version: "1.0"
trust-zone: "restricted-data-and-telemetry"
supersedes: null
superseded-by: null
---

# Security Enclave: Enclave de Persistencia Multi-Tenant e Infraestructura de Observabilidad

## 1. Enclave Description & Trust Boundary

Zona restringida en la red Docker aislada sin salida a Internet (`net-data-isolated`, `internal: true`, `ports: []`) que aloja **PostgreSQL 16 Alpine** (`DATA-REL-POSTGRES-001`), el subsistema de telemetría dual **`Serilog` + `OpenTelemetry .NET`** (`CMP-OBS-TELEMETRY-001`) y las copias de seguridad cifradas (`AES-256-GCM`) hacia Object Storage europeo (`RES-STORAGE-BACKUP-001`). Solo es accesible desde `SEC-ENC-APP-CORE-001` y, para la conmutación de nivel en caliente o extracción de telemetría, exclusivamente por sesiones autenticadas con el rol `ACT-MAINTAINER-001`.

## 2. Security Controls Enforced

- **Aislamiento en Pool `Npgsql` y `FORCE ROW LEVEL SECURITY` (`SEC-REQ-TENANT-001`, `RSK-RLS-POOL-LEAK-004`)**: Conexión desde .NET 10 mediante un rol de base de datos sin privilegios `SUPERUSER` ni `BYPASSRLS`, ejecutando exclusivamente `SELECT set_config('app.current_center_id', @centerId::text, true)` (`is_local = true` ligado al ciclo de vida de la transacción) para impedir la contaminación de contexto entre conexiones reutilizadas del pool de `Npgsql`, con política `FORCE ROW LEVEL SECURITY` *fail-closed* si la variable es nula o vacía.
- **Redacción Inmutable en `Serilog` / `OpenTelemetry` y Protección contra Bombas de Destructuración (`SEC-REQ-OBS-001`)**: `PiiAndSecretRedactingPolicy` ofusca con `[REDACTED]` secretos, tokens, cabeceras, cadenas de conexión `Npgsql`, claves `DataProtection` y PII tanto en propiedades estructuradas como en `Exception.Message` incluso en nivel `DEBUG` (`LoggingLevelSwitch`), limitando la profundidad de destructuración (`ToMaximumDepth(3)`) y aislando los eventos `SECURITY` en un canal inmutable inmune a inundación de logs.
- **Integridad Referencial e Inmutabilidad Financiera (`NUMERIC(18,6)`)**: Tablas de histórico (`escandallo_snapshots`, `stripe_webhook_events`) con semántica `append-only` y precisión decimal exacta.

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Definición inicial del enclave de datos y observabilidad. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Auditoría técnica .NET 10 LTS + PostgreSQL 16: prevención de fugas en pool Npgsql (`SET LOCAL` + `NOBYPASSRLS`), límites de destructuración Serilog y buffer inmutable `SECURITY`. |
