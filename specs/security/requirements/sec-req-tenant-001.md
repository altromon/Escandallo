---
id: SEC-REQ-TENANT-001
type: security-requirement
title: Autorización Multi-Tenant Zero-Trust, Prevención IDOR/BOLA y Row-Level Security
status: active
version: 1.0.0
schema-version: '1.0'
category: security
priority: critical
derives-from:
  - UC-COST-CATALOG-001
  - UC-EXEC-REPORT-001
mitigates-abuse-case:
  - ABUSE-IDOR-TENANT-001
enforced-in-enclave: SEC-ENC-APP-CORE-001
owasp-asvs: V4.1, V4.2
stride-category: Information Disclosure
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - '@SEC-REQ-TENANT-001'
  - '@automated'
  - '@regression'
supersedes: null
superseded-by: null
---


# Security Requirement: Autorización Multi-Tenant Zero-Trust, Prevención IDOR/BOLA y Row-Level Security

## 1. Normative Statement

El sistema **DEBE** aplicar control de acceso estricto a nivel de objeto y fila (OWASP ASVS V4.1 / V4.2) sobre todas las operaciones de lectura, escritura, exportación de informes (`UC-EXEC-REPORT-001`) y copia de catálogos entre centros (`M4` en `UC-COST-CATALOG-001`):
1. **Validación Dual de Propiedad en Copia Cross-Center (`M4`)**: En cualquier solicitud de clonado de catálogo desde `source_center_id` hacia `target_center_id`, el backend **DEBE** verificar en una única consulta transaccional que el `user_id` autenticado es propietario activo de **ambos** centros antes de leer un solo registro.
2. **Identificadores No Enumerables y Filtrado Forzoso**: Todos los recursos (`center_id`, `cost_id`, `escandallo_id`) **DEBEN** utilizar identificadores opacos (UUIDv4 o ULID) y toda consulta SQL **DEBE** inyectar el predicado de pertenencia al `center_id` autorizado (o políticas RLS equivalentes en PostgreSQL).
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

  Scenario Outline: Bloqueo de acceso IDOR sobre endpoints de costes, escandallos e informes
    Given el usuario autenticado "U_ATACANTE" tiene sesión activa en el centro "C_PROPIO"
    When "U_ATACANTE" invoca la operación "<operacion>" apuntando al recurso "<recurso_ajeno>" del centro "C_AJENO"
    Then el sistema deniega la petición con código HTTP <codigo_http>
    And la respuesta no contiene ningún dato financiero ni nombre comercial de "C_AJENO"
    And se emite una traza de seguridad con nivel "<nivel_log>"

    Examples:
      | operacion                        | recurso_ajeno            | codigo_http | nivel_log |
      | GET /costs                       | center_id=C_AJENO        | 403         | SECURITY  |
      | PATCH /costs/cost-uuid-ajeno     | cost_id=cost-uuid-ajeno  | 404         | SECURITY  |
      | POST /reports/export-pdf         | center_id=C_AJENO        | 403         | SECURITY  |
      | POST /reports/export-xlsx        | center_id=C_AJENO        | 403         | SECURITY  |

  Scenario Outline: Análisis de frontera en validación dual de titularidad al copiar catálogos entre centros (M4)
    Given el usuario autenticado tiene relación "<titularidad_origen>" sobre el centro origen y "<titularidad_destino>" sobre el centro destino
    When ejecuta la solicitud transaccional de clonado de catálogo M4
    Then el guardia multi-tenant responde con código HTTP <codigo_http> y estado "<estado_copia>"

    Examples:
      | titularidad_origen | titularidad_destino | codigo_http | estado_copia               |
      | OWNER              | OWNER               | 200         | CATALOGO_CLONADO_OK        |
      | OWNER              | NONE                | 403         | RECHAZADO_DESTINO_AJENO    |
      | NONE               | OWNER               | 403         | RECHAZADO_ORIGEN_AJENO     |
      | NONE               | NONE                | 403         | RECHAZADO_AMBOS_AJENOS     |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del requisito de seguridad anti-IDOR multi-tenant. |


## Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-07 | agent-devops | Implementación verificada (TSK-001, TSK-002, TSK-003, TSK-004, TSK-005, TSK-006, TSK-007) | chg-001-escandallo-mvp |
