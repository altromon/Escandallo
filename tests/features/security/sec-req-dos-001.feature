# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/security/requirements/sec-req-dos-001.md
# ID Requerimiento: SEC-REQ-DOS-001
# Versión: 1.1.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@SEC-REQ-DOS-001 @automated @regression @security @mitigation
Feature: Protección contra denegación de servicio asimétrica en recálculos y generación de informes
  Como pasarela perimetral y gestor de recursos (SEC-ENC-EDGE-WAF-001)
  Quiero limitar la tasa de recálculos en cascada, los circuitos SignalR de Blazor y la concurrencia de renderizado PDF/Excel por inquilino
  Para garantizar la estabilidad del servidor compartido frente a vecinos ruidosos y bots

  Scenario: Bloqueo de ráfaga de exportaciones concurrentes de informes PDF por un mismo inquilino
    Given el usuario del centro "C_ACTIVO" tiene 2 exportaciones de informe PDF en curso simultáneamente
    When el mismo usuario envía una tercera petición concurrente de exportación PDF antes de que finalicen las anteriores
    Then el sistema rechaza la petición excedente con estado HTTP 429
    And incluye la cabecera "Retry-After" indicando el tiempo de espera recomendado
    And el consumo de memoria y CPU del resto de centros de estética permanece dentro del umbral operativo

  Scenario Outline: Aplicación de límites de tasa, circuitos SignalR y cardinalidad ante tráfico abusivo
    Given un cliente autenticado en el centro "C_ACTIVO" ejecuta la operación "<operacion_intensiva>"
    When supera el umbral permitido de "<umbral_configurado>" enviando "<volumen_enviado>"
    Then el sistema intercepta el exceso devolviendo el código HTTP <codigo_http>
    And se registra una alerta preventiva con severidad "<severidad_log>"

    Examples:
      | operacion_intensiva                  | umbral_configurado               | volumen_enviado            | codigo_http | severidad_log |
      | actualizacion_coste_con_cascada      | 30 mutaciones por minuto         | 45 mutaciones en 20s       | 429         | WARN          |
      | exportacion_informe_ejecutivo_xlsx   | 10 exportaciones por minuto      | 15 exportaciones en 30s    | 429         | SECURITY      |
      | guardado_receta_escandallo_m3        | 100 lineas maximas por servicio  | 500 lineas en un servicio  | 422         | SECURITY      |
      | apertura_circuitos_blazor_signalr    | 5 circuitos WSS por usuario      | 20 conexiones WSS simult   | 429         | SECURITY      |

  @performance
  Scenario Outline: Análisis de frontera exacta (N-1, N, N+1) en concurrencia de informes (2), mutaciones por minuto (30), líneas de receta (100) y circuitos Blazor WSS (5)
    Given un cliente autenticado en el centro "C_ACTIVO" opera con <informes_concurrentes> informes en curso, <mutaciones_min> mutaciones/min, <lineas_receta> líneas y <circuitos_wss> circuitos WSS
    When envía la solicitud en la frontera de los límites operativos configurados
    Then el servidor responde con código HTTP <codigo_http>

    Examples:
      | informes_concurrentes | mutaciones_min | lineas_receta | circuitos_wss | codigo_http |
      | 1                     | 29             | 99            | 4             | 200         |
      | 2                     | 30             | 100           | 5             | 200         |
      | 3                     | 30             | 100           | 5             | 429         |
      | 2                     | 31             | 100           | 5             | 429         |
      | 2                     | 30             | 101           | 5             | 422         |
      | 2                     | 30             | 100           | 6             | 429         |
