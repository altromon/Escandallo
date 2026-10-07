# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-obs-001.md
# ID Requerimiento: FR-OBS-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

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
    When solicita configurar el nivel "<nivel_log>" y exportar telemetría en el formato "<formato_exportacion>"
    Then el servidor responde con código HTTP <codigo_http> y estado "<estado_operacion>"

    Examples:
      | nivel_log | formato_exportacion | codigo_http | estado_operacion                  |
      | DEBUG     | JSON                | 200         | EXPORTACION_TELEMETRIA_OK         |
      | SECURITY  | OTLP                | 200         | EXPORTACION_TELEMETRIA_OK         |
      | VERBOSE99 | JSON                | 400         | NIVEL_GRANULARIDAD_NO_SOPORTADO   |
      | INFO      | EXE                 | 400         | FORMATO_TELEMETRIA_NO_SOPORTADO   |
