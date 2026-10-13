---
id: FR-OBS-001
type: requirement
title: "Extracción de Logs, Trazas y Métricas del Servidor para Usuario Mantenedor"
status: draft
version: "1.0.0"
schema-version: "1.0"
category: functional
derives-from:
  - UC-OBSERVABILITY-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@FR-OBS-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Requirement: Extracción de Logs, Trazas y Métricas del Servidor para Usuario Mantenedor

## 1. Normative Statement

El sistema DEBE emitir logs estructurados en formato estándar con granularidad configurable (`DEBUG`, `INFO`, `WARN`, `ERROR`, `SECURITY`), trazas y métricas, permitiendo al rol `ACT-MAINTAINER-001` tanto su extracción directa desde la interfaz web de administración como su envío a un stack de observabilidad externo, y denegando el acceso a cualquier usuario sin privilegios de mantenedor.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-OBS-001 @automated @regression
Feature: Extracción de Logs, Trazas y Métricas del Servidor para Usuario Mantenedor

  Scenario: Usuario mantenedor configura la granularidad y extrae logs estructurados, trazas y métricas
    Given el usuario "ops@escandallo.es" está autenticado con el rol "ACT-MAINTAINER-001"
    When ajusta el nivel de granularidad de logs a "DEBUG" y solicita la extracción de telemetría de las últimas 24 horas
    Then el servidor aplica la granularidad "DEBUG" sin reiniciar el servicio
    And entrega el paquete en formato estándar con los registros de logs estructurados, trazas de ejecución y métricas del sistema

  Scenario Outline: Control de acceso por rol a la configuración de granularidad y extracción de observabilidad
    Given un usuario autenticado con rol "<rol>"
    When realiza una petición sobre el recurso de telemetría "<recurso>"
    Then el servidor responde con código HTTP <codigo_http>

    Examples:
      | rol                   | recurso          | codigo_http |
      | ACT-MAINTAINER-001    | logs             | 200         |
      | ACT-MAINTAINER-001    | traces           | 200         |
      | ACT-MAINTAINER-001    | metrics          | 200         |
      | ACT-MAINTAINER-001    | log_granularity  | 200         |
      | ACT-ESTHETIC-USER-001 | logs             | 403         |
      | ACT-ESTHETIC-USER-001 | log_granularity  | 403         |

  Scenario Outline: Rechazo controlado de niveles de severidad inexistentes o formatos de exportación de telemetría inválidos
    Given el usuario "ops@escandallo.es" está autenticado con el rol "ACT-MAINTAINER-001"
    When solicita configurar el nivel de log "<nivel_solicitado>" o exportar telemetría en formato "<formato_exportacion>"
    Then el servidor responde con código HTTP <codigo_http> y estado de validación "<estado_validacion>"

    Examples:
      | nivel_solicitado | formato_exportacion | codigo_http | estado_validacion          |
      | DEBUG            | JSON                | 200         | APLICADO_Y_EXPORTADO       |
      | SECURITY         | OTLP                | 200         | APLICADO_Y_EXPORTADO       |
      | VERBOSE99        | JSON                | 400         | NIVEL_GRANULARIDAD_INVALIDO|
      | INFO             | EXE                 | 400         | FORMATO_TELEMETRIA_INVALIDO|
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de extracción de logs, trazas y métricas. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Logs estructurados en formato estándar con granularidad configurable y soporte dual (descarga web + stack externo). |
