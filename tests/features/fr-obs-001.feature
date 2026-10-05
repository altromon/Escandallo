# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-obs-001.md
# ID Requerimiento: FR-OBS-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

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
