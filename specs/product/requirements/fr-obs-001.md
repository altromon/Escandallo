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
@FR-OBS-001 @automated @regression @security
Feature: Extracción de Logs, Trazas y Métricas del Servidor para Usuario Mantenedor

  Scenario: Usuario mantenedor configura la granularidad y extrae logs estructurados, trazas y métricas
    Given el usuario "ops@escandallo.es" está autenticado con el rol "ACT-MAINTAINER-001"
    When ajusta el nivel de granularidad de logs a "DEBUG" y solicita la extracción de telemetría de las últimas 24 horas
    Then el servidor aplica la granularidad "DEBUG" sin reiniciar el servicio
    And entrega el paquete en formato estándar con los registros de logs estructurados, trazas de ejecución y métricas del sistema

  @boundary
  Scenario Outline: Extracción de telemetría y cambio de niveles de granularidad válidos en los límites de ventana temporal
    Given un usuario autenticado con rol "ACT-MAINTAINER-001"
    When configura el nivel de granularidad "<nivel_log>" y solicita exportar el recurso "<recurso>" con ventana de <horas_ventana> horas
    Then el servidor responde con código HTTP <codigo_http> y formato de salida "<formato_salida>"

    Examples:
      | nivel_log | recurso | horas_ventana | codigo_http | formato_salida |
      | DEBUG     | logs    | 1             | 200         | ndjson         |
      | INFO      | traces  | 24            | 200         | otlp_json      |
      | WARN      | metrics | 168           | 200         | prometheus_csv |
      | ERROR     | logs    | 720           | 200         | ndjson         |
      | SECURITY  | logs    | 720           | 200         | ndjson         |

  @invalid @security
  Scenario Outline: Rechazo de acceso sin privilegios de mantenedor o parámetros de observabilidad fuera de rango
    Given un usuario autenticado con rol "<rol>"
    When realiza una petición sobre el recurso "<recurso>" con nivel "<nivel_log>" y ventana de <horas_ventana> horas
    Then el servidor rechaza la petición con código HTTP <codigo_http> y código de error "<error_code>"

    Examples:
      | rol                   | recurso         | nivel_log     | horas_ventana | codigo_http | error_code                |
      | ACT-ESTHETIC-USER-001 | logs            | INFO          | 24            | 403         | MAINTAINER_ROLE_REQUIRED  |
      | ACT-ESTHETIC-USER-001 | log_granularity | DEBUG         | 24            | 403         | MAINTAINER_ROLE_REQUIRED  |
      | ACT-MAINTAINER-001    | log_granularity | TRACE_VERBOSE | 24            | 400         | INVALID_LOG_LEVEL         |
      | ACT-MAINTAINER-001    | logs            | INFO          | 0             | 422         | INVALID_TIME_WINDOW_RANGE |
      | ACT-MAINTAINER-001    | logs            | INFO          | -24           | 422         | INVALID_TIME_WINDOW_RANGE |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de extracción de logs, trazas y métricas. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Logs estructurados en formato estándar con granularidad configurable y soporte dual (descarga web + stack externo). |
| 1.3.0 | 2026-10-05 | agent-qa-engineer | Adición de casos límite de niveles/ventanas de telemetría y casos inválidos de rol o rango temporal. |

