---
id: SEC-REQ-TENANT-001
type: security-requirement
title: "Autorización Multi-Tenant Zero-Trust, Prevención IDOR/BOLA y Row-Level Security"
status: draft
version: "1.1.0"
schema-version: "1.0"
category: security
priority: critical
derives-from:
  - UC-COST-CATALOG-001
  - UC-EXEC-REPORT-001
mitigates-abuse-case:
  - ABUSE-IDOR-TENANT-001
enforced-in-enclave: "SEC-ENC-APP-CORE-001"
owasp-asvs: "V4.1, V4.2"
stride-category: "Information Disclosure"
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@SEC-REQ-TENANT-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Security Requirement: Autorización Multi-Tenant Zero-Trust, Prevención IDOR/BOLA y Row-Level Security

## 1. Normative Statement

El sistema **DEBE** aplicar control de acceso estricto a nivel de objeto, circuito Blazor y fila en PostgreSQL 16 (OWASP ASVS V4.1 / V4.2) sobre todas las operaciones de lectura, escritura, exportación de informes (`UC-EXEC-REPORT-001`) y copia de catálogos entre centros (`M4` en `UC-COST-CATALOG-001`):
1. **Revalidación Continua en Blazor SSR / Interactive Server y Copia Dual (`M4`)**: En cualquier evento de componente Razor (`CMP-WEB-UI-001`) o solicitud de clonado `M4` desde `source_center_id` hacia `target_center_id`, `ITenantIsolationGuard` (`CMP-IAM-TENANT-001`) **DEBE** obtener el `userId` exclusivamente del `AuthenticationStateProvider` en servidor y verificar transaccionalmente la titularidad activa sobre los centros implicados (y clonar por valor con nuevos UUIDs), sin confiar jamás en propiedades `[Parameter]` o `[SupplyParameterFromForm]` enviadas por el cliente.
2. **Aislamiento en Pool `Npgsql` con `SET LOCAL` y `FORCE ROW LEVEL SECURITY` (`ADR-003-MULTITENANT-RLS-AND-ATOMIC-QUOTA`, `RSK-RLS-POOL-LEAK-004`)**: Todos los recursos (`center_id`, `cost_id`, `escandallo_id`) **DEBEN** utilizar UUIDv4/ULID, el rol de conexión de la aplicación en PostgreSQL 16 **NO DEBE** poseer `SUPERUSER` ni `BYPASSRLS`, y toda unidad de trabajo **DEBE** fijar `SELECT set_config('app.current_center_id', @centerId::text, true)` (`is_local = true` ligado a la transacción) bajo políticas `ENABLE` + `FORCE ROW LEVEL SECURITY` *fail-closed* para impedir contaminación entre conexiones reutilizadas del pool de `Npgsql`.
3. **Respuesta Opaca Anti-Enumeración y Auditoría**: Cualquier intento de acceso a un recurso perteneciente a otro inquilino **DEBE** ser rechazado con código HTTP `403 Forbidden` o `404 Not Found` sin filtrar metadatos del centro ajeno, emitiendo inmediatamente un evento estructurado de severidad `SECURITY`.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@SEC-REQ-TENANT-001 @automated @regression @security @mitigation
Feature: Prevención de ataques IDOR/BOLA cross-tenant en catálogos e informes
  Como motor de seguridad del núcleo de aplicación (SEC-ENC-APP-CORE-001)
  Quiero validar criptográfica y relacionalmente la propiedad de cada centro de estética
  Para impedir la exfiltración o copia no autorizada de costes y márgenes entre competidores

  Scenario: Rechazo de copia de catálogo M4 desde un centro de estética ajeno
    Given el usuario autenticado "U_ATACANTE" es propietario únicamente del centro "C_PROPIO"
    And existe un centro competidor "C_AJENO" propiedad de otro usuario con 25 cosméticos registrados
    When "U_ATACANTE" solicita clonar el catálogo de productos indicando origen "C_AJENO" y destino "C_PROPIO"
    Then el sistema rechaza la operación con estado HTTP 403
    And el catálogo del centro "C_PROPIO" permanece con 0 productos copiados
    And se registra un evento de auditoría con severidad "SECURITY" y motivo "CROSS_TENANT_IDOR_ATTEMPT"

  Scenario Outline: Bloqueo de acceso IDOR sobre endpoints HTTP, circuitos Blazor y pool Npgsql
    Given el usuario autenticado "U_ATACANTE" tiene sesión activa en el centro "C_PROPIO"
    When "U_ATACANTE" invoca la operación "<operacion>" apuntando al recurso "<recurso_ajeno>" del centro "C_AJENO"
    Then el sistema deniega la petición con código HTTP <codigo_http>
    And la respuesta no contiene ningún dato financiero ni nombre comercial de "C_AJENO"
    And se emite una traza de seguridad con nivel "<nivel_log>"

    Examples:
      | operacion                              | recurso_ajeno            | codigo_http | nivel_log |
      | GET /costs                             | center_id=C_AJENO        | 403         | SECURITY  |
      | PATCH /costs/cost-uuid-ajeno           | cost_id=cost-uuid-ajeno  | 404         | SECURITY  |
      | POST /reports/export-pdf               | center_id=C_AJENO        | 403         | SECURITY  |
      | POST /reports/export-xlsx              | center_id=C_AJENO        | 403         | SECURITY  |
      | WSS /_blazor InvokeSaveEscandallo      | parameter_center=C_AJENO | 403         | SECURITY  |

  Scenario Outline: Análisis de frontera en validación dual de titularidad al copiar catálogos entre centros (M4) y aislamiento SET LOCAL en pool Npgsql
    Given un usuario autenticado posee relación "<relacion_origen>" sobre el centro origen y "<relacion_destino>" sobre el centro destino en una conexión reutilizada del pool Npgsql
    When solicita ejecutar la copia de catálogo M4 entre ambos centros bajo política FORCE ROW LEVEL SECURITY
    Then el servidor responde con código HTTP <codigo_http>

    Examples:
      | relacion_origen | relacion_destino | codigo_http |
      | OWNER           | OWNER            | 200         |
      | OWNER           | NONE             | 403         |
      | NONE            | OWNER            | 403         |
      | NONE            | NONE             | 403         |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del requisito de seguridad anti-IDOR multi-tenant. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Auditoría técnica .NET 10 LTS + Blazor + PostgreSQL 16: revalidación de parámetros en circuitos SignalR y aislamiento `SET LOCAL` + `FORCE RLS` en pool Npgsql. |
